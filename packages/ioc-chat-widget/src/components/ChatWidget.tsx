import { useEffect, useMemo, useRef, useState } from "react";
import { sendChat } from "../network/adapter";
import type { ChatMessage, ChatConfig } from "../core/types";
import { resolveConfig } from "../core/config";
import { ChatPanel } from "./ChatPanel";
import { ChatButton } from "./ChatButton";

export function ChatWidget({ config }: { config?: Partial<ChatConfig> }) {
  const cfg = resolveConfig(config);
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const listRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (open) inputRef.current?.focus();
  }, [open]);

  async function handleSend() {
    const text = input.trim();
    if (!text) return;
  
    const userMsg: ChatMessage = {
      id: crypto.randomUUID(),
      role: "user",
      content: text,
      createdAt: Date.now(),
    };
  
    setMessages((m) => [...m, userMsg]);
    setInput("");
    setLoading(true);
  
    const started = performance.now();
    try {
      const content = await sendChat(
        { apiBase: cfg.apiBase, tokenProvider: cfg.tokenProvider },
        [...messages, userMsg].map((m) => ({ role: m.role, content: m.content }))
      );
      const botMsg: ChatMessage = {
        id: crypto.randomUUID(),
        role: "assistant",
        content,
        createdAt: Date.now(),
      };
      setMessages((m) => [...m, botMsg]);
    } catch {
      const errMsg: ChatMessage = {
        id: crypto.randomUUID(),
        role: "assistant",
        content: "No se puede conectar con el proveedor en este momento.",
        createdAt: Date.now(),
      };
      setMessages((m) => [...m, errMsg]);
    } finally {
      const elapsed = performance.now() - started;
      const min = 500;
      const wait = Math.max(0, min - elapsed);
      setTimeout(() => {
        setLoading(false);
        requestAnimationFrame(() => {
          listRef.current?.scrollTo({
            top: listRef.current?.scrollHeight ?? 0,
            behavior: "smooth",
          });
        });
      }, wait);
    }
  }
  

  function handleNewChat() {
    setMessages([]);
    setInput("");
  }

  const title = useMemo(() => cfg.title ?? "Asistente", [cfg.title]);

  return (
    <>
      <ChatButton open={open} onToggle={() => setOpen((v) => !v)} />
      {open && (
        <ChatPanel
          title={title}
          messages={messages}
          loading={loading}
          input={input}
          onChangeInput={setInput}
          onSend={handleSend}
          onNewChat={handleNewChat}
          listRef={listRef}
        />
      )}
    </>
  );
}
