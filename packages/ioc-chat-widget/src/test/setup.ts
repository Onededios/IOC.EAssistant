import "@testing-library/jest-dom";

// scrollTo no existe en JSDOM por defecto
Object.defineProperty(window, "scrollTo", { value: () => {}, writable: true });

// polyfill de crypto.randomUUID si no existe
const g: Record<string, unknown> = globalThis as unknown as Record<string, unknown>;
const maybeCrypto = g.crypto as { randomUUID?: () => string } | undefined;

if (!maybeCrypto || typeof maybeCrypto.randomUUID !== "function") {
  g.crypto = {
    ...(maybeCrypto ?? {}),
    randomUUID: () => "test-uuid-" + Math.random().toString(16).slice(2),
  };
}
