<script lang="ts">
  import { formatarCompacto } from '../lib/formato';
  import { base, eixoBase, linhaDeGrade, Plot } from '../lib/plot';
  import { tema } from '../lib/tema.svelte';
  import Grafico from './Grafico.svelte';
  import Tabela from './Tabela.svelte';

  // Série anual de um só valor: linha de 2px, ponto final rotulado, crosshair com tooltip e marcos verticais.
  let {
    pontos,
    formatar,
    formatarEixo = formatarCompacto,
    marcos = [],
    altura = 280,
    rotuloColuna
  }: {
    pontos: { ano: number; valor: number | null }[];
    formatar: (valor: number) => string;
    formatarEixo?: (valor: number) => string;
    marcos?: { ano: number; texto: string }[];
    altura?: number;
    rotuloColuna: string;
  } = $props();

  const validos = $derived(pontos.filter((p): p is { ano: number; valor: number } => p.valor !== null));

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
        ...marcos.map(m => Plot.ruleX([m.ano], { stroke: c.muted, strokeWidth: 1 })),
        ...marcos.map(m => Plot.text([m], { x: 'ano', frameAnchor: 'top', text: 'texto', dx: 4, textAnchor: 'start', fill: c['text-secondary'] })),
        Plot.lineY(validos, { x: 'ano', y: 'valor', stroke: c['series-1'], strokeWidth: 2, curve: 'linear' }),
        ...(ultimo
          ? [
              Plot.dot([ultimo], { x: 'ano', y: 'valor', r: 4, fill: c['series-1'], stroke: c.surface, strokeWidth: 2 }),
              Plot.text([ultimo], { x: 'ano', y: 'valor', text: d => formatar(d.valor), dx: 8, textAnchor: 'start', fill: c['text-primary'] })
            ]
          : []),
        Plot.ruleX(validos, Plot.pointerX({ x: 'ano', stroke: c.muted })),
        Plot.dot(validos, Plot.pointerX({ x: 'ano', y: 'valor', r: 4, fill: c['series-1'], stroke: c.surface, strokeWidth: 2 })),
        Plot.tip(validos, Plot.pointerX({ x: 'ano', y: 'valor', title: d => `${d.ano}\n${formatar(d.valor)}` }))
      ]
    });
  }
</script>

<Grafico {desenhar} {altura} />
<Tabela colunas={['Ano', rotuloColuna]} linhas={pontos.map(p => [p.ano, p.valor === null ? '—' : formatar(p.valor)])} />
