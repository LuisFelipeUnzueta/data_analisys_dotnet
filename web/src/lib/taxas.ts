import { FAIXA_IGNORADA, NUM_FAIXAS, type Dimensoes, type Filtro, type Metrica } from './tipos';

export const BASE_TAXA = 100_000;

/** Soma de óbitos e população de um recorte, por faixa etária. Mesmas regras de Dominio/Taxas.cs. */
export interface Acumulador {
  obitos: number;
  /** Óbitos com idade ignorada redistribuída (zero na faixa Ignorada); só para a taxa padronizada. */
  ajustados: Float64Array;
  populacao: Float64Array;
}

export const novoAcumulador = (): Acumulador => ({
  obitos: 0,
  ajustados: new Float64Array(NUM_FAIXAS),
  populacao: new Float64Array(NUM_FAIXAS)
});

/** Pesos da população-padrão indexados pela faixa (a faixa Ignorada tem peso zero). */
export function pesosDoPadrao(dimensoes: Dimensoes): Float64Array {
  const pesos = new Float64Array(NUM_FAIXAS);
  for (const { faixa, peso } of dimensoes.padrao.pesos) pesos[faixa] = peso;
  return pesos;
}

export function taxaBruta(acc: Acumulador): number | null {
  let populacao = 0;
  for (const p of acc.populacao) populacao += p;
  return populacao > 0 ? (acc.obitos / populacao) * BASE_TAXA : null;
}

/** Método direto: Σ peso·(óbitos/população) / Σ peso. Indefinida se faltar população numa faixa de peso positivo. */
export function taxaPadronizada(acc: Acumulador, pesos: Float64Array): number | null {
  let soma = 0;
  let somaPesos = 0;
  for (let f = 0; f < NUM_FAIXAS; f++) {
    const peso = pesos[f]!;
    if (f === FAIXA_IGNORADA || peso === 0) continue;
    const populacao = acc.populacao[f]!;
    if (populacao <= 0) return null;
    soma += (peso * acc.ajustados[f]!) / populacao;
    somaPesos += peso;
  }
  return somaPesos > 0 ? (soma / somaPesos) * BASE_TAXA : null;
}

/**
 * Pesos só das faixas do intervalo: a taxa padronizada de um recorte etário (por exemplo 20 a 59 anos) usa o padrão
 * restrito a essas faixas. Com uma só faixa, ela é igual à taxa específica.
 */
export function pesosDoIntervalo(pesos: Float64Array, faixa: Filtro['faixa']): Float64Array {
  if (faixa === null) return pesos;
  return pesos.map((peso, f) => (f >= faixa.inicio && f <= faixa.fim ? peso : 0));
}

export function valorMetrica(acc: Acumulador, metrica: Metrica, pesos: Float64Array): number | null {
  if (metrica === 'obitos') return acc.obitos;
  return metrica === 'bruta' ? taxaBruta(acc) : taxaPadronizada(acc, pesos);
}
