import { novoAcumulador, type Acumulador } from './taxas';
import { MAIOR_CODIGO_UF, NUM_FAIXAS, type Base, type Dimensoes, type Filtro, type Local } from './tipos';

/**
 * Tudo que as visualizações precisam para um filtro. Cada visão ignora o próprio eixo do filtro
 * (o mapa não filtra UF, a pirâmide não filtra sexo nem faixa, o ranking não filtra grupo),
 * para mostrar o contexto do recorte escolhido.
 */
export interface Agregado {
  primeiroAno: number;
  /** Índice = ano - primeiroAno. */
  serie: Acumulador[];
  /** Índice = código IBGE da UF; período inteiro. */
  porUf: Acumulador[];
  /** [sexo][faixa] no ano final; cada acumulador tem uma só faixa preenchida. */
  piramide: Acumulador[][];
  /** Índice = grupo; período inteiro. */
  porGrupo: Acumulador[];
  /** Óbitos por ano, sem filtro de grupo: total e mal definidas, para o indicador de qualidade. */
  qualidade: { total: Float64Array; malDefinidas: Float64Array };
}

export function ufsDoLocal(dimensoes: Dimensoes, local: Local): Uint8Array {
  const marcadas = new Uint8Array(MAIOR_CODIGO_UF + 1);
  for (const uf of dimensoes.ufs) {
    const incluida = local.tipo === 'brasil'
      || (local.tipo === 'regiao' && uf.regiao === local.codigo)
      || (local.tipo === 'uf' && uf.codigo === local.codigo);
    if (incluida) marcadas[uf.codigo] = 1;
  }
  return marcadas;
}

const acumuladores = (n: number): Acumulador[] => Array.from({ length: n }, novoAcumulador);

export function agregar(base: Base, filtro: Filtro, indiceMalDefinidas: number): Agregado {
  const { dimensoes, obitos: o, populacao: p } = base;
  const primeiroAno = dimensoes.anos[0]!.ano;
  const ufs = ufsDoLocal(dimensoes, filtro.local);
  const { anoInicio, anoFim, grupo, sexo, faixa } = filtro;

  const resultado: Agregado = {
    primeiroAno,
    serie: acumuladores(dimensoes.anos.length),
    porUf: acumuladores(MAIOR_CODIGO_UF + 1),
    piramide: [acumuladores(NUM_FAIXAS), acumuladores(NUM_FAIXAS), acumuladores(NUM_FAIXAS)],
    porGrupo: acumuladores(dimensoes.grupos.length),
    qualidade: { total: new Float64Array(dimensoes.anos.length), malDefinidas: new Float64Array(dimensoes.anos.length) }
  };

  // Uma passada pelo cubo (~1,8 mi linhas). Cada condição abaixo é o filtro de uma visão.
  for (let i = 0; i < o.n; i++) {
    const ano = o.ano[i]!;
    const uf = o.uf[i]!;
    const g = o.grupo[i]!;
    const s = o.sexo[i]!;
    const f = o.faixa[i]!;
    const obitos = o.obitos[i]!;
    const ajustados = o.ajustados[i]!;

    const okUf = ufs[uf] === 1;
    const okSexo = sexo === null || s === sexo;
    const okFaixa = faixa === null || f === faixa;
    const okGrupo = grupo === null || g === grupo;
    const noPeriodo = ano >= anoInicio && ano <= anoFim;

    if (okUf && okSexo && okFaixa) {
      const t = ano - primeiroAno;
      if (okGrupo) somar(resultado.serie[t]!, f, obitos, ajustados);
      resultado.qualidade.total[t]! += obitos;
      if (g === indiceMalDefinidas) resultado.qualidade.malDefinidas[t]! += obitos;
      if (noPeriodo) somar(resultado.porGrupo[g]!, f, obitos, ajustados);
    }
    if (noPeriodo && okSexo && okFaixa && okGrupo) somar(resultado.porUf[uf]!, f, obitos, ajustados);
    if (ano === anoFim && okUf && okGrupo) somar(resultado.piramide[s]![f]!, f, obitos, ajustados);
  }

  // A população recebe os mesmos recortes (sem grupo: população não tem causa).
  const populacaoDoPeriodo = new Float64Array(NUM_FAIXAS);
  for (let i = 0; i < p.n; i++) {
    const ano = p.ano[i]!;
    const uf = p.uf[i]!;
    const s = p.sexo[i]!;
    const f = p.faixa[i]!;
    const habitantes = p.populacao[i]!;
    const okSexo = sexo === null || s === sexo;
    const okFaixa = faixa === null || f === faixa;
    const noPeriodo = ano >= anoInicio && ano <= anoFim;

    if (ufs[uf] === 1 && okSexo && okFaixa) {
      resultado.serie[ano - primeiroAno]!.populacao[f]! += habitantes;
      if (noPeriodo) populacaoDoPeriodo[f]! += habitantes;
    }
    if (noPeriodo && okSexo && okFaixa) resultado.porUf[uf]!.populacao[f]! += habitantes;
    if (ano === anoFim && ufs[uf] === 1) resultado.piramide[s]![f]!.populacao[f]! += habitantes;
  }
  for (const acc of resultado.porGrupo) acc.populacao.set(populacaoDoPeriodo);

  return resultado;
}

function somar(acc: Acumulador, faixa: number, obitos: number, ajustados: number): void {
  acc.obitos += obitos;
  acc.ajustados[faixa]! += ajustados;
}
