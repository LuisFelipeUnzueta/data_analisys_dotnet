<script lang="ts">
  import { formatarCompacto, formatarIntervalo } from '../lib/formato';
  import { base, eixoBase, linhaDeGrade, Plot } from '../lib/plot';
  import { tema } from '../lib/tema.svelte';
  import type { PontoAnual } from '../lib/vistas';
  import Grafico from './Grafico.svelte';
  import Tabela from './Tabela.svelte';

  // Série anual de um só valor: linha de 2px, faixa do IC 95% (quando há), ponto final rotulado,
  // crosshair com tooltip, marcos verticais e anos instáveis como círculos vazados.
  let {
    pontos,
    formatar,
    formatarEixo = formatarCompacto,
    marcos = [],
    altura = 280,
    rotuloColuna
  }: {
    pontos: PontoAnual[];
    formatar: (valor: number) => string;
    formatarEixo?: (valor: number) => string;
    marcos?: { ano: number; texto: string }[];
    altura?: number;
    rotuloColuna: string;
  } = $props();

  type Valido = PontoAnual & { valor: number };
  const validos = $derived(pontos.filter((p): p is Valido => p.valor !== null));
  const comIntervalo = $derived(validos.some(p => p.inferior !== undefined && p.inferior !== p.superior));
  const instaveis = $derived(validos.filter(p => p.instavel));
  const rotulo = (d: Valido) => [String(d.ano), formatar(d.valor), formatarIntervalo(d, formatar), d.instavel ? 'estimativa instável' : '']
    .filter(Boolean).join('\n');

  function desenhar(largura: number, alturaGrafico: number) {
    const c = tema.cores;
    const ultimo = validos.at(-1);
    return Plot.plot({
      ...base(largura, alturaGrafico),
      marginRight: 56,
      x: { label: null, tickFormat: (a: number) => String(a), ticks: Math.max(2, Math.min(validos.length, Math.floor(largura / 70))) },
      y: { label: null, zero: true, tickFormat: formatarEixo },
      marks: [
        linhaDeGrade(),
        eixoBase(),
        ...(comIntervalo
          ? [Plot.areaY(validos, { x: 'ano', y1: 'inferior', y2: 'superior', fill: c['series-1'], fillOpacity: 0.15, curve: 'linear' })]
          : []),
        ...marcos.map(m => Plot.ruleX([m.ano], { stroke: c.muted, strokeWidth: 1 })),
        ...marcos.map(m => Plot.text([m], { x: 'ano', frameAnchor: 'top', text: 'texto', dx: 4, textAnchor: 'start', fill: c['text-secondary'] })),
        Plot.lineY(validos, { x: 'ano', y: 'valor', stroke: c['series-1'], strokeWidth: 2, curve: 'linear' }),
        Plot.dot(instaveis, { x: 'ano', y: 'valor', r: 4, fill: c.surface, stroke: c['series-1'], strokeWidth: 2 }),
        ...(ultimo
          ? [
              Plot.dot([ultimo], { x: 'ano', y: 'valor', r: 4, fill: ultimo.instavel ? c.surface : c['series-1'], stroke: ultimo.instavel ? c['series-1'] : c.surface, strokeWidth: 2 }),
              Plot.text([ultimo], { x: 'ano', y: 'valor', text: d => formatar(d.valor), dx: 8, textAnchor: 'start', fill: c['text-primary'] })
            ]
          : []),
        Plot.ruleX(validos, Plot.pointerX({ x: 'ano', stroke: c.muted })),
        Plot.dot(validos, Plot.pointerX({ x: 'ano', y: 'valor', r: 4, fill: c['series-1'], stroke: c.surface, strokeWidth: 2 })),
        Plot.tip(validos, Plot.pointerX({ x: 'ano', y: 'valor', title: rotulo }))
      ]
    });
  }
</script>

<Grafico {desenhar} {altura} />
{#if comIntervalo || instaveis.length > 0}
  <p class="legenda">
    {#if comIntervalo}<span class="faixa" aria-hidden="true"></span> intervalo de confiança de 95%{/if}
    {#if instaveis.length > 0}<span class="vazado" aria-hidden="true"></span> estimativa instável (menos de 20 óbitos ou erro relativo acima de 30%){/if}
  </p>
{/if}
<Tabela
  colunas={comIntervalo ? ['Ano', rotuloColuna, 'IC 95% inferior', 'IC 95% superior', 'Instável'] : ['Ano', rotuloColuna]}
  linhas={pontos.map(p => {
    const valor = p.valor === null ? '—' : formatar(p.valor);
    return comIntervalo && p.inferior !== undefined && p.superior !== undefined
      ? [p.ano, valor, formatar(p.inferior), formatar(p.superior), p.instavel ? 'sim' : '']
      : comIntervalo ? [p.ano, valor, '—', '—', ''] : [p.ano, valor];
  })}
/>

<style>
  .legenda {
    display: flex;
    flex-wrap: wrap;
    gap: 4px 16px;
    align-items: center;
    margin: 4px 0 0;
    font-size: 12px;
    color: var(--text-secondary);
  }
  .faixa,
  .vazado {
    display: inline-block;
    width: 14px;
    height: 10px;
    margin-right: 4px;
    vertical-align: middle;
  }
  .faixa {
    background: var(--series-1);
    opacity: 0.25;
  }
  .vazado {
    width: 10px;
    border: 2px solid var(--series-1);
    border-radius: 50%;
  }
</style>
