import { describe, it, expect } from "vitest";
import { formatDateTime } from "./datetime";

describe("formatDateTime", () => {
  it("devuelve una cadena no vacía", () => {
    const s = formatDateTime(Date.now(), "es-ES");
    expect(typeof s).toBe("string");
    expect(s.length).toBeGreaterThan(0);
  });
});
