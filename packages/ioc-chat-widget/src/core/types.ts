export type Role = "user" | "assistant" | "system";

export interface ChatMessage {
  id: string;
  role: Role;
  content: string;
  createdAt: number;
}

export interface ChatConfig {
  title?: string;
  placeholder?: string;
  apiBase?: string;
  tokenProvider?: () => Promise<string | null>;
}
