"""
RAG Agent Class
Creates an agent with tools for querying vectorized documents with conversation history
and improved retrieval + web search fallbacks.
"""
from typing import List, Tuple, Optional
from dotenv import load_dotenv
from langchain.tools import tool
from langchain.agents import create_agent
from langchain_chroma import Chroma
from langchain_core.messages import HumanMessage, AIMessage
from langchain_ollama import OllamaEmbeddings, ChatOllama

load_dotenv()

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
        temperature: float = 0.0,
        k_results: int = 4,
        num_ctx: int = 8192,
        use_mmr: bool = True,
        fetch_k_multiplier: int = 4,
        score_threshold: float = 0.2,
    ) -> None:
        """
        Initialize RAG Agent

        Args:
            persist_directory: Path to ChromaDB
            collection_name: Name of the ChromaDB collection
            embedding_model: Ollama embedding model
            llm_model: Ollama LLM model
            temperature: LLM temperature (0-1)
            k_results: Number of documents to retrieve
            num_ctx: LLM context window
            use_mmr: Use maximal marginal relevance for retrieval diversification
            fetch_k_multiplier: Over-fetch factor for MMR
            score_threshold: Similarity score cutoff when not using MMR
        """

        self.k_results = k_results
        self.conversation_history: List[Tuple[str, str]] = []
        self.use_mmr = use_mmr
        self.fetch_k_multiplier = max(2, int(fetch_k_multiplier))
        self.score_threshold = float(score_threshold)

        print(f"Initializing RAG Agent with model {llm_model}...")

        # Decide Ollama GPU param (-1 uses all GPUs, 0 CPU), keep simple and robust
        try:
            import torch  # type: ignore

            num_gpu_param = -1 if torch.cuda.is_available() else 0
        except Exception:
            num_gpu_param = 0

        print(f"Using num_gpu parameter: {num_gpu_param}")

        # Initialize embeddings
        self.embeddings = OllamaEmbeddings(
            model=embedding_model,
            num_gpu=num_gpu_param,
        )

        # Load vector store
        print(f"Loading vector store from {persist_directory}...")
        self.vector_store = Chroma(
            collection_name=collection_name,
            embedding_function=self.embeddings,
            persist_directory=persist_directory,
        )

        # Initialize LLM (increase context window for better grounding)
        self.llm = ChatOllama(
            model=llm_model,
            temperature=temperature,
            num_gpu=num_gpu_param,
            num_ctx=num_ctx,
        )

        # Create tools
        self._create_retrieval_general_tool()
        self._create_retrieval_noticia_tool()
        self._create_history_tool()
        self._create_web_search_tool()

        # Try to create agent with tools, fallback to simple RAG if not supported
        self._initialize_agent()

    # ------------------------------ Tools ---------------------------------
    def _create_retrieval_general_tool(self) -> None:
        """Create the retrieval tool for general IOC docs with MMR option."""
        vector_store = self.vector_store
        k = self.k_results
        use_mmr = self.use_mmr
        fetch_k = max(k * self.fetch_k_multiplier, 20)
        score_threshold = self.score_threshold

        @tool(response_format="content_and_artifact")
        def retrieve_general_context(query: str):
            """Retrieve IOC general docs (guides, procedures, FAQs, reference)."""
            try:
                if use_mmr:
                    retrieved_docs = vector_store.max_marginal_relevance_search(
                        query,
                        k=k,
                        fetch_k=fetch_k,
                        filter={"type": "general"},
                    )
                else:
                    with_scores = vector_store.similarity_search_with_score(
                        query,
                        k=k,
                        filter={"type": "general"},
                    )
                    # For some backends higher score is better; if None, keep the doc
                    retrieved_docs = [
                        doc for doc, score in with_scores if (score is None or score >= score_threshold)
                    ]
            except Exception:
                # Fallback to basic similarity search
                retrieved_docs = vector_store.similarity_search(
                    query, k=k, filter={"type": "general"}
                )

            serialized = "\n\n".join(
                (f"Source: {doc.metadata}\nContent: {doc.page_content}") for doc in retrieved_docs
            )
            return serialized, retrieved_docs

        self.retrieve_general_context = retrieve_general_context

    def _create_retrieval_noticia_tool(self) -> None:
        """Create the retrieval tool for IOC news/announcements with MMR option."""
        vector_store = self.vector_store
        k = self.k_results
        use_mmr = self.use_mmr
        fetch_k = max(k * self.fetch_k_multiplier, 20)
        score_threshold = self.score_threshold

        @tool(response_format="content_and_artifact")
        def retrieve_noticia_context(query: str):
            """Retrieve IOC news/announcements (dates, recent changes)."""
            try:
                if use_mmr:
                    retrieved_docs = vector_store.max_marginal_relevance_search(
                        query,
                        k=k,
                        fetch_k=fetch_k,
                        filter={"type": "noticia"},
                    )
                else:
                    with_scores = vector_store.similarity_search_with_score(
                        query,
                        k=k,
                        filter={"type": "noticia"},
                    )
                    retrieved_docs = [
                        doc for doc, score in with_scores if (score is None or score >= score_threshold)
                    ]
            except Exception:
                retrieved_docs = vector_store.similarity_search(
                    query, k=k, filter={"type": "noticia"}
                )

            serialized = "\n\n".join(
                (f"Source: {doc.metadata}\nContent: {doc.page_content}") for doc in retrieved_docs
            )
            return serialized, retrieved_docs

        self.retrieve_noticia_context = retrieve_noticia_context

    def _create_history_tool(self) -> None:
        """Create a placeholder history tool (not used with external history management)."""
        @tool
        def get_user_history(user_id: str, limit: int = 5):
            """Placeholder: History managed by .NET backend."""
            return "History is managed by the backend system."

        self.get_user_history = get_user_history

    def _create_web_search_tool(self) -> None:
        """Create the web search tool using DuckDuckGo with site-restricted first."""

        @tool
        def web_search(query: str):
            """Search the web for IOC info. Prefer site:ioc.xtec.cat; fallback open web."""
            try:
                try:
                    from langchain_community.tools import DuckDuckGoSearchResults  # type: ignore

                    search = DuckDuckGoSearchResults(num_results=5)
                    results = search.run(f"site:ioc.xtec.cat {query}")
                    if not results:
                        results = search.run(query)
                    return results
                except Exception:
                    from langchain_community.tools import DuckDuckGoSearchRun  # type: ignore

                    search = DuckDuckGoSearchRun()
                    return search.run(f"site:ioc.xtec.cat {query}") or search.run(query)
            except Exception as e:
                return f"Web search failed: {str(e)}"

        self.web_search = web_search

    # --------------------------- Agent wiring ------------------------------
    def _initialize_agent(self) -> None:
        """Initialize the agent, with fallback to simple RAG."""
        # Include all retrieval, history, and web tools
        self.tools = [
            self.retrieve_general_context,
            self.retrieve_noticia_context,
            self.get_user_history,
            self.web_search,
        ]

        system_prompt = (
            "Ets un assistent expert de l'Institut Obert de Catalunya (IOC). "
            "IMPORTANT: Respon SEMPRE a la pregunta més recent de l'usuari. "
            "Tria eines segons el context: si és procediment o guia, usa retrieve_general_context; "
            "si és canvi/novetat/‘notícia’ o hi ha dates recents, usa retrieve_noticia_context. "
            "Si la recuperació és buida o poc rellevant, usa web_search. "
            "Respon sempre en l'idioma de la pregunta."
        )

        try:
            self.agent = create_agent(self.llm, self.tools, system_prompt=system_prompt)
            self.use_agent = True
            print("Using agent mode with tool calling")
        except NotImplementedError:
            print("Agent mode not supported, langchain version may be outdated.")
            self.use_agent = False

    # ---------------------------- Query path -------------------------------
    def query(self, question: str, verbose: bool = True) -> str:
        """
        Query the agent without history (for simple CLI usage).
        For web API with history, use query_with_history instead.
        """
        messages = [HumanMessage(content=question)]

        try:
            response = self.agent.invoke({"messages": messages})
            response_text = response["messages"][-1].content
        except Exception as e:
            if verbose:
                print(f"Agent invocation failed, using simple RAG fallback: {e}")

            # Simple RAG fallback: attempt diversified retrieval across both types
            ctx_docs = []
            try:
                ctx_docs += self.vector_store.max_marginal_relevance_search(
                    question,
                    k=self.k_results,
                    fetch_k=max(20, self.k_results * self.fetch_k_multiplier),
                    filter={"type": "general"},
                )
            except Exception:
                ctx_docs += self.vector_store.similarity_search(
                    question, k=self.k_results
                )
            try:
                ctx_docs += self.vector_store.max_marginal_relevance_search(
                    question,
                    k=self.k_results,
                    fetch_k=max(20, self.k_results * self.fetch_k_multiplier),
                    filter={"type": "noticia"},
                )
            except Exception:
                pass

            context_blob = "\n\n".join(
                [f"Source: {d.metadata}\nContent: {d.page_content}" for d in ctx_docs]
            )
            response_text = self.llm.invoke(context_blob + "\n\nQuestion: " + question).content

        self.conversation_history.append((question, response_text))
        return response_text

    def query_with_history(
        self,
        question: str,
        conversation_history: List[Tuple[str, str]] = None,
        temperature: Optional[float] = None,
        verbose: bool = True
    ) -> str:
        """
        Query the agent with externally provided conversation history.
        This method does NOT persist to database - history management is external.
        
        Args:
            question: The current question to answer
            conversation_history: List of (question, answer) tuples representing previous conversation
            temperature: Optional temperature override for this query
            verbose: Whether to print debug information
            
        Returns:
            The agent's response as a string
        """
        messages = []
        
        # Add conversation history from parameter (if provided)
        if conversation_history:
            for question_hist, answer_hist in conversation_history:
                messages.append(HumanMessage(content=question_hist))
                messages.append(AIMessage(content=answer_hist))
        
        # Current question
        messages.append(HumanMessage(content=question))
        
        # Temporarily override temperature if provided
        original_temp = None
        if temperature is not None:
            original_temp = self.llm.temperature
            self.llm.temperature = temperature
        
        try:
            response = self.agent.invoke({"messages": messages})
            response_text = response["messages"][-1].content
        except Exception as e:
            if verbose:
                print(f"Agent invocation failed, using simple RAG fallback: {e}")
            
            # Simple RAG fallback: attempt diversified retrieval across both types
            ctx_docs = []
            try:
                ctx_docs += self.vector_store.max_marginal_relevance_search(
                    question,
                    k=self.k_results,
                    fetch_k=max(20, self.k_results * self.fetch_k_multiplier),
                    filter={"type": "general"},
                )
            except Exception:
                ctx_docs += self.vector_store.similarity_search(
                    question, k=self.k_results
                )
            try:
                ctx_docs += self.vector_store.max_marginal_relevance_search(
                    question,
                    k=self.k_results,
                    fetch_k=max(20, self.k_results * self.fetch_k_multiplier),
                    filter={"type": "noticia"},
                )
            except Exception:
                pass
            
            context_blob = "\n\n".join(
                [f"Source: {d.metadata}\nContent: {d.page_content}" for d in ctx_docs]
            )
            
            # Build simple prompt with context
            prompt = f"Context:\n{context_blob}\n\nQuestion: {question}\n\nAnswer:"
            response_text = self.llm.invoke(prompt).content
        finally:
            # Restore original temperature if it was overridden
            if original_temp is not None:
                self.llm.temperature = original_temp
        
        return response_text

    # ---------------------------- Utilities --------------------------------
    def clear_history(self) -> None:
        """Clear in-memory conversation history."""
        self.conversation_history = []
        print("Conversation history cleared (in-memory)")

    def get_history(self) -> List[Tuple[str, str]]:
        """Get in-memory conversation history."""
        return self.conversation_history.copy()

    def search_documents(
        self, query: str, k: Optional[int] = None, filter_dict: Optional[dict] = None
    ) -> List:
        """Direct search in vector store without LLM with optional filtering."""
        k = k or self.k_results
        if filter_dict:
            return self.vector_store.similarity_search(query, k=k, filter=filter_dict)
        return self.vector_store.similarity_search(query, k=k)

    def search_by_metadata(self, metadata_key: str, metadata_value: str, k: int = 10) -> List:
        """Search documents by metadata."""
        filter_dict = {metadata_key: metadata_value}
        return self.vector_store.similarity_search("", k=k, filter=filter_dict)


if __name__ == "__main__":
    # Example usage for CLI testing
    agent = RAGAgent()
    print("IOC.EAssistant - Type your questions (Ctrl+C to exit)")
    try:
        while True:
            user_input = input("\nYou: ")
            if user_input.strip():
                response = agent.query(user_input)
                print(f"\nAssistant: {response}")
    except KeyboardInterrupt:
        print("\n\nExiting...")