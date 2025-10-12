import type { Ref } from "react";
import type { ChatMessage } from "../core/types";
import { formatDateTime } from "../core/datetime";

export function MessageList({
  messages,
  loading,
  listRef,
}: {
  messages: ChatMessage[];
  loading: boolean;
  listRef?: Ref<HTMLDivElement>;
}) {
  return (
    <div ref={listRef} className="flex-1 overflow-y-auto px-3 py-3 space-y-3" aria-live="polite">
      {messages.length === 0 && (
        <p className="text-sm text-gray-500">Escribe tu primer mensaje para comenzar.</p>
      )}

      {messages.map((m) => {
        const mine = m.role === "user";
        const ts = formatDateTime(m.createdAt);
        return (
          <div key={m.id} className={`max-w-[85%] ${mine ? "ml-auto" : "mr-auto"}`}>
            <article
              className={`rounded-2xl px-3 py-2 text-sm ${
                mine ? "bg-brand text-white" : "bg-gray-100 text-gray-900"
              }`}
            >
              <div className="whitespace-pre-wrap">{m.content}</div>
              <div
                className={`mt-1 text-[10px] ${
                  mine ? "text-white/80 text-right" : "text-gray-500 text-right"
                } select-none`}
                title={ts}
              >
                {ts}
              </div>
            </article>
          </div>
        );
      })}

      {loading && (
        <div className="mr-auto flex items-center gap-2 bg-gray-100 text-gray-700 rounded-2xl px-3 py-2 text-sm">
          <svg
            className="animate-spin"
            width="16"
            height="16"
            viewBox="0 0 24 24"
            role="status"
            aria-label="Cargando"
          >
            <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" className="opacity-25" />
            <path fill="currentColor" d="M4 12a8 8 0 0 1 8-8v4a4 4 0 0 0-4 4H4z" className="opacity-100" />
          </svg>
          <span>Escribiendo…</span>
        </div>
      )}
    </div>
  );
}
