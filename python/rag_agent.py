"""
RAG Agent Class
Creates an agent with tools for querying vectorized documents with conversation history
"""
import uuid
from langchain_ollama import OllamaEmbeddings, ChatOllama
from langchain_chroma import Chroma
from langchain.agents import create_agent
from langchain.tools import tool
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage
from langchain_core.prompts import ChatPromptTemplate
from typing import List, Tuple, Optional
from dotenv import load_dotenv
from utils import (
    DatabaseManager,
    format_conversation_history,
    format_db_history_for_display,
    configure_gpu_settings,
    format_document_context
)

load_dotenv()

# Configure GPU usage for Ollama
configure_gpu_settings(num_gpu=1, cuda_device=0)


class RAGAgent:
    """
    RAG Agent with conversation history support and database persistence
    """
    
    def __init__(
        self,
        persist_directory: str = "./chroma_db",
        collection_name: str = "ioc_data",
        embedding_model: str = "nomic-embed-text",
        llm_model: str = "llama3.2",
        temperature: float = 0,
        k_results: int = 4,
        db_path: str = "./histories.db"
    ):
        """
        Initialize RAG Agent
        
        Args:
            persist_directory: Path to ChromaDB
            collection_name: Name of the ChromaDB collection
            embedding_model: Ollama embedding model
            llm_model: Ollama LLM model
            temperature: LLM temperature (0-1)
            k_results: Number of documents to retrieve
            db_path: Path to SQLite database for conversation history
        """
        self.k_results = k_results
        self.conversation_history: List[Tuple[str, str]] = []
        
        # Initialize database manager
        self.db_manager = DatabaseManager(db_path)
        
        print(f"Initializing RAG Agent with model {llm_model}...")
        
        # Initialize embeddings
        self.embeddings = OllamaEmbeddings(
            model=embedding_model,
            num_gpu=-1
        )
        
        # Load vector store
        print(f"Loading vector store from {persist_directory}...")
        self.vector_store = Chroma(
            collection_name=collection_name,
            embedding_function=self.embeddings,
            persist_directory=persist_directory
        )
        
        # Initialize LLM
        self.llm = ChatOllama(
            model=llm_model,
            temperature=temperature,
            num_gpu=-1
        )
        
        # Create retrieval tool
        self._create_retrieval_tool()
        
        # Create history retrieval tool
        self._create_history_tool()
        
        # Try to create agent with tools, fallback to simple RAG if not supported
        self._initialize_agent()
    
    def _create_retrieval_tool(self):
        """Create the retrieval tool with Ollama reranking for higher precision"""
        vector_store = self.vector_store
        k = self.k_results

        @tool(response_format="content_and_artifact")
        def retrieve_context(query: str):
            """Retrieve information from IOC website data and rerank with bge-reranker-large."""
            # Step 1 – Initial semantic search from Chroma
            initial_docs = vector_store.similarity_search(query, k=k * 2)

            # Step 2 – Score each doc using Ollama reranker (local model)
            from langchain_ollama import OllamaEmbeddings
            reranker = OllamaEmbeddings(model="qllama/bge-reranker-large")

            scored = []
            for doc in initial_docs:
                try:
                    # The reranker’s embed method returns a single-score embedding we can use as relevance
                    score_vec = reranker.embed_query(f"{query} [SEP] {doc.page_content[:512]}")
                    score = sum(score_vec) / len(score_vec)
                    scored.append((score, doc))
                except Exception as e:
                    print(f"⚠️ Reranker failed on a doc: {e}")
                    continue

            # Step 3 – Select top-k reranked docs
            reranked = [doc for _, doc in sorted(scored, key=lambda x: x[0], reverse=True)[:k]]

            # Step 4 – Format nicely for the LLM
            serialized = format_document_context(reranked, include_metadata=True)
            return serialized, reranked

        # self.retrieve_context = retrieve_context
        from langchain_classic.tools.retriever import create_retriever_tool
        self.retrieve_context = create_retriever_tool(
            vector_store.as_retriever(),
            "retrieve_blog_posts",
            "Search and return information about Lilian Weng blog posts.",
        )

    
    def _create_history_tool(self):
        """Create the history retrieval tool for the agent"""
        db_manager = self.db_manager
        
        @tool
        def get_user_history(user_id: str, limit: int = 5):
            """Retrieve conversation history for a specific user. Use this ONLY as context/reference to better understand the current question. Always answer the LATEST question, not previous ones."""
            results = db_manager.get_user_history(user_id, limit)
            return format_db_history_for_display(results, user_id)
        
        self.get_user_history = get_user_history
    
    def _initialize_agent(self):
        """Initialize the agent, with fallback to simple RAG"""
        # Update tools list to include history tool
        self.tools = [self.retrieve_context, self.get_user_history]
        
        system_prompt = (
        "Ets un assistent expert de l'Institut Obert de Catalunya (IOC). "
        "IMPORTANT: Respon SEMPRE a la pregunta més recent de l'usuari. "
        "L'historial de converses és només per context i referència. "
        "Utilitza únicament la informació dels documents recuperats. "
        "Si no trobes la resposta, indica-ho clarament. "
        "Quan citis informació, menciona la font (títol, data o categoria). "
        "Respon sempre en l'idioma de la pregunta. "
        "Sigues precís, breu i amb evidència."
        )
        
        try:
            self.agent = create_agent(self.llm, self.tools, system_prompt=system_prompt)
            self.use_agent = True
            print("Using agent mode with tool calling")
        except NotImplementedError:
            print("Agent mode not supported, langchain version may be outdated.")
            self.use_agent = False
    
    def query(self, question: str, user_id: str = "default", verbose: bool = True) -> str:
        """
        Query the agent with a question, maintaining conversation history
        
        Args:
            question: The user's question
            user_id: Unique identifier for the user
            verbose: Whether to print the response
            
        Returns:
            The agent's response as a string
        """
        messages = [SystemMessage(content=f"Context: User ID = {user_id}. Respon SEMPRE a la ÚLTIMA pregunta de l'usuari. L'historial és només context per entendre millor la pregunta actual.")]

        # Build messages with history from database
        db_history = self.db_manager.get_user_history(user_id, limit=5)
        
        # Add database history to messages (in chronological order)
        for question_hist, answer_hist, _ in reversed(db_history):
            messages.append(HumanMessage(content=question_hist))
            messages.append(AIMessage(content=answer_hist))
        
        # Add current question
        messages.append(HumanMessage(content=question))
        
        response_text = self.agent.invoke({"messages": messages})['messages'][-1].content
        
        # Save to database
        self.db_manager.save_conversation(user_id, question, response_text)
        
        # Add to in-memory history (for backward compatibility)
        self.conversation_history.append((question, response_text))
        
        return response_text
    
    def clear_history(self):
        """Clear conversation history"""
        self.conversation_history = []
        print("✓ Conversation history cleared")
    
    def get_history(self, user_id: Optional[str] = None) -> List[Tuple[str, str]]:
        """
        Get conversation history
        
        Args:
            user_id: If provided, get history from database for this user.
                    If None, return in-memory history
        
        Returns:
            List of (question, answer) tuples
        """
        if user_id:
            db_history = self.db_manager.get_user_history(user_id, limit=100)
            return [(q, a) for q, a, _ in db_history]
        return self.conversation_history.copy()
    
    def get_user_conversation_history(self, user_id: str, limit: int = 10) -> List[dict]:
        """
        Get conversation history for a specific user from database
        
        Args:
            user_id: User identifier
            limit: Maximum number of conversations to retrieve
            
        Returns:
            List of conversation dictionaries with question, answer, and timestamp
        """
        history = self.db_manager.get_user_history(user_id, limit)
        return [
            {
                "question": q,
                "answer": a,
                "timestamp": t
            }
            for q, a, t in history
        ]
    
    def search_documents(self, query: str, k: Optional[int] = None, filter_dict: Optional[dict] = None) -> List:
        """
        Direct search in vector store without LLM with optional filtering
        
        Args:
            query: Search query
            k: Number of results (uses self.k_results if not specified)
            filter_dict: Optional filter for metadata (e.g., {"type": "noticias"})
            
        Returns:
            List of retrieved documents
        """
        k = k or self.k_results
        
        if filter_dict:
            return self.vector_store.similarity_search(query, k=k, filter=filter_dict)
        else:
            return self.vector_store.similarity_search(query, k=k)
    
    def search_by_metadata(self, metadata_key: str, metadata_value: str, k: int = 10) -> List:
        """
        Search documents by metadata
        
        Args:
            metadata_key: Metadata field to search (e.g., "type", "noticias")
            metadata_value: Value to match
            k: Number of results
            
        Returns:
            List of documents matching the metadata
        """
        filter_dict = {metadata_key: metadata_value}
        # Using a broad query to get documents, then filtering by metadata
        return self.vector_store.similarity_search("", k=k, filter=filter_dict)


if __name__ == "__main__":
    # Example usage
    agent = RAGAgent()
    user_id = f"{uuid.uuid4()}"

    while True:
        user_input = input("You: ")
        print(agent.query(user_input, user_id=user_id))
