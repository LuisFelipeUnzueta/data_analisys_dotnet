import { describe, expect, it } from 'vitest';
import { estimativaBruta, estimativaPadronizada, excesso, quantilQuiQuadrado, tendencia, valorAjustado } from './estatistica';
import { novoAcumulador, taxaPadronizada, type Acumulador } from './taxas';

const Z = 1.959963984540054;

function acumulador(porFaixa: [faixa: number, obitos: number, populacao: number][]): Acumulador {
  const acc = novoAcumulador();
  for (const [f, d, p] of porFaixa) {
    acc.obitos += d;
    acc.ajustados[f] = d;
    acc.populacao[f] = p;
  }
  return acc;
}

const pesosOms = () => {
  const pesos = new Float64Array(18);
  [88569, 86870, 85970, 84670, 82171, 79272, 76073, 71475, 65877, 60379, 53681, 45484, 37187, 29590, 22092, 15195, 15445]
    .forEach((p, f) => (pesos[f] = p));
  return pesos;
};

describe('quantil do qui-quadrado (Wilson–Hilferty)', () => {
  it('aproxima os valores tabelados', () => {
    expect(quantilQuiQuadrado(10, Z)).toBeCloseTo(20.483, 1);
    expect(quantilQuiQuadrado(10, -Z)).toBeCloseTo(3.247, 1);
    expect(quantilQuiQuadrado(200, Z)).toBeCloseTo(241.058, 1);
  });
});

describe('IC da taxa bruta (Poisson exato)', () => {
  it('100 óbitos em 100 mil: 100 [81,4; 121,6]', () => {
    const e = estimativaBruta(acumulador([[10, 100, 100_000]]))!;
    expect(e.valor).toBe(100);
    expect(e.inferior).toBeCloseTo(81.4, 0);
    expect(e.superior).toBeCloseTo(121.6, 0);
    expect(e.instavel).toBe(false);
  });

  it('sem óbitos o limite inferior é zero e a estimativa é instável', () => {
    const e = estimativaBruta(acumulador([[10, 0, 100_000]]))!;
    expect(e.inferior).toBe(0);
    expect(e.superior).toBeCloseTo(3.69, 1);
    expect(e.instavel).toBe(true);
  });
});

describe('IC da taxa padronizada (Fay e Feuer)', () => {
  it('com uma só faixa coincide com o IC exato de Poisson', () => {
    const acc = acumulador([[10, 50, 20_000]]);
    const pesos = new Float64Array(18);
    pesos[10] = 1;
    const padronizada = estimativaPadronizada(acc, pesos)!;
    const bruta = estimativaBruta(acc)!;
    expect(padronizada.valor).toBeCloseTo(bruta.valor, 9);
    expect(padronizada.inferior).toBeCloseTo(bruta.inferior, 9);
    expect(padronizada.superior).toBeCloseTo(bruta.superior, 9);
  });

  it('tem o mesmo valor que taxaPadronizada e um intervalo que contém o valor', () => {
    const acc = acumulador(Array.from({ length: 17 }, (_, f) => [f, 10 + 30 * f, 50_000 + 1_000 * f] as [number, number, number]));
    const e = estimativaPadronizada(acc, pesosOms())!;
    expect(e.valor).toBeCloseTo(taxaPadronizada(acc, pesosOms())!, 9);
    expect(e.inferior).toBeLessThan(e.valor);
    expect(e.superior).toBeGreaterThan(e.valor);
  });

  it('erro relativo alto marca a estimativa como instável', () => {
    const acc = acumulador([[0, 2, 100_000], [16, 30, 1_000]]);
    const pesos = new Float64Array(18);
    pesos[0] = 0.5;
    pesos[16] = 0.5;
    expect(estimativaPadronizada(acc, pesos)!.instavel).toBe(false);
    expect(estimativaPadronizada(acumulador([[0, 1, 100_000], [16, 3, 1_000]]), pesos)!.instavel).toBe(true);
  });
});

describe('tendência (APC)', () => {
  it('recupera uma queda exata de 2% ao ano', () => {
    const pontos = Array.from({ length: 10 }, (_, i) => ({ ano: 2000 + i, valor: 500 * 0.98 ** i, varianciaLog: 0.001 }));
    const t = tendencia(pontos)!;
    expect(t.apc).toBeCloseTo(-2, 6);
    expect(t.superior - t.inferior).toBeLessThan(1e-6);
    expect(valorAjustado(t, 2000)).toBeCloseTo(500, 6);
    expect(valorAjustado(t, 2009)).toBeCloseTo(500 * 0.98 ** 9, 6);
  });

  it('com ruído o intervalo contém a tendência e exige pelo menos 3 anos', () => {
    const ruido = [0.01, -0.02, 0.015, -0.01, 0.02, -0.015, 0.005, -0.005];
    const pontos = ruido.map((r, i) => ({ ano: 2010 + i, valor: 100 * 1.03 ** i * Math.exp(r), varianciaLog: 1e-4 }));
    const t = tendencia(pontos)!;
    expect(t.inferior).toBeLessThan(3);
    expect(t.superior).toBeGreaterThan(3);
    expect(tendencia(pontos.slice(0, 2))).toBeNull();
  });
});

describe('excesso de mortalidade', () => {
  // Uma faixa com taxa caindo 1% ao ano e população crescendo 2% ao ano; 2020 tem 1.000 óbitos a mais.
  const serie = (extra: number) => (ano: number) => {
    const t = ano - 2015;
    const populacao = 1_000_000 * 1.02 ** t;
    const obitos = populacao * 0.006 * 0.99 ** t + (ano === 2020 ? extra : 0);
    return acumulador([[12, obitos, populacao]]);
  };

  it('sem choque o excesso é praticamente zero', () => {
    const [a] = excesso(serie(0), [2015, 2016, 2017, 2018, 2019], [2020])!;
    expect(a!.observado - a!.esperado).toBeCloseTo(0, 3);
    expect(a!.inferior).toBeLessThan(0);
    expect(a!.superior).toBeGreaterThan(0);
  });

  it('recupera o choque de 1.000 óbitos em 2020', () => {
    const [a] = excesso(serie(1_000), [2015, 2016, 2017, 2018, 2019], [2020])!;
    expect(a!.observado - a!.esperado).toBeCloseTo(1_000, 3);
    expect(a!.inferior).toBeGreaterThan(0);
  });
});
