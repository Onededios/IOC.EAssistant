from flask import Flask, request, jsonify
from flasgger import Swagger

# --- Flask + Swagger setup ---
app = Flask(__name__)
swagger = Swagger(app)


@app.route("/chat", methods=["POST"])
def chat():
    """
    Chat with IOC.EAssistant chatbot
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
              example: "What is IOC?"
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
              example: "IOC is the Institut Obert de Catalunya..."
            history:
              type: array
              items:
                type: array
                items:
                  type: string
    """
    data = request.get_json(force=True)
    query = data.get("query", "")
    user_id = data.get("user_id", "default")

    if not query:
        return jsonify({"error": "query field required"}), 400
    
    
    # Dummy response for demonstration purposes
    answer = f"Echo: {query}"
    history = [[f"user: {query}", f"bot: {answer}"]]
    return jsonify({"answer": answer, "history": history})


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8000, debug=True)
