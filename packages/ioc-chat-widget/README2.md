# IOC Chat Widget – Guía de uso, build e integración

> Frontend del widget de chat embebible (TypeScript + React + Vite + Tailwind). Incluye modo **mock** para desarrollo, build **IIFE** para insertar vía `<script>` y **tests** con Vitest.

---

## 1) ¿Qué es y cómo funciona?

**IOC Chat Widget** es un componente de chat flotante que puedes:

* Ejecutar como **SPA de desarrollo** con Vite.
* Empaquetar como **bundle IIFE** para integrarlo en cualquier página HTML con un `<script>` y `window.ChatWidget.init(...)`.
* Conectar a un backend real (cookies o token Bearer) o usar **mock** en local.

---

## 2) Stack técnico

* **React 19 + TS**
* **Vite 7** (dev + build)
* **Tailwind CSS** (estilos utilitarios)
* **Vitest** + **Testing Library** (unit/UI tests)

---

## 3) Estructura del proyecto

```
app/
  src/
    components/
      ChatWidget.tsx        # Orquestador del widget (estado, envío, toggle)
      ChatPanel.tsx         # Panel del chat (header + mensajes + composer)
      ChatHeader.tsx        # Cabecera (título, logo opcional, botón "Nuevo chat")
      MessageList.tsx       # Lista de mensajes + timestamp + spinner
      Composer.tsx          # Input + botón enviar
      ChatButton.tsx        # Botón flotante (logo/X)

    core/
      types.ts              # Tipos (ChatMessage, ChatConfig, etc.)
      config.ts             # `defaultConfig` y `resolveConfig`
      datetime.ts           # formateo de fechas/horas

    network/
      adapter.ts            # `sendChat` (mock/real). Contrato con el backend

    test/
      setup.ts              # setup de Vitest/RTL (polyfills)

    widget-entry.tsx        # Punto de entrada del bundle IIFE (expone window.ChatWidget)

  index.html                # SOLO para SPA (desarrollo)
  src/main.tsx              # Mount de la SPA de ejemplo

  tailwind.config.js        # Config Tailwind (modo ESM)
  postcss.config.js         # PostCSS + Autoprefixer
  vite.config.ts            # Build SPA
  vite.lib.config.ts        # Build IIFE (librería embebible)

  vitest.config.ts          # Config de tests
  package.json              # Scripts y metadatos del paquete
  README.md / INTEGRATION_GUIDE.md
```

---

## 4) Scripts principales

```json
{
  "scripts": {
    "dev": "vite",                          // SPA dev
    "build": "tsc -b && vite build",       // build SPA
    "preview": "vite preview",             // servir build SPA
    "build:widget": "vite build --config vite.lib.config.ts", // build IIFE
    "pack:widget": "npm run build:widget && npm pack",        // empaquetar .tgz

    "test": "vitest run",                  // tests una pasada
    "test:watch": "vitest",                // tests en watch
    "coverage": "vitest run --coverage",   // cobertura

    "lint": "eslint ."                      // lint
  }
}
```

---

## 5) Desarrollo (SPA)

```bash
# 1) instala deps
npm install

# 2) levanta dev server
npm run dev

# 3) abre el enlace local que te da Vite
```

> **No** modifiques `index.html` para probar el modo embebido. Para eso usa un HTML aparte (ver §8).

---

## 6) Build de la librería embebible (IIFE)

Config: `vite.lib.config.ts` genera `dist/chat-widget.iife.js` con el nombre global `window.ChatWidget`.

```bash
npm run build:widget
# resultado: dist/chat-widget.iife.js
```

### Empaquetar como .tgz

```bash
npm run pack:widget
# genera ioc-chat-widget-x.y.z.tgz
```

---

## 7) Integración en cualquier web (script + init)

### Opción A (usar el dist local)

1. Asegúrate de tener `dist/chat-widget.iife.js` (ver §6).
2. Crea `embed-test.html` (en raíz o donde prefieras):

```html
<!doctype html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Prueba IOC Chat Widget (dist local)</title>
  <script src="./dist/chat-widget.iife.js"></script>
</head>
<body>
  <main><h1>Demo IIFE</h1></main>
  <script>
    window.ChatWidget.init({
      title: "Asistente IOC",
      apiBase: "mock" // usar "mock" en local, o URL real en producción
    });
  </script>
</body>
</html>
```

3. Sirve estático (no `file://`): `npx serve -p 5177 .` y abre `/embed-test.html`.

### Opción B (instalar .tgz en un proyecto vacío)

```bash
mkdir -p ~/widget-test && cd ~/widget-test
npm init -y
npm i ../ruta/a/ioc-chat-widget-x.y.z.tgz
```

`index.html` del test:

```html
<!doctype html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Prueba paquete IOC Chat Widget</title>
  <script src="./node_modules/ioc-chat-widget/dist/chat-widget.iife.js"></script>
</head>
<body>
  <main><h1>Demo paquete</h1></main>
  <script>
    window.ChatWidget.init({ title: "Asistente IOC", apiBase: "mock" });
  </script>
</body>
</html>
```

Servir: `npx serve -p 5178 .`

---

## 8) Configuración del widget (API)

El `init` acepta un objeto de configuración (merge con `defaultConfig`):

```ts
interface ChatConfig {
  title?: string;               // Título del header
  placeholder?: string;         // Placeholder del input
  apiBase?: string;             // "mock" | URL de tu endpoint de chat
  tokenProvider?: () => Promise<string | null>; // Si necesitas Bearer
}
```

* **Modo mock**: `apiBase: "mock"` → no hace red y responde eco (útil en local/tests).
* **Producción**: usa URL real (`https://api.tu-dominio.com/chat`) y, si procede, pasa `tokenProvider` para Bearer.

---

## 9) Contrato con backend (adapter)

Archivo: `src/network/adapter.ts` → función `sendChat(options, messages)`

* **Entrada**:

  ```ts
  options = { apiBase: string, tokenProvider?: () => Promise<string|null> }
  messages = Array<{ role: "user"|"assistant"|"system"; content: string }>
  ```
* **Salida**: `Promise<string>` → el contenido textual de la respuesta del asistente.
* **Comportamiento**:

  * `apiBase === "mock"` → devuelve un eco del último mensaje.
  * `apiBase !== "mock"` → `fetch(options.apiBase)` con `POST` y `JSON { messages }`.
  * Si hay `tokenProvider`, añade `Authorization: Bearer <token>` y usa `credentials: "omit"`.
  * Si **no** hay token, usa `credentials: "include"` (cookies).
  * En HTTP error (`!res.ok`) **lanza** `Error("HTTP <status>")`.

**Backend esperado**: `POST <apiBase>` → `{ content: string }`.

---

## 10) Estilos y theming

* Tailwind habilitado en `src/index.css`:

  ```css
  @tailwind base;
  @tailwind components;
  @tailwind utilities;
  :root {
    --chat-radius: 1rem;
    --chat-shadow: 0 10px 30px rgba(0,0,0,.15);
    --chat-max-h: 70vh;
  }
  /* Colores de marca */
  .bg-brand { background-color: #4C96CD; }
  .text-brand { color: #4C96CD; }
  ```
* El botón flotante usa el logo cuando está cerrado y una “X” cuando abierto.
* El header permite título, logo opcional y botón “Nuevo chat”.

---

## 11) Tests

Config: `vitest.config.ts` y `src/test/setup.ts` (JSDOM + jest-dom + polyfills).

### Ejecutar

```bash
npm run test          # una pasada
npm run test:watch    # modo watch
npm run coverage      # cobertura
```

### Qué se testea

* **Adapter** (`src/network/adapter.*.test.ts`):

  * Mock local (`apiBase: "mock"`)
  * HTTP OK (mock de `fetch`)
  * HTTP error (espera reject o ajusta si devuelves string de error)
* **UI** (`src/components/*.test.tsx`):

  * ChatWidget: abrir, enviar, recibir eco.
  * MessageList: render de timestamp por mensaje.

> Si cambias comportamiento del adapter (p.ej. no lanzar en error), ajusta las aserciones del test de error.

---

## 12) Quitar el mock para pruebas/prod

* En **local** puedes usar `apiBase: "mock"`.
* En **preproducción/producción** cambia a tu endpoint real y, si requiere token, provee `tokenProvider`:

```ts
window.ChatWidget.init({
  title: "Asistente IOC",
  apiBase: "https://api.tu-dominio.com/chat",
  tokenProvider: async () => localStorage.getItem("token") || null
});
```

> En producción, si el fetch falla, el componente muestra: “No se puede conectar con el proveedor en este momento.”

---

## 13) Lint y formateo

```bash
npm run lint -- --fix
```

* Regla estricta contra `any` y `@ts-ignore`.
* Importaciones de tipo con `import type { ... }` donde aplica.

---

## 14) Publicación/monorepo

* Si vas a subir a un monorepo, coloca el paquete en `packages/ioc-chat-widget/`.
* **No** subas `node_modules/`, `dist/`, `.vite/`, `coverage/`, `*.tgz`.
* Usa `npm run pack:widget` para generar el artefacto local.

`.gitignore` recomendado:

```
node_modules
.vite
coverage
*.log
*.tgz
.DS_Store
.env*
dist
```

---

## 15) Problemas frecuentes

* **"Unknown at-rule @tailwind" en VS Code**: es solo aviso del analizador CSS. Mantén la extensión **Tailwind CSS IntelliSense**. Si molesta, silencia `css.lint.unknownAtRules` en tu `settings.json` del workspace.
* **El IIFE no carga**: asegúrate de no usar `type="module"` en el `<script>` del IIFE y de servir por HTTP (no `file://`).
* **`window.ChatWidget` undefined**: revisa el orden de scripts y el nombre del archivo (`chat-widget.iife.js`).
* **CORS/credenciales**: si usas cookies, backend debe permitir `credentials` y orígenes.

---

## 16) Roadmap (sugerencias)

* Soporte de streaming (SSE) para la respuesta del asistente.
* Reintentos y timeout en `sendChat` (AbortController).
* Theming avanzado vía CSS variables.
* e2e con Playwright (abrir/minimizar/escribir/recibir).

---

## 17) Licencia

MIT (o la que el repositorio principal defina).

---

**Contacto**: Para dudas de integración o cambios de contrato con backend, consulta `src/network/adapter.ts` y coordina el esquema de `POST <apiBase> { messages } → { content }`. ¡Buen vibecoding! 🎯
