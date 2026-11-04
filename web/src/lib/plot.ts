import * as Plot from '@observablehq/plot';
import { tema } from './tema.svelte';

/** Opções comuns a todos os gráficos: tipografia do sistema, eixos recessivos e grade em hairline. */
export function base(largura: number, altura: number): Plot.PlotOptions {
  const c = tema.cores;
  return {
    width: largura,
    height: altura,
    marginLeft: 56,
    style: { fontFamily: 'system-ui, -apple-system, "Segoe UI", sans-serif', fontSize: '12px', color: c['text-secondary'], background: 'transparent' },
    grid: false
  };
}

export const linhaDeGrade = () => Plot.gridY({ stroke: tema.cores.grid, strokeOpacity: 1 });

export const eixoBase = () => Plot.ruleY([0], { stroke: tema.cores.muted });

export { Plot };
