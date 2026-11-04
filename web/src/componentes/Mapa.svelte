<script lang="ts">
  import { formatarCompacto } from '../lib/formato';
  import { aoClicar, base, Plot } from '../lib/plot';
  import { tema } from '../lib/tema.svelte';
  import type { GeoJSON } from '../lib/tipos';
  import Grafico from './Grafico.svelte';
  import Tabela from './Tabela.svelte';

  // Coroplético sequencial (um só tom, claro → escuro). Clicar numa UF a seleciona no filtro.
  let {
    geo,
    valores,
    siglas,
    selecionada,
    formatar,
    rotuloMetrica,
    aoSelecionar
  }: {
    geo: GeoJSON;
    valores: Map<number, number | null>;
    siglas: Map<number, string>;
    selecionada: number | null;
    formatar: (valor: number) => string;
    rotuloMetrica: string;
    aoSelecionar: (uf: number) => void;
  } = $props();

  const valor = (f: GeoJSON['features'][number]) => valores.get(f.properties.uf) ?? null;
  const rotulo = (f: GeoJSON['features'][number]) => {
    const v = valor(f);
    return `${siglas.get(f.properties.uf)}\n${v === null ? 'sem dado' : formatar(v)}`;
  };

  function desenhar(largura: number, altura: number) {
    const c = tema.cores;
    const selecionadas = geo.features.filter(f => f.properties.uf === selecionada);
    const figura = Plot.plot({
      ...base(largura, altura),
      marginLeft: 0,
      projection: { type: 'mercator', domain: geo },
      color: {
        type: 'linear',
        range: [c['seq-min'], c['seq-max']],
        interpolate: 'lab',
        legend: true,
        label: rotuloMetrica,
        tickFormat: formatarCompacto
      },
      marks: [
        Plot.geo(geo, { fill: valor, stroke: c.surface, strokeWidth: 1 }),
        Plot.geo(selecionadas, { stroke: c['text-primary'], strokeWidth: 2 }),
        Plot.tip(geo.features, Plot.pointer(Plot.centroid({ title: rotulo })))
      ]
    });
    aoClicar<GeoJSON['features'][number]>(figura, f => aoSelecionar(f.properties.uf));
    return figura;
  }
</script>

<Grafico {desenhar} altura={380} />
<Tabela
  colunas={['UF', rotuloMetrica]}
  linhas={[...siglas].sort((a, b) => a[1].localeCompare(b[1])).map(([uf, sigla]) => {
    const v = valores.get(uf) ?? null;
    return [sigla, v === null ? '—' : formatar(v)];
  })}
/>
