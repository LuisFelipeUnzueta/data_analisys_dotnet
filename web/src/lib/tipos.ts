/** Formato de `dimensoes.json`, gerado pelo ETL (Cubo/GeradorDimensoes.cs). */
export interface Dimensoes {
  gerado_em: string;
  anos: {
    ano: number;
    revisao_cid: number;
    fonte_populacao: string;
    populacao_estimada: boolean;
    /** Taxas nacionais calculadas pelo ETL; o teste do front confere o próprio cálculo contra elas. */
    taxa_bruta_brasil: number | null;
    taxa_padronizada_brasil: number | null;
  }[];
  regioes: { codigo: number; nome: string }[];
  ufs: { codigo: number; sigla: string; regiao: number }[];
  grupos: { indice: number; codigo: string; nome: string; capitulo: string; comparabilidade_reduzida: boolean }[];
  faixas: { indice: number; rotulo: string; inicio: number | null; fim: number | null }[];
  sexos: { codigo: number; nome: string }[];
  padrao: { nome: string; pesos: { faixa: number; peso: number }[] };
  /** Marcos metodológicos; `ufs` vazio vale para todo o país. */
  marcos: { ano: number; tipo: 'cid' | 'territorio'; rotulo: string; descricao: string; ufs: number[] }[];
}

/** Colunas de `obitos.parquet`, uma posição por célula do cubo. */
export interface TabelaObitos {
  n: number;
  ano: Int16Array;
  uf: Uint8Array;
  grupo: Int16Array;
  sexo: Uint8Array;
  faixa: Uint8Array;
  obitos: Int32Array;
  ajustados: Float32Array;
}

/** Colunas de `populacao.parquet`. */
export interface TabelaPopulacao {
  n: number;
  ano: Int16Array;
  uf: Uint8Array;
  sexo: Uint8Array;
  faixa: Uint8Array;
  populacao: Int32Array;
}

export interface Base {
  dimensoes: Dimensoes;
  obitos: TabelaObitos;
  populacao: TabelaPopulacao;
  geo: GeoJSON;
}

export interface GeoJSON {
  type: 'FeatureCollection';
  features: { type: 'Feature'; properties: { uf: number }; geometry: unknown }[];
}

export type Metrica = 'obitos' | 'bruta' | 'padronizada';

export type Local = { tipo: 'brasil' } | { tipo: 'regiao'; codigo: number } | { tipo: 'uf'; codigo: number };

export interface Filtro {
  anoInicio: number;
  anoFim: number;
  local: Local;
  grupo: number | null;
  /** 1 masculino, 2 feminino; null = ambos (inclui sexo ignorado). */
  sexo: 1 | 2 | null;
  /** Intervalo de faixas (índices, inclusivo); null = todas, inclusive idade ignorada. */
  faixa: { inicio: number; fim: number } | null;
  metrica: Metrica;
}

/** Códigos fixos do cubo (ver dimensoes.json). */
export const NUM_FAIXAS = 18;
export const FAIXA_IGNORADA = 17;
export const MAIOR_CODIGO_UF = 53;
export const SEXO_MASCULINO = 1;
export const SEXO_FEMININO = 2;
