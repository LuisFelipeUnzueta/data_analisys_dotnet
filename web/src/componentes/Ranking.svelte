<script lang="ts">
  import { aoClicar, base, Plot } from '../lib/plot';
  import { tema } from '../lib/tema.svelte';
  import Grafico from './Grafico.svelte';
  import Tabela from './Tabela.svelte';

  // Barras horizontais ordenadas; com um grupo selecionado, ele fica em destaque e os demais recuam.
  let {
    itens,
    selecionado,
    formatar,
    rotuloMetrica,
    aoSelecionar
  }: {
    itens: { indice: number; nome: string; valor: number }[];
    selecionado: number | null;
    formatar: (valor: number) => string;
    rotuloMetrica: string;
    aoSelecionar: (grupo: number) => void;
  } = $props();

  function desenhar(largura: number, altura: number) {
    const c = tema.cores;
    const cor = (d: (typeof itens)[number]) => (selecionado === null || d.indice === selecionado ? c['series-1'] : c.muted);
    const margemNomes = Math.min(220, largura * 0.42);
    const figura = Plot.plot({
      ...base(largura, altura),
      marginLeft: margemNomes,
      marginRight: 64,
      x: { label: null, axis: null },
      y: { label: null, domain: itens.map(i => i.nome), axis: null },
      marks: [
        // Nomes longos quebram em até duas linhas em vez de serem cortados (lineWidth em ems).
        Plot.axisY({ tickSize: 0, tickPadding: 8, lineWidth: margemNomes / 12 - 1, fill: c['text-secondary'] }),
        Plot.barX(itens, { x: 'valor', y: 'nome', fill: cor, insetTop: 4, insetBottom: 4 }),
        Plot.text(itens, { x: 'valor', y: 'nome', text: d => formatar(d.valor), dx: 6, textAnchor: 'start', fill: c['text-primary'] }),
        Plot.ruleX([0], { stroke: c.muted }),
        Plot.tip(itens, Plot.pointerY({ x: 'valor', y: 'nome', title: d => `${d.nome}\n${formatar(d.valor)}` }))
      ]
    });
    aoClicar<(typeof itens)[number]>(figura, item => aoSelecionar(item.indice));
    return figura;
  }
</script>

<Grafico {desenhar} altura={Math.max(160, itens.length * 38 + 24)} />
<Tabela colunas={['Causa', rotuloMetrica]} linhas={itens.map(i => [i.nome, formatar(i.valor)])} />
