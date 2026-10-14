import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

const api = process.env.services__api__http__0 ||
  process.env.services__api__https__0 ||
  'http://localhost:5000';

console.log(`api connected at ${api}`);

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: process.env.PORT ? parseInt(process.env.PORT) : 5173,
    proxy: {
      '^/api': {
        target: api,
        changeOrigin: true,
        // rewrite: (path) => path.replace(/^\/api/, ''), // Not sure if we want to not be using a common /api endpoint group yet.
        configure: (proxy) => {
          proxy.on('error', (err) => {
            console.log('proxy error', err);
          });
        }
      }
    }
  }
});
