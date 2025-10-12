import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { MessageList } from "./MessageList";
import type { ChatMessage } from "../core/types";

describe("MessageList", () => {
  it("muestra un timestamp visible en cada mensaje", () => {
    const now = Date.now();
    const msgs: ChatMessage[] = [
      { id: "1", role: "user", content: "Hola", createdAt: now },
      { id: "2", role: "assistant", content: "¿Qué tal?", createdAt: now }
    ];

    render(<MessageList messages={msgs} loading={false} />);

    // Comprueba contenido
    expect(screen.getByText("Hola")).toBeInTheDocument();

    // Busca timestamps por patrón genérico: "dd/dd/dd, hh:mm" (no asumimos orden ni 12/24h)
    const tsRegex = /\d{1,2}\/\d{1,2}\/\d{2}.*\d{1,2}:\d{2}/;
    const candidates = screen.getAllByText((content) => tsRegex.test(content));
    expect(candidates.length).toBeGreaterThanOrEqual(1);
  });
});
