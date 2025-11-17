import { describe, it, expect, vi, afterEach, beforeEach } from "vitest";
import { sendChat } from "./adapter";

type MinimalResponse = {
  ok: boolean;
  json: () => Promise<unknown>;
  status?: number;
};

let mockFetch: ReturnType<typeof vi.fn>;

describe("adapter (http ok)", () => {
  beforeEach(() => {
    mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ content: "Respuesta del backend" }),
    } satisfies MinimalResponse);

    (globalThis as { fetch?: unknown }).fetch = mockFetch as unknown as typeof fetch;
  });

  afterEach(() => {
    delete (globalThis as { fetch?: unknown }).fetch;
    vi.restoreAllMocks();
  });

  it("retorna el 'content' del backend cuando HTTP responde OK", async () => {
    const content = await sendChat(
      { apiBase: "https://api.example.com" },
      [{ role: "user", content: "Hola" }]
    );
    expect(content).toBe("Respuesta del backend");
    expect(mockFetch).toHaveBeenCalledTimes(1);
  });
});
