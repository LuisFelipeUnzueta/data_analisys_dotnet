<script lang="ts">
  import type { Snippet } from 'svelte';

  let { titulo, contexto, id, children }: { titulo?: string; contexto?: string; id?: string; children: Snippet } = $props();
</script>

<section class="painel" {id}>
  {#if titulo}<h2>{titulo}</h2>{/if}
  {#if contexto}<p class="contexto">{contexto}</p>{/if}
  {@render children()}
</section>

<style>
  .painel {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 8px;
    padding: 16px;
    margin-bottom: 16px;
    /* Âncoras não ficam escondidas atrás da barra fixa do recorte. */
    scroll-margin-top: 72px;
  }
  .contexto {
    font-size: 13px;
    color: var(--text-secondary);
    margin: 2px 0 12px;
  }
</style>
