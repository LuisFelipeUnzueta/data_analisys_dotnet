import { existsSync, readFileSync } from 'node:fs';
import { describe, expect, it } from 'vitest';
import { agregar } from './agregacao';
import { avisosDoRecorte } from './avisos';
import { COLUNAS_OBITOS, COLUNAS_POPULACAO, lerParquet } from './dados';
import { novoAcumulador, taxaBruta, taxaPadronizada, valorMetrica } from './taxas';
import type { Base, Dimensoes, Filtro, TabelaObitos, TabelaPopulacao } from './tipos';
import { criarVistas } from './vistas';

const FAIXAS = 18;
const pesosUniformes = () => {
  const pesos = new Float64Array(FAIXAS);
  pesos.fill(1, 0, 17);
  return pesos;
};

const dimensoes: Dimensoes = {
  gerado_em: '',
  anos: [1995, 1996].map(ano => ({ ano, revisao_cid: ano < 1996 ? 9 : 10, fonte_populacao: 'interpolada', populacao_estimada: true })),
  regioes: [{ codigo: 3, nome: 'Sudeste' }, { codigo: 1, nome: 'Norte' }],
  ufs: [{ codigo: 35, sigla: 'SP', regiao: 3 }, { codigo: 12, sigla: 'AC', regiao: 1 }],
  grupos: [
    { indice: 0, codigo: 'isquemicas', nome: 'Isquêmicas', capitulo: 'circ', comparabilidade_reduzida: false },
    { indice: 1, codigo: 'mal_definidas', nome: 'Mal definidas', capitulo: 'md', comparabilidade_reduzida: true }
  ],
  faixas: Array.from({ length: FAIXAS }, (_, i) => ({ indice: i, rotulo: String(i), inicio: i * 5, fim: i * 5 + 4 })),
  sexos: [],
  padrao: { nome: 'teste', pesos: Array.from({ length: 17 }, (_, faixa) => ({ faixa, peso: 1 })) },
  marcos: []
};

/** Cubo mínimo: [ano, uf, grupo, sexo, faixa, óbitos] com ajustados = óbitos. */
function base(linhas: number[][], populacao: number[][]): Base {
  const col = <T extends Int16Array | Uint8Array | Int32Array | Float32Array>(Tipo: new (v: number[]) => T, i: number, l: number[][]) =>
    new Tipo(l.map(x => x[i]!));
  const obitos: TabelaObitos = {
    n: linhas.length, ano: col(Int16Array, 0, linhas), uf: col(Uint8Array, 1, linhas), grupo: col(Int16Array, 2, linhas),
    sexo: col(Uint8Array, 3, linhas), faixa: col(Uint8Array, 4, linhas), obitos: col(Int32Array, 5, linhas), ajustados: col(Float32Array, 5, linhas)
  };
  const pop: TabelaPopulacao = {
    n: populacao.length, ano: col(Int16Array, 0, populacao), uf: col(Uint8Array, 1, populacao), sexo: col(Uint8Array, 2, populacao),
    faixa: col(Uint8Array, 3, populacao), populacao: col(Int32Array, 4, populacao)
  };
  return { dimensoes, obitos, populacao: pop, geo: { type: 'FeatureCollection', features: [] } };
}

const filtro = (parcial: Partial<Filtro> = {}): Filtro => ({
  anoInicio: 1995, anoFim: 1996, local: { tipo: 'brasil' }, grupo: null, sexo: null, faixa: null, metrica: 'obitos', ...parcial
});

describe('taxas', () => {
  it('padronizada com taxa constante por faixa é igual a essa taxa', () => {
    const acc = novoAcumulador();
    for (let f = 0; f < 17; f++) {
      acc.populacao[f] = 1000 * (f + 1);
      acc.ajustados[f] = 5 * (f + 1);
      acc.obitos += 5 * (f + 1);
    }
    expect(taxaPadronizada(acc, pesosUniformes())).toBeCloseTo(500, 6);
    expect(taxaBruta(acc)).toBeCloseTo(500, 6);
  });

  it('padronizada fica indefinida sem população numa faixa de peso positivo', () => {
    const acc = novoAcumulador();
    acc.populacao[0] = 100;
    expect(taxaPadronizada(acc, pesosUniformes())).toBeNull();
  });

  it('com faixa filtrada a padronizada vira a taxa específica', () => {
    const acc = novoAcumulador();
    acc.obitos = 3;
    acc.populacao[4] = 1000;
    expect(valorMetrica(acc, 'padronizada', pesosUniformes(), true)).toBeCloseTo(300, 6);
  });
});

describe('agregação', () => {
  const cubo = base(
    [
      [1995, 35, 0, 1, 10, 10],
      [1995, 12, 1, 2, 10, 4],
      [1996, 35, 0, 2, 17, 2],
      [1996, 12, 0, 1, 10, 6]
    ],
    [
      [1995, 35, 1, 10, 1000],
      [1996, 35, 1, 10, 1000],
      [1995, 12, 1, 10, 500]
    ]
  );

  it('série respeita todos os filtros e o mapa ignora o filtro de UF', () => {
    const a = agregar(cubo, filtro({ local: { tipo: 'uf', codigo: 35 } }), 1);
    expect(a.serie[0]!.obitos).toBe(10);
    expect(a.serie[1]!.obitos).toBe(2);
    expect(a.porUf[12]!.obitos).toBe(10);
  });

  it('ranking ignora o filtro de grupo e a qualidade conta mal definidas', () => {
    const a = agregar(cubo, filtro({ grupo: 0 }), 1);
    expect(a.porGrupo[1]!.obitos).toBe(4);
    expect(a.qualidade.malDefinidas[0]).toBe(4);
    expect(a.qualidade.total[0]).toBe(14);
  });

  it('pirâmide usa o ano final e separa por sexo e faixa', () => {
    const a = agregar(cubo, filtro(), 1);
    expect(a.piramide[2]![17]!.obitos).toBe(2);
    expect(a.piramide[1]![10]!.obitos).toBe(6);
  });

  it('filtro de sexo exclui óbitos de outro sexo e de sexo ignorado', () => {
    const a = agregar(cubo, filtro({ sexo: 1 }), 1);
    expect(a.serie[0]!.obitos).toBe(10);
    expect(a.serie[1]!.obitos).toBe(6);
    expect(a.serie[0]!.populacao[10]).toBe(1500);
  });
});

describe('avisos', () => {
  it('marca comparabilidade reduzida quando o período cruza 1996', () => {
    expect(avisosDoRecorte(filtro({ grupo: 1 }), dimensoes)[0]).toContain('Comparabilidade reduzida');
    expect(avisosDoRecorte(filtro({ anoInicio: 1996, grupo: 1 }), dimensoes)).toEqual([]);
  });
});

// Confere o front contra o ETL (C#) com os dados reais, quando web/public/dados existe.
const dados = 'public/dados';
describe.skipIf(!existsSync(`${dados}/obitos.parquet`))('dados reais', () => {
  const ler = (arquivo: string) => {
    const b = readFileSync(`${dados}/${arquivo}`);
    return b.buffer.slice(b.byteOffset, b.byteOffset + b.byteLength) as ArrayBuffer;
  };

  it('taxa padronizada nacional bate com o ETL em 1979 e 2023', async () => {
    const real: Base = {
      dimensoes: JSON.parse(readFileSync(`${dados}/dimensoes.json`, 'utf8')) as Dimensoes,
      obitos: await lerParquet<TabelaObitos>(ler('obitos.parquet'), COLUNAS_OBITOS),
      populacao: await lerParquet<TabelaPopulacao>(ler('populacao.parquet'), COLUNAS_POPULACAO),
      geo: { type: 'FeatureCollection', features: [] }
    };
    const pesos = new Float64Array(FAIXAS);
    for (const { faixa, peso } of real.dimensoes.padrao.pesos) pesos[faixa] = peso;
    const f = filtro({ anoInicio: 1979, anoFim: 2023, metrica: 'padronizada' });
    const serie = criarVistas(agregar(real, f, -1), f, real.dimensoes, pesos).serie;

    expect(serie[0]!.valor).toBeCloseTo(889.0, 1);
    expect(serie.at(-1)!.valor).toBeCloseTo(560.9, 1);
  });
});
