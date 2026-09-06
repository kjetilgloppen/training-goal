import vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  build: {
    // Build straight into the ASP.NET app's wwwroot so one server serves the SPA + API.
    outDir: '../server/wwwroot',
    emptyOutDir: true,
  },
  server: {
    // In dev, forward API calls to the running ASP.NET app (dotnet run, http profile).
    proxy: {
      '/api': {
        target: 'http://localhost:5107',
        changeOrigin: true,
      },
    },
  },
})
