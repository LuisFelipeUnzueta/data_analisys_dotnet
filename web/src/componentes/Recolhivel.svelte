<script lang="ts">
  import type { Snippet } from 'svelte';
  import Painel from './Painel.svelte';

  // Painel fechado por padrão, que abre sozinho quando um link da página aponta para ele.
  let { id, titulo, children }: { id: string; titulo: string; children: Snippet } = $props();

  let aberto = $state(false);
  const abrirSeForOAlvo = () => {
    if (location.hash === `#${id}`) aberto = true;
  };
  abrirSeForOAlvo();
</script>

<svelte:window onhashchange={abrirSeForOAlvo} />

<Painel {id}>
  <details bind:open={aberto}>
    <summary><h2>{titulo}</h2></summary>
    <div class="conteudo">{@render children()}</div>
  </details>
</Painel>

<style>
  summary {
    cursor: pointer;
  }
  h2 {
    display: inline;
  }
  .conteudo {
    margin-top: 12px;
  }
</style>
