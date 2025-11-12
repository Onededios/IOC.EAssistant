"""
Document Vectorization Script - OPTIMIZED VERSION
Converts documents to vectors and persists them to ChromaDB with enhanced metadata
"""
from langchain_ollama import OllamaEmbeddings
from langchain_openai import OpenAIEmbeddings
from langchain_chroma import Chroma
from langchain_classic.text_splitter import RecursiveCharacterTextSplitter
from langchain_core.documents import Document
from pathlib import Path
from typing import List
from dotenv import load_dotenv
import json
import os
import re
from utils import configure_gpu_settings


load_dotenv()

# Get provider from environment
PROVIDER = os.getenv("MODEL_PROVIDER", "openai").lower()

# Configure GPU usage for Ollama (automatically falls back to CPU if no GPU available)
if PROVIDER == "ollama":
    num_gpus = configure_gpu_settings(num_gpu=1, cuda_device=0)


def extract_metadata_from_filename(filename: str) -> dict:
    """
    Extract metadata from filename pattern:
    https__ioc.xtec.cat_educacio_20-latest-news_1111-adjudicacio-places-fp-curs-2022-23-semestre-1.json
    """
    metadata = {}
    
    # Remove .json extension
    name = filename.replace('.json', '')
    
    # Extract URL
    url = name.replace('_', '/', 1).replace('_', '/', 1).replace('_', '.', 1)
    url = url.replace('_', '/')
    metadata['source_url'] = f"https://{url.split('https__')[1] if 'https__' in url else url}"
    
    # Extract ID from filename (e.g., 1111)
    id_match = re.search(r'_(\d+)-', name)
    if id_match:
        metadata['article_id'] = id_match.group(1)
    
    # Extract topic from filename
    topic_match = re.search(r'\d+-(.*?)$', name)
    if topic_match:
        topic = topic_match.group(1).replace('-', ' ').title()
        metadata['topic'] = topic
    
    metadata['source_file'] = filename
    
    return metadata


def extract_date_from_content(content: str) -> str:
    """Extract date from content if present"""
    # Pattern: DILLUNS, 14 JUNY 2021 or similar
    date_patterns = [
        r'(\d{1,2}\s+(?:GENER|FEBRER|MARÇ|ABRIL|MAIG|JUNY|JULIOL|AGOST|SETEMBRE|OCTUBRE|NOVEMBRE|DESEMBRE)\s+\d{4})',
        r'(\d{1,2}/\d{1,2}/\d{4})',
        r'(\d{4}-\d{2}-\d{2})'
    ]
    
    for pattern in date_patterns:
        match = re.search(pattern, content, re.IGNORECASE)
        if match:
            return match.group(1)
    
    return None


def extract_category_from_content(content: str) -> str:
    """Extract category from content (e.g., NOTÍCIES, MATRÍCULES)"""
    # Look for common categories in uppercase
    categories = ['NOTÍCIES', 'MATRÍCULES', 'BEQUES', 'CONVOCATÒRIES', 'PREINSCRIPCIÓ', 
                  'CALENDARI', 'EXÀMENS', 'FP', 'ESO', 'BATXILLERAT']
    
    for category in categories:
        if category in content.upper():
            return category
    
    return 'GENERAL'


def load_documents(folder_path: str) -> List[Document]:
    """
    Load documents from a folder containing JSON files with enhanced metadata extraction
    """
    documents = []
    
    for filename in os.listdir(folder_path):
        if not filename.endswith('.json'):
            print(f"Skipping unsupported file type: {filename}")
            continue
            
        file_path = os.path.join(folder_path, filename)
        
        try:
            # Load JSON with proper encoding
            with open(file_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
            
            # Extract base metadata from filename
            metadata = extract_metadata_from_filename(filename)
            
            # Get title and content
            title = data.get('title', 'Sense títol')
            content = data.get('content', '')
            
            if not content:
                print(f"Warning: Empty content in {filename}")
                continue
            
            # Add title to metadata
            metadata['title'] = title
            
            # Add type from JSON (noticia or general)
            doc_type = data.get('type', 'general')
            metadata['type'] = doc_type
            
            # Extract date from content
            date = extract_date_from_content(content)
            if date:
                metadata['date'] = date
            
            # Extract category
            category = extract_category_from_content(content)
            metadata['category'] = category
            
            # Create enriched content with title
            enriched_content = f"Títol: {title}\n\n{content}"
            
            # Create document with enriched metadata
            doc = Document(
                page_content=enriched_content,
                metadata=metadata
            )
            
            documents.append(doc)
            
        except Exception as e:
            print(f"Error loading {filename}: {str(e)}")
            continue
    
    print(f"Successfully loaded {len(documents)} documents")
    return documents


def vectorize_and_persist(
    data_folder: str = "./data",
    persist_directory: str = "./chroma_db",
    collection_name: str = "ioc_data",
    chunk_size: int = 800,  # Reduced for better context
    chunk_overlap: int = 150,  # Optimized overlap
    embedding_model: str = "nomic-embed-text"
):
    """
    Load documents, split them, create embeddings, and persist to ChromaDB
    
    Args:
        data_folder: Path to folder containing documents
        persist_directory: Path to persist ChromaDB
        collection_name: Name of the ChromaDB collection
        chunk_size: Size of text chunks (optimized to 800)
        chunk_overlap: Overlap between chunks (optimized to 150)
        embedding_model: Ollama embedding model to use
    """
    print(f"Loading documents from {data_folder}...")
    documents = load_documents(data_folder)
    
    if not documents:
        print("ERROR: No documents loaded!")
        return None
    
    print(f"Loaded {len(documents)} documents")
    
    # Print sample metadata
    if documents:
        print("\nSample document metadata:")
        sample = documents[0]
        print(f"  Title: {sample.metadata.get('title', 'N/A')}")
        print(f"  Type: {sample.metadata.get('type', 'N/A')}")
        print(f"  Category: {sample.metadata.get('category', 'N/A')}")
        print(f"  Date: {sample.metadata.get('date', 'N/A')}")
        print(f"  Content preview: {sample.page_content[:150]}...\n")
    
    print(f"Splitting documents into chunks (size={chunk_size}, overlap={chunk_overlap})...")
    
    # Use optimized text splitter with separators that respect document structure
    text_splitter = RecursiveCharacterTextSplitter.from_tiktoken_encoder(
        chunk_size=700,
        chunk_overlap=120,
        separators=["\n\n", "\n", ". ", " ", ""],
    )
    
    split_docs = text_splitter.split_documents(documents)
    print(f"Created {len(split_docs)} chunks")
    
    # Enhance chunk metadata with position information
    for i, doc in enumerate(split_docs):
        doc.metadata['chunk_id'] = i
        # Add a summary field for better retrieval
        preview = doc.page_content[:200].replace('\n', ' ')
        doc.metadata['preview'] = preview
    
    print(f"Creating embeddings using {embedding_model} with provider {PROVIDER}...")
    
    # Initialize embeddings based on provider
    if PROVIDER == "openai":
        api_key = os.getenv("OPENAI_API_KEY")
        if not api_key:
            raise ValueError("OPENAI_API_KEY not found in environment variables")
        embeddings = OpenAIEmbeddings(
            model=embedding_model,
            openai_api_key=api_key,
        )
    elif PROVIDER == "ollama":
        # Detect GPU availability
        try:
            import torch
            gpu_available = torch.cuda.is_available()
            num_gpu_param = -1 if gpu_available else 0
        except ImportError:
            num_gpu_param = 0
        
        embeddings = OllamaEmbeddings(
            model=embedding_model,
            num_gpu=num_gpu_param  # Use GPU if available, otherwise CPU
        )
    else:
        raise ValueError(f"Unsupported provider: {PROVIDER}. Choose 'ollama' or 'openai'")
    
    print(f"Persisting to ChromaDB at {persist_directory}...")
    
    # Clear existing collection if it exists
    if os.path.exists(persist_directory):
        print("Existing database found. Creating new version...")
    
    vector_store = Chroma.from_documents(
        collection_name=collection_name,
        documents=split_docs,
        embedding=embeddings,
        persist_directory=persist_directory
    )
    
    print(f"\nSuccessfully vectorized and persisted {len(split_docs)} document chunks!")
    print(f"Statistics:")
    print(f"   - Total documents: {len(documents)}")
    print(f"   - Total chunks: {len(split_docs)}")
    print(f"   - Avg chunks per document: {len(split_docs)/len(documents):.1f}")
    
    return vector_store


if __name__ == "__main__":
    # Get embedding model from environment
    embedding_model = os.getenv("EMBEDDING_MODEL")
    
    # Set defaults based on provider if not specified
    if not embedding_model:
        if PROVIDER == "openai":
            embedding_model = "text-embedding-3-small"
        else:
            embedding_model = "nomic-embed-text"
    
    print(f"Using provider: {PROVIDER}")
    print(f"Using embedding model: {embedding_model}")
    
    # Run the vectorization process
    vectorize_and_persist(embedding_model=embedding_model)

