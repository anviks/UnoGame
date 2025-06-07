// Plugins
import Vue from '@vitejs/plugin-vue';
import VueDevTools from 'vite-plugin-vue-devtools';
import SvgLoader from 'vite-svg-loader';

// Utilities
import { defineConfig } from 'vite';
import { fileURLToPath, URL } from 'node:url';
import * as fs from 'node:fs';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    Vue(),
    VueDevTools(),
    SvgLoader(),
  ],
  optimizeDeps: {
    exclude: [
      'vuetify',
      'vue-router',
    ],
  },
  define: { 'process.env': {} },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
    extensions: [
      '.js',
      '.json',
      '.jsx',
      '.mjs',
      '.ts',
      '.tsx',
      '.vue',
    ],
  },
  server: {
    https: {
      key: fs.readFileSync('./certs/localhost-key.pem'),
      cert: fs.readFileSync('./certs/localhost.pem'),
    },
    port: 5173,
  },
});
