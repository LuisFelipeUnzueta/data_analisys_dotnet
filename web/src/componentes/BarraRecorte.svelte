<script lang="ts">
  import { formatarCompacto, rotulosDoRecorte } from '../lib/formato';
  import type { Dimensoes, Filtro } from '../lib/tipos';
  import { filtroPadrao } from '../lib/url';

  // Barra fixa: mostra o recorte mesmo longe dos filtros; cada filtro ativo vira um item que se remove com um clique.
  let { dimensoes, filtro = $bindable(), totalObitos }: { dimensoes: Dimensoes; filtro: Filtro; totalObitos: number } = $props();

  const padrao = $derived(filtroPadrao(dimensoes));
  const rotulos = $derived(rotulosDoRecorte(filtro, dimensoes));
  const periodoCompleto = $derived(filtro.anoInicio === padrao.anoInicio && filtro.anoFim === padrao.anoFim);

  type Item = { texto: string; limpar: (() => void) | null };
  const itens: Item[] = $derived([
    {
      texto: `${filtro.anoInicio}–${filtro.anoFim}`,
      limpar: periodoCompleto ? null : () => { filtro.anoInicio = padrao.anoInicio; filtro.anoFim = padrao.anoFim; }
    },
    { texto: rotulos.local, limpar: filtro.local.tipo === 'brasil' ? null : () => { filtro.local = padrao.local; } },
    { texto: rotulos.causa, limpar: filtro.grupo === null ? null : () => { filtro.grupo = null; } },
    { texto: rotulos.idade, limpar: filtro.faixa === null ? null : () => { filtro.faixa = null; } },
    ...(filtro.sexo === null ? [] : [{ texto: rotulos.sexo, limpar: () => { filtro.sexo = null; } }])
  ]);
  const filtrado = $derived(itens.some(i => i.limpar !== null));

  let copiado = $state(false);
  async function copiarLink() {
    await navigator.clipboard.writeText(location.href);
    copiado = true;
    setTimeout(() => (copiado = false), 2000);
  }
</script>

<div class="barra">
  <p class="total"><strong>{formatarCompacto(totalObitos)}</strong> óbitos</p>
  <ul aria-label="Recorte atual">
    {#each itens as item (item.texto)}
      {#if item.limpar}
        <li><button class="ativo" onclick={item.limpar} aria-label="Remover filtro: {item.texto}"><span>{item.texto}</span> ×</button></li>
      {:else}
        <li class="padrao">{item.texto}</li>
      {/if}
    {/each}
  </ul>
  <div class="acoes">
    {#if filtrado}
      <button onclick={() => (filtro = { ...padrao, metrica: filtro.metrica })}>Limpar</button>
    {/if}
    <button onclick={copiarLink}>{copiado ? 'Link copiado' : 'Copiar link'}</button>
  </div>
</div>

<style>
  .barra {
    position: sticky;
    top: 0;
    z-index: 10;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px 12px;
    margin: 0 -16px 12px;
    padding: 8px 16px;
    background: var(--page);
    border-bottom: 1px solid var(--border);
  }
  .total {
    margin: 0;
    font-size: 14px;
    color: var(--text-secondary);
  }
  strong {
    font-size: 18px;
    color: var(--text-primary);
  }
  ul {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 4px 8px;
    flex: 1 1 0;
    min-width: 0;
    margin: 0;
    padding: 0;
    list-style: none;
    font-size: 13px;
    color: var(--text-secondary);
  }
  li {
    min-width: 0;
  }
  .acoes {
    display: flex;
    gap: 6px;
  }
  button {
    font: inherit;
    font-size: 13px;
    padding: 3px 8px;
    color: var(--text-primary);
    background: transparent;
    border: 1px solid var(--border);
    border-radius: 6px;
    cursor: pointer;
  }
  button:focus-visible {
    outline: 2px solid var(--acento);
    outline-offset: 2px;
  }
  .ativo {
    display: flex;
    gap: 4px;
    max-width: 100%;
    border-color: var(--acento);
    background: var(--selecao);
  }
  .ativo span {
    max-width: 220px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  /* No celular a barra mostra só o que foi filtrado, para ocupar pouca altura. */
  @media (max-width: 640px) {
    .padrao {
      display: none;
    }
  }
</style>
