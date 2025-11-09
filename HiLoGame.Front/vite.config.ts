// ==========================================
// FILE: vite.config.ts
// ==========================================
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // change target to wherever your API runs in dev
      '/api': { target: 'https://localhost:7258', changeOrigin: true },
      '/hubs': { target: 'http://localhost:5114', ws: true, changeOrigin: true }
    }
  }
})
