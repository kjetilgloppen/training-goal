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
      // changeOrigin is left off on purpose: the API then sees Host=localhost:5173, so the
      // Google redirect URI (/signin-google) and the post-login redirect stay on the Vite
      // origin instead of jumping to the API port.
      '/api': {
        target: 'http://localhost:5107',
      },
      // Google redirects back here after sign-in; the server handles it.
      '/signin-google': {
        target: 'http://localhost:5107',
      },
    },
  },
})
