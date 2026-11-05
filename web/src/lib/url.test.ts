import { describe, expect, it } from 'vitest';
import type { Dimensoes, Filtro } from './tipos';
import { filtroDaUrl, filtroPadrao, filtroParaUrl } from './url';

const dimensoes = {
  anos: [2000, 2001, 2002].map(ano => ({ ano })),
  regioes: [{ codigo: 3, nome: 'Sudeste' }],
  ufs: [{ codigo: 33, sigla: 'RJ', regiao: 3 }],
  grupos: [{ indice: 0, codigo: 'circ' }, { indice: 1, codigo: 'agressoes' }]
} as unknown as Dimensoes;

describe('filtro na URL', () => {
  it('o recorte padrão não deixa nada na URL', () => {
    expect(filtroParaUrl(filtroPadrao(dimensoes), dimensoes)).toBe('');
    expect(filtroDaUrl('', dimensoes)).toEqual(filtroPadrao(dimensoes));
  });

  it.each<Partial<Filtro>>([
    { anoInicio: 2001, anoFim: 2001 },
    { local: { tipo: 'uf', codigo: 33 } },
    { local: { tipo: 'regiao', codigo: 3 } },
    { grupo: 1 },
    { sexo: 2 },
    { faixa: { inicio: 4, fim: 11 } },
    { metrica: 'obitos' }
  ])('ida e volta preserva %j', parte => {
    const filtro = { ...filtroPadrao(dimensoes), ...parte };
    expect(filtroDaUrl(filtroParaUrl(filtro, dimensoes), dimensoes)).toEqual(filtro);
  });

  it('usa nomes legíveis e estáveis', () => {
    const filtro: Filtro = { ...filtroPadrao(dimensoes), local: { tipo: 'uf', codigo: 33 }, grupo: 1 };
    expect(filtroParaUrl(filtro, dimensoes)).toBe('?uf=RJ&causa=agressoes');
  });

  it.each([
    '?de=1900',
    '?de=2002&ate=2000',
    '?uf=XX',
    '?regiao=9',
    '?causa=inexistente',
    '?sexo=x',
    '?idade=9-2',
    '?idade=0-16',
    '?idade=2-17',
    '?idade=abc',
    '?metrica=x'
  ])('valor inválido volta ao padrão: %s', busca => {
    expect(filtroDaUrl(busca, dimensoes)).toEqual(filtroPadrao(dimensoes));
  });
});
