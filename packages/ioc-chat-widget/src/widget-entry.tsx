import React from "react";
import ReactDOM from "react-dom/client";
import { ChatWidget } from "./components/ChatWidget";

type InitOptions = {
  selector?: string;
  apiBase?: string;
  tokenProvider?: () => Promise<string | null>;
  title?: string;
  placeholder?: string;
};

declare global {
  interface Window {
    ChatWidget?: {
      init: (selectorOrOptions?: string | InitOptions) => void;
    };
  }
}

function ensureMount(selector?: string): HTMLElement {
  if (selector) {
    const el = document.querySelector(selector);
    if (el) return el as HTMLElement;
    throw new Error(`No se encontró el selector "${selector}".`);
  }
  const mount = document.createElement("div");
  mount.id = "chat-widget-mount";
  document.body.appendChild(mount);
  return mount;
}

function init(selectorOrOptions?: string | InitOptions) {
  const opts: InitOptions =
    typeof selectorOrOptions === "string"
      ? { selector: selectorOrOptions }
      : selectorOrOptions ?? {};

  const mount = ensureMount(opts.selector);

  ReactDOM.createRoot(mount).render(
    <React.StrictMode>
      <ChatWidget
        config={{
          apiBase: opts.apiBase ?? "mock",
          tokenProvider: opts.tokenProvider,
          title: opts.title ?? "IOC E-Assistant",
          placeholder: opts.placeholder ?? "Escribe tu pregunta…",
        }}
      />
    </React.StrictMode>
  );
}
window.ChatWidget = { init };
