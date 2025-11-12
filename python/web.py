from flask import Flask, request, jsonify
from flasgger import Swagger
from rag_agent import RAGAgent
import os
from datetime import datetime
import tiktoken

# --- Flask + Swagger setup ---
app = Flask(__name__)
swagger = Swagger(app)

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


def count_tokens(text: str, model: str = None) -> int:
    """
    Count tokens in text using the appropriate method based on provider.
    
    For OpenAI: Uses tiktoken for accurate token counting
    For Ollama: Uses word-based approximation (1 token ≈ 0.75 words)
    
    Args:
        text: The text to count tokens for
        model: Optional model name override
        
    Returns:
        Estimated token count
    """
    if not text:
        return 0
    
    # Use the model from RAG agent if not provided
    if model is None:
        model = rag_agent.llm.model
    
    # For OpenAI models, use tiktoken for accurate counting
    if provider.lower() == "openai":
        try:
            # Try to get encoding for the specific model
            encoding = tiktoken.encoding_for_model(model)
        except KeyError:
            # Fallback to cl100k_base encoding (used by gpt-4, gpt-3.5-turbo, text-embedding-ada-002)
            encoding = tiktoken.get_encoding("cl100k_base")
        
        return len(encoding.encode(text))
    
    # For Ollama models, use word-based approximation
    # Research suggests approximately 1 token per 0.75 words for most models
    # This is a conservative estimate that tends to slightly overestimate
    else:
        word_count = len(text.split())
        return int(word_count / 0.75)


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
        
        # Count tokens accurately based on provider
        # For prompt tokens: count all conversation history + current question
        prompt_text = ""
        for q, a in conversation_history:
            prompt_text += q + " " + a + " "
        prompt_text += current_question
        
        prompt_tokens = count_tokens(prompt_text)
        completion_tokens = count_tokens(answer)
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
        return jsonify({"error": str(e)}), 500


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
    app.run(host="0.0.0.0", port=8000, debug=True)
