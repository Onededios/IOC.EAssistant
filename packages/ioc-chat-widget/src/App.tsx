import { ChatWidget } from "./components/ChatWidget";

// - API en mode "mock" asta que tengamos backend.
// - Cuando haya backend: cambia apiBase y si hace falta pasa tokenProvider.

export default function App() {
  return (
    <div className="min-h-screen bg-gradient-to-b from-gray-50 to-white">
      <main className="max-w-3xl mx-auto p-6">
        <h1 className="text-2xl font-bold mb-2">Chatbot IOC E-Assistant</h1>
        <div className="h-[60vh] border rounded-xl bg-white p-4">
        </div>
      </main>

      <ChatWidget
        config={{
          apiBase: "mock", // ← cambia a "/api/chat" (cookies) o "https://api..." (token)
          // tokenProvider: async () => "eyJhbGciOi...", // ← si backend pide Bearer
          title: "Chatbot E-Assistant ",
        }}
      />
    </div>
  );
}

