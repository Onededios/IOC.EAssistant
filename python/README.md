# Python Module - IOC.EAssistant

This module contains the Python components for the IOC.EAssistant project, including a web crawler, document vectorization, and a stateless RAG (Retrieval-Augmented Generation) API that integrates with the .NET backend.

## Installation

### Prerequisites
- Python 3.7 or higher
- pip package manager

### Install requiments

```bash
cd python
pip install -r requirements.txt
```

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

**Data Storage**: Crawled data is stored in the `data/` folder with filenames based on the source URL.

### Document Vectorization

Before using the RAG API, you need to vectorize the crawled documents:

```bash
# Vectorize documents and store in ChromaDB
python vectorize_documents.py
```

This will:
- Process JSON files from the `data/` directory
- Generate embeddings
- Store vectors in ChromaDB at `./chroma_db`

### Web API with RAG Agent

The web API provides a **stateless** RESTful interface for the IOC.EAssistant chatbot powered by RAG (Retrieval-Augmented Generation). The API uses LangChain with llm models for embeddings and chat completion, and ChromaDB for vector storage.

#### Prerequisites

Before running the web API, you need to choose a model provider and set up the necessary dependencies.

##### Model Provider Options

The system supports two model providers:

**OpenAI (Default - Cloud, API Key Required)**
- ✅ High quality responses
- ✅ Fast processing
- ✅ No local GPU needed
- ⚠️ Requires API key and usage fees
- ⚠️ Data sent to OpenAI servers

**Ollama (Alternative - Local, Free)**
- ✅ Completely free
- ✅ Full privacy (runs locally)
- ✅ No usage limits
- ⚠️ Requires GPU for best performance
- ⚠️ Slower than cloud models

##### Setup Instructions

**Option 1: Using OpenAI (Default)**

1. **Get an OpenAI API key** from [OpenAI Platform](https://platform.openai.com/)

2. **Create a `.env` file** in the `python/` directory:
   ```env
   MODEL_PROVIDER=openai
   OPENAI_API_KEY=sk-your-api-key-here
   EMBEDDING_MODEL=text-embedding-3-small
   LLM_MODEL=gpt-4o-mini
   ```

   Available OpenAI models:
   - **Embeddings**: `text-embedding-3-small` (recommended), `text-embedding-3-large`, `text-embedding-ada-002`
   - **LLM**: `gpt-4o-mini` (recommended, cost-effective), `gpt-4o` (highest quality), `gpt-3.5-turbo` (fastest)

3. **Vectorize documents**:
   ```bash
   python vectorize_documents.py
   ```

**Option 2: Using Ollama (Local)**

1. **Install Ollama** from [ollama.ai](https://ollama.ai/) and pull the required models:
   ```bash
   # Install embedding model
   ollama pull nomic-embed-text
   
   # Install LLM model
   ollama pull llama3.2
   ```

2. **Create a `.env` file** in the `python/` directory:
   ```env
   MODEL_PROVIDER=ollama
   EMBEDDING_MODEL=nomic-embed-text
   LLM_MODEL=llama3.2
   ```

3. **Vectorize documents**:
   ```bash
   python vectorize_documents.py
   ```

#### Running the Web API

```bash
# Start the web server
python web.py
```

The server will start on `http://localhost:8000` and provide:
- **Swagger UI**: `http://localhost:8000/apidocs/` for interactive API documentation
- **RESTful API**: Endpoints for RAG-powered chat with conversation history

#### API Endpoints

##### 1. Health Check
```bash
GET /health
```
Check if the service is running and get model information.

**Example:**
```bash
curl http://localhost:8000/health
```

**Response:**
```json
{
  "status": "healthy",
  "model": "llama3.2",
  "timestamp": "2025-11-11T12:34:56"
}
```

##### 2. Chat with RAG Agent (with Conversation History)
```bash
POST /chat
```
Send a question to the RAG-powered chatbot with full conversation history. The .NET backend sends the complete conversation context with each request.

**Request Body:**
```json
{
  "messages": [
    {
      "index": 0,
      "question": "Què és l'IOC?",
      "answer": "L'IOC és l'Institut Obert de Catalunya, una institució educativa..."
    },
    {
      "index": 1,
      "question": "Com em puc matricular?",
      "answer": ""
    }
  ],
  "modelConfig": {
    "temperature": 0.7
  },
  "metadata": {
    "locale": "ca-ES"
  }
}
```

**Response (OpenAI-compatible format):**
```json
{
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "L'IOC és l'Institut Obert de Catalunya..."
      },
      "finishReason": "stop"
    }
  ],
  "usage": {
    "promptTokens": 42,
    "completionTokens": 38,
    "totalTokens": 80
  },
  "metadata": {
    "modelVersion": "llama3.2",
    "processingTime": 1523
  }
}
```


#### Switching Between Providers

You can easily switch between OpenAI and Ollama by changing the `MODEL_PROVIDER` environment variable in your `.env` file:

**For OpenAI (Default):**
```env
MODEL_PROVIDER=openai
OPENAI_API_KEY=sk-your-api-key-here
EMBEDDING_MODEL=text-embedding-3-small
LLM_MODEL=gpt-4o-mini
```

**For Ollama (Local):**
```env
MODEL_PROVIDER=ollama
EMBEDDING_MODEL=nomic-embed-text
LLM_MODEL=llama3.2
```

**⚠️ Important**: When you change the embedding model or provider, you **must re-vectorize your documents**:
```bash
python vectorize_documents.py
```
This is required because embeddings from different models are not compatible.

#### Architecture

The web API integrates several components:

- **Flask**: Web framework for API endpoints
- **Flasgger**: Swagger/OpenAPI documentation
- **RAGAgent**: Custom agent using LangChain for RAG implementation
- **ChromaDB**: Vector database for document embeddings
- **Model Providers**: 
  - **OpenAI** (default): Cloud-based models with high quality responses
  - **Ollama** (alternative): Local LLM and embedding models (free, runs locally)
- **LangChain**: Agent framework with tool calling for dynamic retrieval

## Project Structure

```
python/
├── crawler.py                 # Web crawler for IOC education portal
├── rag_agent.py               # RAG Agent with LangChain (stateless)
├── utils.py                   # Utility functions (GPU config, formatting)
├── vectorize_documents.py     # Document vectorization script
├── web.py                     # Flask API server (stateless)
├── requirements.txt           # Python dependencies
├── README.md                  # This file
├── data/                      # Crawled data storage (JSON files)
└── chroma_db/                 # ChromaDB vector storage
```
