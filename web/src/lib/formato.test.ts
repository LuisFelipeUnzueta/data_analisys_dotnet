import { describe, expect, it } from 'vitest';
import { formatarVariacao, nomeMetrica, rotuloMetrica, rotulosDoRecorte } from './formato';
import type { Dimensoes, Filtro } from './tipos';

const dimensoes = {
  regioes: [{ codigo: 3, nome: 'Sudeste' }],
  ufs: [{ codigo: 33, sigla: 'RJ', regiao: 3 }],
  grupos: [{ indice: 0, nome: 'Agressões' }],
  sexos: [{ codigo: 1, nome: 'Masculino' }],
  faixas: [{ indice: 0, rotulo: '0-4', inicio: 0, fim: 4 }]
} as unknown as Dimensoes;

const filtro: Filtro = { anoInicio: 2000, anoFim: 2001, local: { tipo: 'brasil' }, grupo: null, sexo: null, faixa: null, metrica: 'bruta' };

describe('rótulos do recorte', () => {
  it('descreve o recorte padrão sem sexo', () => {
    expect(rotulosDoRecorte(filtro, dimensoes).contexto).toBe('todas as causas · Brasil · todas as idades');
  });

  it('descreve um recorte completo', () => {
    const recorte: Filtro = { ...filtro, local: { tipo: 'uf', codigo: 33 }, grupo: 0, sexo: 1, faixa: { inicio: 0, fim: 0 } };
    expect(rotulosDoRecorte(recorte, dimensoes).contexto).toBe('Agressões · RJ · 0 a 4 anos · Masculino');
  });

  it('usa o nome da região', () => {
    expect(rotulosDoRecorte({ ...filtro, local: { tipo: 'regiao', codigo: 3 } }, dimensoes).local).toBe('Sudeste');
  });
});

describe('nome da métrica', () => {
  it('com uma só faixa, a padronizada é a taxa específica', () => {
    expect(nomeMetrica({ metrica: 'padronizada', faixa: { inicio: 2, fim: 2 } })).toBe('Taxa específica da faixa');
    expect(rotuloMetrica({ metrica: 'padronizada', faixa: null })).toBe('Taxa padronizada por 100 mil hab.');
    expect(rotuloMetrica({ metrica: 'obitos', faixa: null })).toBe('Óbitos');
  });
});

describe('variação', () => {
  it('mostra o sinal', () => {
    expect(formatarVariacao(12.34)).toBe('+12,3%');
    expect(formatarVariacao(-36.9)).toBe('-36,9%');
  });
});
