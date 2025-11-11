"""
Utility functions for RAG Agent
Formatting helpers and other reusable utilities
"""
from typing import List
import os


def configure_gpu_settings(num_gpu: int = 1, cuda_device: int = 0):
    """
    Configure GPU settings for Ollama with automatic CPU fallback
    
    Args:
        num_gpu: Number of GPUs to use (-1 for all available, 0 for CPU only)
        cuda_device: CUDA device index to use
        
    Returns:
        int: Number of GPUs configured (0 if using CPU)
    """
    try:
        import torch
        if torch.cuda.is_available():
            os.environ['OLLAMA_NUM_GPU'] = str(num_gpu)
            os.environ['CUDA_VISIBLE_DEVICES'] = str(cuda_device)
            print(f"GPU available - Using {num_gpu} GPU(s) on device {cuda_device}")
            return num_gpu
        else:
            print("No GPU detected - Using CPU")
            os.environ['OLLAMA_NUM_GPU'] = '0'
            return 0
    except ImportError:
        print("PyTorch not installed - Using CPU (install torch to enable GPU detection)")
        os.environ['OLLAMA_NUM_GPU'] = '0'
        return 0
    except Exception as e:
        print(f"Error detecting GPU: {e} - Using CPU")
        os.environ['OLLAMA_NUM_GPU'] = '0'
        return 0


def format_document_context(retrieved_docs: List, include_metadata: bool = True) -> str:
    """
    Format retrieved documents with metadata for context
    
    Args:
        retrieved_docs: List of retrieved documents
        include_metadata: Whether to include metadata in formatting
        
    Returns:
        Formatted context string
    """
    formatted_chunks = []
    
    for i, doc in enumerate(retrieved_docs, 1):
        if include_metadata and hasattr(doc, 'metadata'):
            metadata = doc.metadata
            
            # Build a readable source description
            source_info = []
            if metadata.get('title'):
                source_info.append(f"Títol: {metadata['title']}")
            if metadata.get('type'):
                source_info.append(f"Tipus: {metadata['type']}")
            if metadata.get('date'):
                source_info.append(f"Data: {metadata['date']}")
            
            source_header = " | ".join(source_info) if source_info else "Font IOC"
            
            formatted_chunks.append(
                f"=== Document {i} ===\n"
                f"{source_header}\n"
                f"\n{doc.page_content}\n"
            )
        else:
            formatted_chunks.append(
                f"=== Document {i} ===\n"
                f"{doc.page_content}\n"
            )
    
    return "\n".join(formatted_chunks)
