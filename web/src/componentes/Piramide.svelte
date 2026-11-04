<script lang="ts">
  import { formatarCompacto } from '../lib/formato';
  import { base, Plot } from '../lib/plot';
  import { tema } from '../lib/tema.svelte';
  import Grafico from './Grafico.svelte';
  import Tabela from './Tabela.svelte';

  // Pirâmide: masculino à esquerda (valores negativos no eixo), feminino à direita; 80+ no topo.
  let {
    linhas,
    formatar,
    rotuloMetrica
  }: {
    linhas: { faixa: string; masculino: number | null; feminino: number | null }[];
    formatar: (valor: number) => string;
    rotuloMetrica: string;
  } = $props();

  const SEXOS = ['Masculino', 'Feminino'];

  function desenhar(largura: number, altura: number) {
    const c = tema.cores;
    const barras = linhas.flatMap(l => [
      { faixa: l.faixa, sexo: 'Masculino', valor: l.masculino ?? 0 },
      { faixa: l.faixa, sexo: 'Feminino', valor: l.feminino ?? 0 }
    ]);
    return Plot.plot({
      ...base(largura, altura),
      marginLeft: 48,
      x: { label: rotuloMetrica, tickFormat: (v: number) => formatarCompacto(Math.abs(v)) },
      y: { label: null, domain: linhas.map(l => l.faixa).reverse() },
      color: { domain: SEXOS, range: [c['series-1'], c['series-2']], legend: true },
      marks: [
        Plot.gridX({ stroke: c.grid, strokeOpacity: 1 }),
        Plot.barX(barras, {
          x: d => (d.sexo === 'Masculino' ? -d.valor : d.valor),
          y: 'faixa',
          fill: 'sexo',
          insetTop: 1,
          insetBottom: 1
        }),
        Plot.ruleX([0], { stroke: c.muted }),
        Plot.tip(barras, Plot.pointer({
          x: d => (d.sexo === 'Masculino' ? -d.valor : d.valor),
          y: 'faixa',
          title: d => `${d.sexo}, ${d.faixa}\n${formatar(d.valor)}`
        }))
      ]
    });
  }
</script>

<Grafico {desenhar} altura={420} />
<Tabela
  colunas={['Faixa etária', 'Masculino', 'Feminino']}
  linhas={linhas.map(l => [l.faixa, l.masculino === null ? '—' : formatar(l.masculino), l.feminino === null ? '—' : formatar(l.feminino)])}
/>
