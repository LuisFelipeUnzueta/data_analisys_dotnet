import { svelte } from '@sveltejs/vite-plugin-svelte';
import { defineConfig } from 'vitest/config';

// base relativo: a página funciona em GitHub Pages (subcaminho do repositório) e em S3 sem ajuste.
export default defineConfig({
  base: './',
  plugins: [svelte()],
  test: { environment: 'node' }
});
