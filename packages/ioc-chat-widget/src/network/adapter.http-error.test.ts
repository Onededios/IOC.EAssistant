import { describe, it, expect, vi, afterEach, beforeEach } from "vitest";
import { sendChat } from "./adapter";

type MinimalResponse = {
  ok: boolean;
  json: () => Promise<unknown>;
  status?: number;
};

let mockFetch: ReturnType<typeof vi.fn>;

describe("adapter (http error)", () => {
  beforeEach(() => {
    mockFetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      json: async () => ({ error: "fail" }),
    } satisfies MinimalResponse);

    (globalThis as { fetch?: unknown }).fetch = mockFetch as unknown as typeof fetch;
  });

  afterEach(() => {
    delete (globalThis as { fetch?: unknown }).fetch;
    vi.restoreAllMocks();
  });

  it("rechaza o maneja adecuadamente cuando HTTP no es OK", async () => {
    // Si tu adapter LANZA en errores:
    await expect(
      sendChat({ apiBase: "https://api.example.com" }, [{ role: "user", content: "Hola" }])
    ).rejects.toBeDefined();

    // Si en lugar de lanzar DEVUELVES un string de error, usa esto y borra lo anterior:
    // const content = await sendChat({ apiBase: "https://api.example.com" }, [{ role: "user", content: "Hola" }]);
    // expect(content).toMatch(/No se puede conectar|error/i);

    expect(mockFetch).toHaveBeenCalledTimes(1);
  });
});
