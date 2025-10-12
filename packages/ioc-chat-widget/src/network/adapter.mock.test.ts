import { describe, it, expect } from "vitest";
import { sendChat } from "./adapter";

describe("adapter (mock)", () => {
  it("devuelve un eco si apiBase = 'mock'", async () => {
    const content = await sendChat(
      { apiBase: "mock" },
      [{ role: "user", content: "Hola" }]
    );
    expect(content).toMatch(/Has dicho/i);
  });
});
