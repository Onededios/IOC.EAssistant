import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { ChatWidget } from "./ChatWidget";

type Role = "user" | "assistant" | "system";
type Msg = { role: Role; content: string };

// Mock del adapter tipado (sin `any`)
vi.mock("../network/adapter", () => {
  return {
    sendChat: async (
      _opts: { apiBase: string; tokenProvider?: () => Promise<string | null> },
      msgs: Msg[]
    ): Promise<string> => {
      const last = msgs[msgs.length - 1]?.content ?? "";
      return `Eco: ${last}`;
    },
  };
});

describe("ChatWidget", () => {
  it("abre el panel, envía un mensaje y pinta la respuesta", async () => {
    render(<ChatWidget config={{ apiBase: "mock", title: "Test Bot" }} />);

    const openBtn = screen.getByRole("button", { name: /abrir chat/i });
    fireEvent.click(openBtn);

    const input = await screen.findByRole("textbox", { name: /escribe tu mensaje/i });
    fireEvent.change(input, { target: { value: "Hola" } });

    const sendBtn = screen.getByRole("button", { name: /enviar/i });
    fireEvent.click(sendBtn);

    await waitFor(() => {
      expect(screen.getByText(/Eco: Hola/i)).toBeInTheDocument();
    });
  });
});
