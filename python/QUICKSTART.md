# Quick Start Guide - IOC.EAssistant Web API

This guide will help you get the IOC.EAssistant web API up and running quickly.

## Prerequisites Checklist

- [ ] Python 3.7 or higher installed
- [ ] Ollama installed and running
- [ ] Required Ollama models downloaded
- [ ] Virtual environment created (recommended)

## Step-by-Step Setup

### 1. Install Ollama and Models

```bash
# Download and install Ollama from https://ollama.ai/

# Pull required models
ollama pull nomic-embed-text
ollama pull llama3.2

# Optional: Install reranker for better precision
ollama pull qllama/bge-reranker-large
```

### 2. Set Up Python Environment

```powershell
# Navigate to the python directory
cd python

# Create virtual environment
python -m venv venv

# Activate virtual environment (PowerShell)
.\venv\Scripts\Activate.ps1

# Install dependencies
pip install -r requirements.txt
```

### 3. Prepare Your Data

```powershell
# Run the crawler to fetch IOC data
python crawler.py

# Vectorize the documents into ChromaDB
python vectorize_documents.py
```

### 4. Configure Environment Variables (Optional)

```powershell
# Copy the example env file
copy .env.example .env

# Edit .env with your preferred settings (or use defaults)
# Default settings work fine for most cases
```

### 5. Start the Web Server

```powershell
# Run the web API
python web.py
```

The server will start at `http://localhost:8000`

### 6. Test the API

Open another terminal and run:

```powershell
# Run automated tests
python test_web.py
```

Or visit the Swagger UI in your browser:
```
http://localhost:8000/apidocs/
```

## Quick Test with cURL

### Health Check
```bash
curl http://localhost:8000/health
```

### Ask a Question
```bash
curl -X POST http://localhost:8000/chat \
  -H "Content-Type: application/json" \
  -d "{\"query\": \"Què és l'IOC?\", \"user_id\": \"test_user\"}"
```

### Search Documents
```bash
curl -X POST http://localhost:8000/search \
  -H "Content-Type: application/json" \
  -d "{\"query\": \"formació professional\", \"k\": 3}"
```

## Troubleshooting

### "Ollama not found" error
- Make sure Ollama is installed and running
- Verify with: `ollama list`

### "No module named 'langchain'" error
- Activate your virtual environment
- Run: `pip install -r requirements.txt`

### "ChromaDB not found" error
- Run `python vectorize_documents.py` to create the vector database

### Port 8000 already in use
- Stop any other process using port 8000
- Or change the port in `web.py`: `app.run(host="0.0.0.0", port=8080, debug=True)`

### Models taking too long to respond
- First request is slower (model loading)
- Subsequent requests are faster
- Consider using a smaller model like `llama3.2:1b` for faster responses

## Next Steps

- Explore the Swagger UI at `http://localhost:8000/apidocs/`
- Build a frontend application that consumes the API
- Customize the RAG agent in `rag_agent.py`
- Add more data sources with the crawler
- Adjust the number of retrieved documents (`K_RESULTS` in `.env`)

## Getting Help

If you encounter issues:
1. Check the terminal output for error messages
2. Verify all prerequisites are installed
3. Review the main README.md for detailed documentation
4. Check that Ollama is running: `ollama list`

Happy chatting! 🚀
