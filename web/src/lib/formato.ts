import type { Dimensoes, Filtro, Metrica } from './tipos';

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

/** Nome da métrica como o leitor a vê; com uma só faixa etária, a padronizada é a taxa específica. */
export function nomeMetrica(filtro: Pick<Filtro, 'metrica' | 'faixa'>): string {
  if (filtro.metrica === 'obitos') return 'Óbitos';
  if (filtro.metrica === 'bruta') return 'Taxa bruta';
  const umaFaixa = filtro.faixa !== null && filtro.faixa.inicio === filtro.faixa.fim;
  return umaFaixa ? 'Taxa específica da faixa' : 'Taxa padronizada';
}

/** Nome da métrica com a unidade, para eixos, legendas e tabelas. */
export function rotuloMetrica(filtro: Pick<Filtro, 'metrica' | 'faixa'>): string {
  return filtro.metrica === 'obitos' ? 'Óbitos' : `${nomeMetrica(filtro)} por 100 mil hab.`;
}

/** "IC 95%: 81,4 a 121,6" para taxas; vazio para contagens (inferior = superior). */
export function formatarIntervalo(e: { inferior?: number; superior?: number } | null | undefined, formatar: (v: number) => string): string {
  if (!e || e.inferior === undefined || e.superior === undefined || e.inferior === e.superior) return '';
  return `IC 95%: ${formatar(e.inferior)} a ${formatar(e.superior)}`;
}

const sinal = (v: number) => `${v > 0 ? '+' : ''}${decimal.format(v)}`;

/** "+12,3%". */
export const formatarVariacao = (valor: number) => `${sinal(valor)}%`;

/** "−1,2% ao ano (IC 95%: −1,4 a −1,0)". */
export function formatarTendencia(t: { apc: number; inferior: number; superior: number }): string {
  return `${sinal(t.apc)}% ao ano (IC 95%: ${sinal(t.inferior)} a ${sinal(t.superior)})`;
}

/** "todas as idades", "20 a 59 anos", "60 anos ou mais". */
export function rotuloFaixas(faixas: Dimensoes['faixas'], faixa: Filtro['faixa']): string {
  if (faixa === null) return 'todas as idades';
  const inicio = faixas[faixa.inicio]!.inicio;
  const fim = faixas[faixa.fim]!.fim;
  return fim === null ? `${inicio} anos ou mais` : `${inicio} a ${fim} anos`;
}

/** Nomes do recorte, para o contexto dos gráficos e a barra de filtros ativos. */
export function rotulosDoRecorte(filtro: Filtro, dimensoes: Dimensoes) {
  const { local: lugar, grupo, sexo } = filtro;
  const local = lugar.tipo === 'brasil'
    ? 'Brasil'
    : lugar.tipo === 'uf'
      ? dimensoes.ufs.find(u => u.codigo === lugar.codigo)!.sigla
      : dimensoes.regioes.find(r => r.codigo === lugar.codigo)!.nome;
  const causa = grupo === null ? 'todas as causas' : dimensoes.grupos[grupo]!.nome;
  const idade = rotuloFaixas(dimensoes.faixas, filtro.faixa);
  const nomeSexo = dimensoes.sexos.find(s => s.codigo === sexo)?.nome ?? '';
  return { local, causa, idade, sexo: nomeSexo, contexto: [causa, local, idade, nomeSexo].filter(Boolean).join(' · ') };
}
