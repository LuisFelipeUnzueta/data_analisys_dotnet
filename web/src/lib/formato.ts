import type { Filtro, Metrica } from './tipos';

const inteiro = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 0 });
const decimal = new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 });
const compacto = new Intl.NumberFormat('pt-BR', { notation: 'compact', maximumFractionDigits: 1 });

export const formatarInteiro = (valor: number) => inteiro.format(valor);
export const formatarCompacto = (valor: number) => compacto.format(valor);
export const formatarPercentual = (valor: number) => `${decimal.format(valor)}%`;

export function formatarValor(valor: number | null, metrica: Metrica): string {
  if (valor === null) return '—';
  return metrica === 'obitos' ? inteiro.format(valor) : decimal.format(valor);
}

/** Nome da métrica como o leitor a vê; com faixa etária filtrada, a padronizada é a taxa específica. */
export function rotuloMetrica(filtro: Pick<Filtro, 'metrica' | 'faixa'>): string {
  if (filtro.metrica === 'obitos') return 'Óbitos';
  if (filtro.metrica === 'bruta') return 'Taxa bruta por 100 mil hab.';
  return filtro.faixa === null ? 'Taxa padronizada por idade por 100 mil hab.' : 'Taxa específica da faixa por 100 mil hab.';
}
