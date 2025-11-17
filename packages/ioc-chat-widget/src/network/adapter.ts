// src/network/adapter.ts
export type Role = "user" | "assistant" | "system";
export type TokenProvider = () => Promise<string | null>;

export interface ChatAdapterOptions {
  apiBase: string; // puede ser absoluto o relativo. "mock" activa el modo local.
  tokenProvider?: TokenProvider;
}

type ChatBackendResponse = { content: string };

export async function sendChat(
  options: ChatAdapterOptions,
  messages: Array<{ role: Role; content: string }>
): Promise<string> {
  // --- MODO MOCK: evita fetch y devuelve eco (para desarrollo/tests) ---
  if (options.apiBase === "mock") {
    const last = messages[messages.length - 1]?.content ?? "";
    return `Has dicho: “${last}”. ¿Algo más?`;
  }

  const headers: Record<string, string> = { "Content-Type": "application/json" };
  const useToken = typeof options.tokenProvider === "function";

  if (useToken) {
    const token = await options.tokenProvider!();
    if (token) headers.Authorization = `Bearer ${token}`;
  }

  // Nota: si apiBase es relativo, el fetch usará el origen actual del navegador.
  const res = await fetch(options.apiBase, {
    method: "POST",
    headers,
    credentials: useToken ? "omit" : "include", // cookies solo si NO hay token
    body: JSON.stringify({ messages }),
  });

  // Lanza en errores HTTP. El componente superior ya muestra el mensaje amable.
  if (!res.ok) {
    throw new Error(`HTTP ${res.status || 0}`);
  }

  const data = (await res.json()) as Partial<ChatBackendResponse> | null;
  return typeof data?.content === "string" ? data.content : "";
}
