from flask import Flask, request, jsonify
from flasgger import Swagger
from rag_agent import RAGAgent
import os
import sys
import subprocess
import asyncio
from datetime import datetime

# --- Flask + Swagger setup ---
app = Flask(__name__)
swagger = Swagger(app)


def check_and_setup_data():
    """
    Check if necessary data and database exist.
    If not, run crawler and vectorize_documents.
    """
    data_dir = os.getenv("DATA_PATH", "./data")
    chroma_db_dir = os.getenv("CHROMA_DB_PATH", "./chroma_db")
    
    # Check if data folder exists and has JSON files
    data_exists = False
    if os.path.exists(data_dir):
        json_files = [f for f in os.listdir(data_dir) if f.endswith('.json')]
        data_exists = len(json_files) > 0
    
    # Check if ChromaDB exists
    chroma_db_exists = os.path.exists(chroma_db_dir) and os.path.exists(os.path.join(chroma_db_dir, "chroma.sqlite3"))
    
    # If data doesn't exist, run crawler
    if not data_exists:
      print("crawling data...")
      subprocess.run(
          [sys.executable, "crawler.py"],
          cwd=os.path.dirname(os.path.abspath(__file__)),
          capture_output=True,
          text=True,
          timeout=600  # 10 minutes timeout
      )
    
    # If ChromaDB doesn't exist, run vectorize_documents
    if not chroma_db_exists:
      print("vectorizing documents...")
      subprocess.run(
          [sys.executable, "vectorize_documents.py"],
          cwd=os.path.dirname(os.path.abspath(__file__)),
          capture_output=True,
          text=True,
          timeout=600  # 10 minutes timeout
      )

# --- Check and setup data before initializing RAG Agent ---
print("Checking prerequisites...")
check_and_setup_data()

# --- Initialize RAG Agent ---
print("Initializing RAG Agent...")
provider = os.getenv("MODEL_PROVIDER", "openai")  # Default to openai

# Set default models based on provider
if provider.lower() == "openai":
    default_embedding = "text-embedding-3-small"
    default_llm = "gpt-4o-mini"
else:
    default_embedding = "nomic-embed-text"
    default_llm = "llama3.2"

rag_agent = RAGAgent(
    persist_directory=os.getenv("CHROMA_DB_PATH", "./chroma_db"),
    collection_name=os.getenv("COLLECTION_NAME", "ioc_data"),
    embedding_model=os.getenv("EMBEDDING_MODEL", default_embedding),
    llm_model=os.getenv("LLM_MODEL", default_llm),
    provider=provider,
    temperature=float(os.getenv("LLM_TEMPERATURE", "0")),
    k_results=int(os.getenv("K_RESULTS", "4"))
)
print("RAG Agent initialized successfully!")


@app.route("/chat", methods=["POST"])
def chat():
    """
    Chat with IOC.EAssistant chatbot using RAG with conversation history
    ---
    parameters:
      - in: body
        name: body
        required: true
        schema:
          type: object
          required:
            - messages
          properties:
            messages:
              type: array
              items:
                type: object
                properties:
                  index:
                    type: integer
                  question:
                    type: string
                  answer:
                    type: string
              example:
                - index: 0
                  question: "Què és l'IOC?"
                  answer: "L'IOC és l'Institut Obert de Catalunya..."
                - index: 1
                  question: "Com em puc matricular?"
                  answer: ""
            modelConfig:
              type: object
              properties:
                temperature:
                  type: number
                  example: 0.7
            metadata:
              type: object
              properties:
                locale:
                  type: string
                  example: "ca-ES"
    responses:
      200:
        description: Chatbot answer
        schema:
          type: object
          properties:
            choices:
              type: array
              items:
                type: object
                properties:
                  index:
                    type: integer
                  message:
                    type: object
                    properties:
                      role:
                        type: string
                      content:
                        type: string
                  finishReason:
                    type: string
            usage:
              type: object
              properties:
                promptTokens:
                  type: integer
                completionTokens:
                  type: integer
                totalTokens:
                  type: integer
            metadata:
              type: object
      400:
        description: Bad request
        schema:
          type: object
          properties:
            error:
              type: string
      500:
        description: Internal server error
        schema:
          type: object
          properties:
            error:
              type: string
    """
    try:
        data = request.get_json(force=True)
        messages = data.get("messages", [])
        model_config = data.get("modelConfig", {})
        metadata = data.get("metadata", {})

        if not messages:
            return jsonify({"error": "messages field required"}), 400
        
        # Get the last message (current question)
        last_message = messages[-1]
        current_question = last_message.get("question", "").strip()
        
        if not current_question:
            return jsonify({"error": "Last message must contain a question"}), 400
        
        # Build conversation history from previous messages (excluding the last one)
        conversation_history = []
        for msg in messages[:-1]:
            question = msg.get("question", "")
            answer = msg.get("answer", "")
            if question and answer:
                conversation_history.append((question, answer))
        
        # Get temperature from modelConfig if provided
        temperature = model_config.get("temperature")
        
        # Get answer from RAG agent with history
        start_time = datetime.now()
        answer = rag_agent.query_with_history(
            question=current_question,
            conversation_history=conversation_history,
            temperature=temperature,
            verbose=False
        )
        end_time = datetime.now()
        processing_time = int((end_time - start_time).total_seconds() * 1000)
        
        # Estimate token usage (rough approximation)
        prompt_tokens = sum(len(q.split()) + len(a.split()) for q, a in conversation_history) + len(current_question.split())
        completion_tokens = len(answer.split())
        total_tokens = prompt_tokens + completion_tokens
        
        # Return response in the expected format
        return jsonify({
            "choices": [
                {
                    "index": 0,
                    "message": {
                        "role": "assistant",
                        "content": answer
                    },
                    "finishReason": "stop"
                }
            ],
            "usage": {
                "promptTokens": prompt_tokens,
                "completionTokens": completion_tokens,
                "totalTokens": total_tokens
            },
            "metadata": {
                "modelVersion": rag_agent.llm.model,
                "processingTime": processing_time
            }
        })
    
    except Exception as e:
        print(f"Error in /chat: {str(e)}")
        return jsonify({"error": "An internal server error occurred. Please try again later."}), 500


@app.route("/health", methods=["GET"])
def health():
    """
    Health check endpoint
    ---
    responses:
      200:
        description: Service is healthy
        schema:
          type: object
          properties:
            status:
              type: string
              example: "healthy"
            model:
              type: string
              example: "llama3.2"
            timestamp:
              type: string
    """
    return jsonify({
        "status": "healthy",
        "model": rag_agent.llm.model,
        "timestamp": datetime.now().isoformat()
    })


if __name__ == "__main__":
    from waitress import serve
    serve(app, host="0.0.0.0", port=8080)