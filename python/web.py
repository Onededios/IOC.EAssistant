from flask import Flask, request, jsonify
from flasgger import Swagger
from rag_agent import RAGAgent
import os
from datetime import datetime

# --- Flask + Swagger setup ---
app = Flask(__name__)
swagger = Swagger(app)

# --- Initialize RAG Agent ---
print("Initializing RAG Agent...")
rag_agent = RAGAgent(
    persist_directory=os.getenv("CHROMA_DB_PATH", "./chroma_db"),
    collection_name=os.getenv("COLLECTION_NAME", "ioc_data"),
    embedding_model=os.getenv("EMBEDDING_MODEL", "nomic-embed-text"),
    llm_model=os.getenv("LLM_MODEL", "llama3.2"),
    temperature=float(os.getenv("LLM_TEMPERATURE", "0")),
    k_results=int(os.getenv("K_RESULTS", "4")),
    db_path=os.getenv("DB_PATH", "./histories.db")
)
print("RAG Agent initialized successfully!")


@app.route("/chat", methods=["POST"])
def chat():
    """
    Chat with IOC.EAssistant chatbot using RAG
    ---
    parameters:
      - in: body
        name: body
        required: true
        schema:
          type: object
          required:
            - query
          properties:
            query:
              type: string
              example: "Què és l'IOC?"
            user_id:
              type: string
              example: "student1"
    responses:
      200:
        description: Chatbot answer with history
        schema:
          type: object
          properties:
            answer:
              type: string
              example: "L'IOC és l'Institut Obert de Catalunya..."
            user_id:
              type: string
              example: "student1"
            timestamp:
              type: string
              example: "2025-10-30T12:34:56"
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
        query = data.get("query", "").strip()
        user_id = data.get("user_id", "default")

        if not query:
            return jsonify({"error": "query field required"}), 400
        
        # Get answer from RAG agent
        answer = rag_agent.query(question=query, user_id=user_id, verbose=False)
        
        return jsonify({
            "answer": answer,
            "user_id": user_id,
            "timestamp": datetime.now().isoformat()
        })
    
    except Exception as e:
        print(f"Error in /chat: {str(e)}")
        return jsonify({"error": str(e)}), 500


@app.route("/history/<user_id>", methods=["GET"])
def get_history(user_id):
    """
    Get conversation history for a specific user
    ---
    parameters:
      - in: path
        name: user_id
        type: string
        required: true
        description: User identifier
      - in: query
        name: limit
        type: integer
        default: 10
        description: Maximum number of conversations to retrieve
    responses:
      200:
        description: User conversation history
        schema:
          type: object
          properties:
            user_id:
              type: string
            history:
              type: array
              items:
                type: object
                properties:
                  question:
                    type: string
                  answer:
                    type: string
                  timestamp:
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
        limit = request.args.get("limit", default=10, type=int)
        history = rag_agent.get_user_conversation_history(user_id, limit=limit)
        
        return jsonify({
            "user_id": user_id,
            "history": history
        })
    
    except Exception as e:
        print(f"Error in /history: {str(e)}")
        return jsonify({"error": str(e)}), 500


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8000, debug=True)
