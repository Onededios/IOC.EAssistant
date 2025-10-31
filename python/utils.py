"""
Utility functions for RAG Agent
Database operations, formatting helpers, and other reusable utilities
"""
import sqlite3
from typing import List, Tuple
import os


class DatabaseManager:
    """
    Manages SQLite database operations for conversation history
    """
    
    def __init__(self, db_path: str = "./histories.db"):
        """
        Initialize database manager
        
        Args:
            db_path: Path to SQLite database file
        """
        self.db_path = db_path
        self.init_database()
    
    def init_database(self):
        """Initialize SQLite database with required tables and indexes"""
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS conversations (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id TEXT NOT NULL,
                question TEXT NOT NULL,
                answer TEXT NOT NULL,
                timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                created_at DATETIME DEFAULT CURRENT_TIMESTAMP
            )
        ''')
        
        # Create index on user_id for faster queries
        cursor.execute('''
            CREATE INDEX IF NOT EXISTS idx_user_id ON conversations(user_id)
        ''')
        
        conn.commit()
        conn.close()
    
    def save_conversation(self, user_id: str, question: str, answer: str):
        """
        Save a conversation to the database
        
        Args:
            user_id: User identifier
            question: User's question
            answer: Agent's answer
        """
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            INSERT INTO conversations (user_id, question, answer)
            VALUES (?, ?, ?)
        ''', (user_id, question, answer))
        
        conn.commit()
        conn.close()
    
    def get_user_history(self, user_id: str, limit: int = 10) -> List[Tuple[str, str, str]]:
        """
        Get conversation history from database for a specific user
        
        Args:
            user_id: User identifier
            limit: Maximum number of conversations to retrieve
            
        Returns:
            List of tuples (question, answer, timestamp)
        """
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT question, answer, timestamp
            FROM conversations
            WHERE user_id = ?
            ORDER BY timestamp DESC
            LIMIT ?
        ''', (user_id, limit))
        
        results = cursor.fetchall()
        conn.close()
        
        return results
    
    def delete_user_history(self, user_id: str):
        """
        Delete all conversation history for a specific user
        
        Args:
            user_id: User identifier
        """
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            DELETE FROM conversations
            WHERE user_id = ?
        ''', (user_id,))
        
        conn.commit()
        conn.close()
    
    def get_all_users(self) -> List[str]:
        """
        Get list of all unique user IDs in the database
        
        Returns:
            List of user IDs
        """
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT DISTINCT user_id
            FROM conversations
        ''')
        
        results = cursor.fetchall()
        conn.close()
        
        return [user_id[0] for user_id in results]


def format_conversation_history(history: List[Tuple[str, str]], max_exchanges: int = 5) -> str:
    """
    Format conversation history as a readable string
    
    Args:
        history: List of (user_message, assistant_message) tuples
        max_exchanges: Maximum number of exchanges to include (most recent)
        
    Returns:
        Formatted history string
    """
    if not history:
        return "No previous conversation."
    
    formatted = []
    for i, (user_msg, ai_msg) in enumerate(history[-max_exchanges:], 1):
        formatted.append(f"User: {user_msg}\nAssistant: {ai_msg}")
    
    return "\n\n".join(formatted)


def format_db_history_for_display(
    history: List[Tuple[str, str, str]], 
    user_id: str
) -> str:
    """
    Format database history for display with timestamps
    
    Args:
        history: List of (question, answer, timestamp) tuples
        user_id: User identifier
        
    Returns:
        Formatted history string with timestamps
    """
    if not history:
        return f"No conversation history found for user {user_id}"
    
    history_text = f"Previous conversations for user {user_id}:\n\n"
    for i, (question, answer, timestamp) in enumerate(reversed(history), 1):
        history_text += f"{i}. [{timestamp}]\n"
        history_text += f"   Q: {question}\n"
        history_text += f"   A: {answer}\n\n"
    
    return history_text


def configure_gpu_settings(num_gpu: int = 1, cuda_device: int = 0):
    """
    Configure GPU settings for Ollama
    
    Args:
        num_gpu: Number of GPUs to use (-1 for all available)
        cuda_device: CUDA device index to use
    """
    os.environ['OLLAMA_NUM_GPU'] = str(num_gpu)
    os.environ['CUDA_VISIBLE_DEVICES'] = str(cuda_device)


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
