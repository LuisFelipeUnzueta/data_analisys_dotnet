import { parquetMetadata, parquetRead } from 'hyparquet';
import { compressors } from 'hyparquet-compressors';
import type { Base, Dimensoes, GeoJSON, TabelaObitos, TabelaPopulacao } from './tipos';

type Colunas<T> = { [K in keyof T]: T[K] extends ArrayLike<number> ? new (n: number) => T[K] : never };

export const COLUNAS_OBITOS: Omit<Colunas<TabelaObitos>, 'n'> = {
  ano: Int16Array, uf: Uint8Array, grupo: Int16Array, sexo: Uint8Array, faixa: Uint8Array,
  obitos: Int32Array, ajustados: Float32Array
};

export const COLUNAS_POPULACAO: Omit<Colunas<TabelaPopulacao>, 'n'> = {
  ano: Int16Array, uf: Uint8Array, sexo: Uint8Array, faixa: Uint8Array, populacao: Int32Array
};

const baixar = async (url: string) => (await fetch(url)).arrayBuffer();

/** Lê um Parquet inteiro em arrays tipados, coluna a coluna, sem materializar linhas. */
export async function lerParquet<T extends { n: number }>(arquivo: ArrayBuffer, tipos: Omit<Colunas<T>, 'n'>): Promise<T> {
  const n = Number(parquetMetadata(arquivo).num_rows);
  const tabela = { n } as Record<string, unknown>;
  for (const [nome, Tipo] of Object.entries(tipos)) tabela[nome] = new (Tipo as new (n: number) => unknown)(n);

  await parquetRead({
    file: arquivo,
    compressors,
    onChunk: ({ columnName, columnData, rowStart }) => {
      const destino = tabela[NOMES[columnName] ?? columnName] as { set(fonte: ArrayLike<number>, inicio: number): void };
      destino.set(columnData as ArrayLike<number>, rowStart);
    }
  });
  return tabela as T;
}

/** Coluna do Parquet → campo da tabela, quando o nome difere. */
const NOMES: Record<string, string> = { obitos_ajustados: 'ajustados' };

export async function carregarBase(raiz = './dados'): Promise<Base> {
  const [dimensoes, geo, obitos, populacao] = await Promise.all([
    fetch(`${raiz}/dimensoes.json`).then(r => r.json() as Promise<Dimensoes>),
    fetch('./ufs.geojson').then(r => r.json() as Promise<GeoJSON>),
    baixar(`${raiz}/obitos.parquet`).then(b => lerParquet<TabelaObitos>(b, COLUNAS_OBITOS)),
    baixar(`${raiz}/populacao.parquet`).then(b => lerParquet<TabelaPopulacao>(b, COLUNAS_POPULACAO))
  ]);
  return { dimensoes, geo, obitos, populacao };
}
