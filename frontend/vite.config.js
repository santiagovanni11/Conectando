import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  // Necesario para que vitest transforme los .jsx con el runtime automático.
  esbuild: {
    jsx: 'automatic',
  },
  server: {
    proxy: {
      '/api': 'http://localhost:5025',
      // El hub de mensajería necesita ws: true, si no el WebSocket no
      // negotiate y en desarrollo no llegan los mensajes en vivo.
      '/hubs': {
        target: 'http://localhost:5025',
        ws: true,
      },
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test/setup.js',
    css: false,
  },
})