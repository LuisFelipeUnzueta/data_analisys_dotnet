/**
 * Medição de desempenho da página (só sob demanda): `MEDIR=1 npx vitest run desempenho`.
 * Mede a leitura dos Parquet e, para vários filtros, a agregação (uma passada pelo cubo) e o cálculo das vistas.
 * Roda no V8 do Node, o mesmo motor do Chrome; grava o resultado em ../out/perfil-web.json para o README.
 */
import { existsSync, mkdirSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import { describe, it } from 'vitest';
import { agregar } from './agregacao';
import { COLUNAS_OBITOS, COLUNAS_POPULACAO, lerParquet } from './dados';
import { pesosDoPadrao } from './taxas';
import type { Base, Dimensoes, Filtro, TabelaObitos, TabelaPopulacao } from './tipos';
import { criarVistas } from './vistas';

const dados = 'public/dados';
const medir = process.env.MEDIR === '1' && existsSync(`${dados}/obitos.parquet`);

const ler = (arquivo: string) => {
  const b = readFileSync(`${dados}/${arquivo}`);
  return b.buffer.slice(b.byteOffset, b.byteOffset + b.byteLength) as ArrayBuffer;
};

/** Mediana de n execuções, em milissegundos. */
async function mediana(n: number, executar: () => unknown): Promise<number> {
  const tempos: number[] = [];
  for (let i = 0; i < n; i++) {
    const inicio = performance.now();
    await executar();
    tempos.push(performance.now() - inicio);
  }
  return tempos.sort((a, b) => a - b)[Math.floor(n / 2)]!;
}

describe.skipIf(!medir)('desempenho da página', () => {
  it('mede leitura, agregação e vistas', async () => {
    const dimensoes = JSON.parse(readFileSync(`${dados}/dimensoes.json`, 'utf8')) as Dimensoes;
    const leituraObitos = await mediana(5, () => lerParquet<TabelaObitos>(ler('obitos.parquet'), COLUNAS_OBITOS));
    const leituraPopulacao = await mediana(5, () => lerParquet<TabelaPopulacao>(ler('populacao.parquet'), COLUNAS_POPULACAO));

    const base: Base = {
      dimensoes,
      obitos: await lerParquet<TabelaObitos>(ler('obitos.parquet'), COLUNAS_OBITOS),
      populacao: await lerParquet<TabelaPopulacao>(ler('populacao.parquet'), COLUNAS_POPULACAO),
      geo: { type: 'FeatureCollection', features: [] }
    };
    const pesos = pesosDoPadrao(dimensoes);
    const primeiro = dimensoes.anos[0]!.ano;
    const ultimo = dimensoes.anos.at(-1)!.ano;
    const filtros: Filtro[] = [
      { anoInicio: primeiro, anoFim: ultimo, local: { tipo: 'brasil' }, grupo: null, sexo: null, faixa: null, metrica: 'padronizada' },
      { anoInicio: 2015, anoFim: 2019, local: { tipo: 'brasil' }, grupo: null, sexo: null, faixa: null, metrica: 'padronizada' },
      { anoInicio: primeiro, anoFim: ultimo, local: { tipo: 'uf', codigo: 35 }, grupo: 52, sexo: 1, faixa: null, metrica: 'bruta' },
      { anoInicio: 2000, anoFim: ultimo, local: { tipo: 'regiao', codigo: 2 }, grupo: null, sexo: 2, faixa: { inicio: 12, fim: 16 }, metrica: 'padronizada' },
      { anoInicio: primeiro, anoFim: ultimo, local: { tipo: 'uf', codigo: 14 }, grupo: 8, sexo: null, faixa: null, metrica: 'obitos' }
    ];

    const agregacao: number[] = [];
    const vistas: number[] = [];
    for (const filtro of filtros) {
      agregacao.push(await mediana(9, () => agregar(base, filtro, 0)));
      const agregado = agregar(base, filtro, 0);
      vistas.push(await mediana(9, () => criarVistas(agregado, filtro, dimensoes, pesos)));
    }

    const perfil = {
      linhasCubo: base.obitos.n,
      linhasPopulacao: base.populacao.n,
      bytes: Object.fromEntries(['obitos.parquet', 'populacao.parquet', 'dimensoes.json'].map(f => [f, statSync(`${dados}/${f}`).size])),
      leituraMs: { obitos: leituraObitos, populacao: leituraPopulacao },
      agregacaoMs: { mediana: [...agregacao].sort((a, b) => a - b)[2], maximo: Math.max(...agregacao) },
      vistasMs: { mediana: [...vistas].sort((a, b) => a - b)[2], maximo: Math.max(...vistas) },
      filtroCompletoMs: Math.max(...agregacao.map((a, i) => a + vistas[i]!)),
      linhasPorMs: base.obitos.n / Math.min(...agregacao)
    };
    mkdirSync('../out', { recursive: true });
    writeFileSync('../out/perfil-web.json', JSON.stringify(perfil, null, 2));
    console.log(perfil);
  }, 120_000);
});
