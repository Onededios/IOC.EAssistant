import type { ChatConfig } from "./types";

export type ResolvedChatConfig =
  Required<Pick<ChatConfig, "apiBase">> & Omit<ChatConfig, "apiBase">;

export const defaultConfig = {
  title: "IOC E-Assistant",
  placeholder: "Escribe tu pregunta…",
  apiBase: "mock",
} satisfies ResolvedChatConfig;

export function resolveConfig(overrides?: Partial<ChatConfig>): ResolvedChatConfig {
  return { ...defaultConfig, ...overrides };
}

