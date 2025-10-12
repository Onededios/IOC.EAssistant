import type { Ref } from "react";
import type { ChatMessage } from "../core/types";
import { ChatHeader } from "./ChatHeader";
import { MessageList } from "./MessageList";
import { Composer } from "./Composer";

export function ChatPanel({
  title,
  messages,
  loading,
  input,
  onChangeInput,
  onSend,
  onNewChat,
  listRef,
}: {
  title: string;
  messages: ChatMessage[];
  loading: boolean;
  input: string;
  onChangeInput: (v: string) => void;
  onSend: () => void;
  onNewChat: () => void;
  listRef?: Ref<HTMLDivElement>;
}) {
  return (
    <section
      id="chat-panel"
      aria-label="Chat con el asistente"
      className="fixed bottom-20 right-5 w-[min(92vw,380px)] max-h-[var(--chat-max-h)] flex flex-col rounded-2xl bg-white shadow-xl border border-gray-200 overflow-hidden"
      role="dialog"
    >
      <ChatHeader title={title} onNewChat={onNewChat} />
      <MessageList messages={messages} loading={loading} listRef={listRef} />
      <Composer
        value={input}
        onChange={onChangeInput}
        onSend={onSend}
        placeholder="Escribe tu pregunta…"
        loading={loading}
      />
    </section>
  );
}
