import type { Agregado } from './agregacao';
import { pesosDoIntervalo, taxaBruta, valorMetrica } from './taxas';
import { FAIXA_IGNORADA, SEXO_FEMININO, SEXO_MASCULINO, type Dimensoes, type Filtro } from './tipos';

/** Transforma o agregado nos dados de cada visualização, já com a métrica do filtro aplicada. */
export function criarVistas(agregado: Agregado, filtro: Filtro, dimensoes: Dimensoes, pesos: Float64Array) {
  const pesosRecorte = pesosDoIntervalo(pesos, filtro.faixa);
  const valor = (acc: Parameters<typeof valorMetrica>[0]) => valorMetrica(acc, filtro.metrica, pesosRecorte);
  // Na pirâmide cada barra é uma faixa: taxas viram a taxa específica da faixa.
  const valorFaixa = (acc: Parameters<typeof valorMetrica>[0]) => (filtro.metrica === 'obitos' ? acc.obitos : taxaBruta(acc));
  const anosDoPeriodo = dimensoes.anos.filter(a => a.ano >= filtro.anoInicio && a.ano <= filtro.anoFim);

  const serie = anosDoPeriodo.map(({ ano }) => ({ ano, valor: valor(agregado.serie[ano - agregado.primeiroAno]!) }));

  const qualidade = anosDoPeriodo.map(({ ano }) => {
    const t = ano - agregado.primeiroAno;
    const total = agregado.qualidade.total[t]!;
    return { ano, valor: total > 0 ? (100 * agregado.qualidade.malDefinidas[t]!) / total : null };
  });

  const porUf = new Map(dimensoes.ufs.map(uf => [uf.codigo, valor(agregado.porUf[uf.codigo]!)]));

  const piramide = dimensoes.faixas
    .filter(f => f.indice !== FAIXA_IGNORADA)
    .map(f => ({
      faixa: f.rotulo,
      masculino: valorFaixa(agregado.piramide[SEXO_MASCULINO]![f.indice]!),
      feminino: valorFaixa(agregado.piramide[SEXO_FEMININO]![f.indice]!)
    }));
  const idadeIgnoradaNaPiramide = agregado.piramide.reduce((soma, porFaixa) => soma + porFaixa[FAIXA_IGNORADA]!.obitos, 0);

  const ranking = dimensoes.grupos
    .map(g => ({ indice: g.indice, nome: g.nome, valor: valor(agregado.porGrupo[g.indice]!) }))
    .filter((g): g is { indice: number; nome: string; valor: number } => g.valor !== null && g.valor > 0)
    .sort((a, b) => b.valor - a.valor)
    .slice(0, 10);

  const totalObitos = anosDoPeriodo.reduce((soma, { ano }) => soma + agregado.serie[ano - agregado.primeiroAno]!.obitos, 0);

  return { serie, qualidade, porUf, piramide, idadeIgnoradaNaPiramide, ranking, totalObitos };
}

export type Vistas = ReturnType<typeof criarVistas>;
