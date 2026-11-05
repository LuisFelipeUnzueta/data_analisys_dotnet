import { describe, expect, it } from 'vitest';
import { destaqueDoPeriodo } from './vistas';

describe('destaque do período', () => {
  it('compara o primeiro e o último ano com valor', () => {
    const destaque = destaqueDoPeriodo([{ ano: 2000, valor: null }, { ano: 2001, valor: 200 }, { ano: 2002, valor: 150 }]);
    expect(destaque).toEqual({ inicio: { ano: 2001, valor: 200 }, fim: { ano: 2002, valor: 150 }, variacao: -25 });
  });

  it.each([
    ['um só ano', [{ ano: 2000, valor: 10 }]],
    ['início zerado', [{ ano: 2000, valor: 0 }, { ano: 2001, valor: 5 }]],
    ['ponta instável', [{ ano: 2000, valor: 10 }, { ano: 2001, valor: 5, instavel: true }]]
  ])('não destaca com %s', (_, serie) => {
    expect(destaqueDoPeriodo(serie)).toBeNull();
  });
});
