// vite.lib.config.ts
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import { resolve } from "path";

export default defineConfig({
  plugins: [react()],
  build: {
    lib: {
      entry: resolve(__dirname, "src/widget-entry.tsx"),
      name: "ChatWidget",
      formats: ["iife"],
      fileName: () => "chat-widget.iife.js",
    },
    outDir: "dist",
    emptyOutDir: false,  
    sourcemap: true,
    minify: "esbuild",
    target: "es2020",
    rollupOptions: {
      output: {
        extend: true,
      },
    },
  },
});

