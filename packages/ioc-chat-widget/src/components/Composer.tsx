import type { RefObject } from "react";
import { Spinner } from "./Spinner";

export function Composer({
  value,
  onChange,
  onSend,
  placeholder,
  loading,
  inputRef,
}: {
  value: string | undefined;
  onChange: (v: string) => void;
  onSend: () => void;
  placeholder?: string;
  loading: boolean;
  inputRef?: RefObject<HTMLInputElement>;
}) {
  const safe = value ?? "";

  return (
    <form
      className="flex items-center gap-2 border-t px-2 py-2 bg-white"
      onSubmit={(e) => {
        e.preventDefault();
        onSend();
      }}
    >
      <input
        ref={inputRef}
        value={safe}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="flex-1 rounded-xl border px-3 py-2 text-sm focus:outline-none focus:ring focus:border-gray-400 bg-white text-gray-900 placeholder-gray-500 shadow-inner"
        aria-label="Escribe tu mensaje"
      />
      <button
        type="submit"
        disabled={loading || safe.trim().length === 0}
        className="rounded-xl px-3 py-2 text-sm text-white bg-brand disabled:opacity-50 hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-brand flex items-center gap-2"
      >
        {loading ? (<><Spinner className="text-white" size={16} /><span>Enviando…</span></>) : "Enviar"}
      </button>
    </form>
  );
}
