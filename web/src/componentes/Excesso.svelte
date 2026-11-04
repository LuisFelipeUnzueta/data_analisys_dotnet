<script lang="ts">
  import { formatarCompacto, formatarInteiro, formatarPercentual } from '../lib/formato';
  import { base, eixoBase, linhaDeGrade, Plot } from '../lib/plot';
  import { tema } from '../lib/tema.svelte';
  import type { Vistas } from '../lib/vistas';
  import Grafico from './Grafico.svelte';
  import Tabela from './Tabela.svelte';

  // Óbitos observados x esperados pela tendência do período escolhido, com o IC 95% do esperado, e o excesso por ano.
  let { dados }: { dados: NonNullable<Vistas['excesso']> } = $props();

  const SERIES = $derived(['Observados', `Esperados (tendência ${dados.anosBase[0]}–${dados.anosBase.at(-1)})`]);
  const sinal = (v: number) => `${v > 0 ? '+' : ''}${formatarInteiro(Math.round(v))}`;
  const margem = (a: (typeof dados.porAno)[number]) => a.superior - (a.observado - a.esperado);

  function desenhar(largura: number, altura: number) {
    const c = tema.cores;
    const esperados = dados.porAno.map(a => ({ ...a, baixo: a.esperado - margem(a), alto: a.esperado + margem(a) }));
    return Plot.plot({
      ...base(largura, altura),
      marginRight: 24,
      x: { label: null, tickFormat: (a: number) => String(a), ticks: Math.min(dados.observados.length, Math.floor(largura / 70)) },
      y: { label: null, zero: true, tickFormat: formatarCompacto },
      color: { domain: SERIES, range: [c['series-1'], c['series-2']], legend: true },
      marks: [
        linhaDeGrade(),
        eixoBase(),
        Plot.areaY(esperados, { x: 'ano', y1: 'baixo', y2: 'alto', fill: c['series-2'], fillOpacity: 0.15 }),
        Plot.lineY(esperados, { x: 'ano', y: 'esperado', stroke: () => SERIES[1], strokeWidth: 2 }),
        Plot.lineY(dados.observados, { x: 'ano', y: 'obitos', stroke: () => SERIES[0], strokeWidth: 2 }),
        Plot.dot(dados.observados, { x: 'ano', y: 'obitos', r: 3, fill: c['series-1'], stroke: c.surface, strokeWidth: 2 }),
        Plot.ruleX(dados.observados, Plot.pointerX({ x: 'ano', stroke: c.muted })),
        Plot.tip(dados.observados, Plot.pointerX({
          x: 'ano',
          y: 'obitos',
          title: d => {
            const a = dados.porAno.find(p => p.ano === d.ano);
            return a
              ? `${d.ano}\nObservados ${formatarInteiro(d.obitos)}\nEsperados ${formatarInteiro(Math.round(a.esperado))}\nExcesso ${sinal(a.observado - a.esperado)}`
              : `${d.ano}\nObservados ${formatarInteiro(d.obitos)}`;
          }
        }))
      ]
    });
  }
</script>

<div class="resumo">
  <div class="numero">
    <span class="rotulo">Excesso {dados.porAno[0]!.ano}–{dados.porAno.at(-1)!.ano}</span>
    <strong>{sinal(dados.total.excesso)}</strong>
    <span class="detalhe">
      {formatarPercentual((100 * dados.total.excesso) / dados.total.esperado)} acima do esperado · IC 95%:
      {sinal(dados.total.inferior)} a {sinal(dados.total.superior)}
    </span>
  </div>
  {#each dados.porAno as a (a.ano)}
    <div class="numero">
      <span class="rotulo">{a.ano}</span>
      <strong>{sinal(a.observado - a.esperado)}</strong>
      <span class="detalhe">{formatarPercentual((100 * (a.observado - a.esperado)) / a.esperado)}</span>
    </div>
  {/each}
</div>

<Grafico {desenhar} altura={260} />
<Tabela
  colunas={['Ano', 'Observados', 'Esperados', 'Excesso', 'IC 95% do excesso']}
  linhas={dados.porAno.map(a => [
    a.ano,
    formatarInteiro(a.observado),
    formatarInteiro(Math.round(a.esperado)),
    sinal(a.observado - a.esperado),
    `${sinal(a.inferior)} a ${sinal(a.superior)}`
  ])}
/>

<style>
  .resumo {
    display: flex;
    flex-wrap: wrap;
    gap: 12px 28px;
    margin-bottom: 12px;
  }
  .numero {
    display: flex;
    flex-direction: column;
  }
  .rotulo,
  .detalhe {
    font-size: 12px;
    color: var(--text-secondary);
  }
  strong {
    font-size: 20px;
    font-weight: 600;
    color: var(--text-primary);
  }
  .numero:first-child strong {
    font-size: 28px;
  }
</style>
