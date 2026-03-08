import react from '@vitejs/plugin-react';
import { visualizer } from 'rollup-plugin-visualizer';
import { defineConfig } from 'vitest/config';
import tsconfigPaths from 'vite-tsconfig-paths';

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tsconfigPaths(),
    visualizer({
      open: false,
      gzipSize: true,
      brotliSize: true,
      filename: 'stats.html',
    }),
  ],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    exclude: ['tests/**', 'node_modules/**'],
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:7000',
        changeOrigin: true,
      },
      '/notificationHub': {
        target: 'http://localhost:7001',
        ws: true,
        changeOrigin: true,
      },
    },
  },
});
