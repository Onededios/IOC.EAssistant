# Python Module - IOC.EAssistant

This module contains the Python components for the IOC.EAssistant project, including a web crawler, Loki logger integration, and web API.

## Installation

### Prerequisites
- Python 3.7 or higher
- pip package manager

### Setup with Virtual Environment (Recommended for Local Development)

It's recommended to use a virtual environment to isolate project dependencies:

```bash
# Navigate to the python folder
cd python

# Create a virtual environment
python -m venv venv

# Activate the virtual environment
# On Windows (PowerShell):
.\venv\Scripts\Activate.ps1

# On Windows (Command Prompt):
.\venv\Scripts\activate.bat

# On Linux/Mac:
source venv/bin/activate

# Install required packages
pip install -r requirements.txt
```

### Install Without Virtual Environment

If you prefer to install globally:

```bash
cd python
pip install -r requirements.txt
```

## Configuration

### Loki Logger Configuration

Before using the Loki logger, you need to configure your Loki server credentials:

1. Copy the example configuration file:
   ```bash
   cp loki_config.example.py loki_config.py
   ```

2. Edit `loki_config.py` with your Loki server details:
   ```python
   # Loki server URL
   LOKI_URL = "http://your-loki-server:3100"
   
   # Authentication credentials
   USER_ID = "your_user_id"
   API_KEY = "your_api_key"
   ```

3. Save the file. The `loki_logger.py` will automatically use these settings.

**Note**: Make sure `loki_config.py` is added to `.gitignore` to avoid committing sensitive credentials.

## Usage

### Crawler

The crawler module is used to scrape and process data from IOC websites:

```bash
# Run the crawler
python crawler.py
```

The crawler will:
- Fetch latest news and updates from IOC education portal
- Save the data in JSON format to the `data/` directory
- Log activities using the Loki logger (if configured)

**Data Storage**: Crawled data is stored in the `data/` folder with filenames based on the source URL.

### Loki Logger

The Loki logger can be used independently in your Python scripts:

```python
from loki_logger import LokiLogger

# Initialize logger (uses configuration from loki_config.py)
logger = LokiLogger()

# Send a log message
logger.send_log(
    message="Your log message here",
    labels={
        "job": "my_job",
        "environment": "production",
        "level": "info"
    }
)

# Check if logger is properly configured
if logger.isConfigured:
    print("Logger is ready to use")
else:
    print("Logger needs configuration")
```

### Web API with RAG Agent

The web API provides a RESTful interface for interacting with the IOC.EAssistant chatbot powered by RAG (Retrieval-Augmented Generation). The API uses LangChain with Ollama models for embeddings and chat completion, ChromaDB for vector storage, and maintains conversation history in SQLite.

#### Prerequisites

Before running the web API, ensure you have:

1. **Ollama installed** with the required models:
   ```bash
   # Install embedding model
   ollama pull nomic-embed-text
   
   # Install LLM model
   ollama pull llama3.2
   
   # Optional: Install reranker model for better precision
   ollama pull qllama/bge-reranker-large
   ```

2. **Vectorized documents** in ChromaDB:
   ```bash
   # Run the vectorization script first
   python vectorize_documents.py
   ```

#### Configuration

Create a `.env` file in the python directory (use `.env.example` as template):

```bash
# ChromaDB Configuration
CHROMA_DB_PATH=./chroma_db
COLLECTION_NAME=ioc_data

# Ollama Models
EMBEDDING_MODEL=nomic-embed-text
LLM_MODEL=llama3.2

# LLM Settings
LLM_TEMPERATURE=0
K_RESULTS=4

# Database
DB_PATH=./histories.db

# GPU Settings (optional)
OLLAMA_NUM_GPU=1
CUDA_VISIBLE_DEVICES=0
```

#### Running the Web API

```bash
# Start the web server
python web.py
```

The server will start on `http://localhost:8000` and provide:
- **Swagger UI**: `http://localhost:8000/apidocs/` for interactive API documentation
- **RESTful API**: Multiple endpoints for chatbot interaction and document search

#### API Endpoints

##### 1. Health Check
```bash
GET /health
```
Check if the service is running.

**Example:**
```bash
curl http://localhost:8000/health
```

**Response:**
```json
{
  "status": "healthy",
  "service": "IOC.EAssistant",
  "timestamp": "2025-10-30T12:34:56"
}
```

##### 2. Chat with RAG Agent
```bash
POST /chat
```
Send a question to the RAG-powered chatbot with conversation history support.

**Request Body:**
```json
{
  "query": "Què és l'IOC?",
  "user_id": "student1"
}
```

**Example:**
```bash
curl -X POST http://localhost:8000/chat \
  -H "Content-Type: application/json" \
  -d '{"query": "Què és l'\''IOC?", "user_id": "student1"}'
```

**Response:**
```json
{
  "answer": "L'IOC és l'Institut Obert de Catalunya...",
  "user_id": "student1",
  "timestamp": "2025-10-30T12:34:56"
}
```

##### 3. Get Conversation History
```bash
GET /history/<user_id>?limit=10
```
Retrieve conversation history for a specific user.

**Example:**
```bash
curl http://localhost:8000/history/student1?limit=5
```

**Response:**
```json
{
  "user_id": "student1",
  "history": [
    {
      "question": "Què és l'IOC?",
      "answer": "L'IOC és l'Institut Obert de Catalunya...",
      "timestamp": "2025-10-30T12:34:56"
    }
  ]
}
```

##### 4. Delete Conversation History
```bash
DELETE /history/<user_id>
```
Delete all conversation history for a specific user.

**Example:**
```bash
curl -X DELETE http://localhost:8000/history/student1
```

**Response:**
```json
{
  "message": "History deleted successfully",
  "user_id": "student1"
}
```

##### 5. Search Documents
```bash
POST /search
```
Direct search in the vector store without LLM processing.

**Request Body:**
```json
{
  "query": "formació professional",
  "k": 4,
  "filter": {"type": "noticias"}
}
```

**Example:**
```bash
curl -X POST http://localhost:8000/search \
  -H "Content-Type: application/json" \
  -d '{"query": "formació professional", "k": 3}'
```

**Response:**
```json
{
  "query": "formació professional",
  "results": [
    {
      "content": "Document content here...",
      "metadata": {
        "title": "Document title",
        "type": "noticias",
        "date": "2024-01-15"
      }
    }
  ]
}
```

#### Testing the API

Run the automated test suite:

```bash
# Make sure the web server is running first
python test_web.py
```

This will test all endpoints and provide a summary of results.

#### Architecture

The web API integrates several components:

- **Flask**: Web framework for API endpoints
- **Flasgger**: Swagger/OpenAPI documentation
- **RAGAgent**: Custom agent using LangChain for RAG implementation
- **ChromaDB**: Vector database for document embeddings
- **Ollama**: Local LLM and embedding models
- **SQLite**: Conversation history persistence

#### Key Features

1. **Conversation History**: Maintains user-specific conversation history in SQLite
2. **RAG Pipeline**: Retrieves relevant documents and generates contextual answers
3. **Reranking**: Uses BGE reranker for improved document relevance
4. **Multi-user Support**: Each user has isolated conversation history
5. **Metadata Filtering**: Search documents by metadata (type, date, etc.)
6. **Interactive Documentation**: Swagger UI for easy API testing

## Project Structure

```
python/
├── crawler.py                 # Web crawler for IOC education portal
├── loki_logger.py             # Loki logging integration
├── loki_config.example.py     # Example configuration file
├── rag_agent.py               # RAG Agent implementation with LangChain
├── utils.py                   # Utility functions (database, formatting)
├── vectorize_documents.py     # Document vectorization script
├── web.py                     # Web API server with RAG integration
├── test_web.py                # API testing script
├── requirements.txt           # Python dependencies
├── .env.example               # Environment variables template
├── data/                      # Crawled data storage (JSON files)
├── chroma_db/                 # ChromaDB vector storage
└── README.md                  # This file
```

## Troubleshooting

### Loki Logger Not Working
- Verify that `loki_config.py` exists and contains valid credentials
- Check that your Loki server is accessible
- Ensure the `isConfigured` property is `True` after initialization

### Import Errors
- Make sure all dependencies are installed: `pip install -r requirements.txt`
- Verify you're using Python 3.7 or higher: `python --version`
- If using a virtual environment, ensure it's activated

### Data Directory Issues
- The `data/` directory should be created automatically by the crawler
- Ensure you have write permissions in the python folder

## Future Development

- [ ] Complete Web API documentation
- [ ] Add API endpoint examples
- [ ] Implement authentication for API
- [ ] Add automated testing
- [ ] Improve error handling and logging

## Contributing

Please refer to the main project's CONTRIBUTING.md file for contribution guidelines.

## License

See the LICENSE file in the root directory of the project.
