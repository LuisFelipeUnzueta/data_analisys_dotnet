import type { Agregado } from './agregacao';
import { estimativaMetrica, excesso, tendencia, type Estimativa, type ExcessoAno } from './estatistica';
import { pesosDoIntervalo, taxaBruta, type Acumulador } from './taxas';
import { FAIXA_IGNORADA, SEXO_FEMININO, SEXO_MASCULINO, type Dimensoes, type Filtro } from './tipos';

/** Ponto de uma série anual; `inferior`/`superior` formam o IC95% (iguais ao valor para contagens). */
export type PontoAnual = { ano: number; valor: number | null } & Partial<Omit<Estimativa, 'valor'>>;

/** Anos-base da linha de referência do excesso de mortalidade (antes da covid). */
export const ANOS_BASE_EXCESSO = [2015, 2016, 2017, 2018, 2019];

/** Transforma o agregado nos dados de cada visualização, já com a métrica do filtro aplicada. */
export function criarVistas(agregado: Agregado, filtro: Filtro, dimensoes: Dimensoes, pesos: Float64Array) {
  const pesosRecorte = pesosDoIntervalo(pesos, filtro.faixa);
  const estimar = (acc: Acumulador) => estimativaMetrica(acc, filtro.metrica, pesosRecorte);
  // Na pirâmide cada barra é uma faixa: taxas viram a taxa específica da faixa.
  const valorFaixa = (acc: Acumulador) => (filtro.metrica === 'obitos' ? acc.obitos : taxaBruta(acc));
  const anosDoPeriodo = dimensoes.anos.filter(a => a.ano >= filtro.anoInicio && a.ano <= filtro.anoFim);
  const doAno = (ano: number) => agregado.serie[ano - agregado.primeiroAno];

  const serie: PontoAnual[] = anosDoPeriodo.map(({ ano }) => {
    const e = estimar(doAno(ano)!);
    return e ? { ...e, ano } : { ano, valor: null };
  });
  // Com a maioria dos anos instáveis (ou sem óbitos), a regressão log-linear descarta os zeros e engana: não calcula.
  const instaveis = serie.filter(p => p.valor === null || p.valor === 0 || p.instavel).length;
  const tendenciaDoPeriodo = instaveis * 2 > serie.length ? null : tendencia(serie.flatMap(p =>
    p.valor !== null && p.varianciaLog !== undefined ? [{ ano: p.ano, valor: p.valor, varianciaLog: p.varianciaLog }] : []));

  const qualidade: PontoAnual[] = anosDoPeriodo.map(({ ano }) => {
    const t = ano - agregado.primeiroAno;
    const total = agregado.qualidade.total[t]!;
    return { ano, valor: total > 0 ? (100 * agregado.qualidade.malDefinidas[t]!) / total : null };
  });

  const porUf = new Map(dimensoes.ufs.map(uf => [uf.codigo, estimar(agregado.porUf[uf.codigo]!)]));

  const piramide = dimensoes.faixas
    .filter(f => f.indice !== FAIXA_IGNORADA)
    .map(f => ({
      faixa: f.rotulo,
      masculino: valorFaixa(agregado.piramide[SEXO_MASCULINO]![f.indice]!),
      feminino: valorFaixa(agregado.piramide[SEXO_FEMININO]![f.indice]!)
    }));
  const idadeIgnoradaNaPiramide = agregado.piramide.reduce((soma, porFaixa) => soma + porFaixa[FAIXA_IGNORADA]!.obitos, 0);

  const ranking = dimensoes.grupos
    .flatMap(g => {
      const e = estimar(agregado.porGrupo[g.indice]!);
      return e && e.valor > 0 ? [{ ...e, indice: g.indice, nome: g.nome }] : [];
    })
    .sort((a, b) => b.valor - a.valor)
    .slice(0, 10);

  // O excesso ignora o filtro de período: compara 2020 em diante com a tendência de 2015–2019 do mesmo recorte.
  const anosAlvo = dimensoes.anos.map(a => a.ano).filter(ano => ano > ANOS_BASE_EXCESSO.at(-1)!);
  const porAnoExcesso = anosAlvo.length > 0 ? excesso(doAno, ANOS_BASE_EXCESSO, anosAlvo) : null;
  const excessoPandemia = porAnoExcesso && {
    porAno: porAnoExcesso,
    observados: [...ANOS_BASE_EXCESSO, ...anosAlvo].map(ano => ({ ano, obitos: doAno(ano)!.obitos })),
    // Ajuste do próprio modelo nos anos-base, para a explicação mostrar a reta que é projetada para frente.
    ajusteBase: excesso(doAno, ANOS_BASE_EXCESSO, ANOS_BASE_EXCESSO) ?? [],
    total: somarExcessos(porAnoExcesso)
  };

  const totalObitos = anosDoPeriodo.reduce((soma, { ano }) => soma + doAno(ano)!.obitos, 0);

  return { serie, tendencia: tendenciaDoPeriodo, qualidade, porUf, piramide, idadeIgnoradaNaPiramide, ranking, excesso: excessoPandemia, totalObitos };
}

export type Vistas = ReturnType<typeof criarVistas>;

/** Excesso acumulado; as margens dos anos se combinam em quadratura (anos tratados como independentes). */
function somarExcessos(anos: ExcessoAno[]) {
  const excessoTotal = anos.reduce((s, a) => s + a.observado - a.esperado, 0);
  const margem = Math.sqrt(anos.reduce((s, a) => s + (a.superior - (a.observado - a.esperado)) ** 2, 0));
  const esperado = anos.reduce((s, a) => s + a.esperado, 0);
  return { excesso: excessoTotal, inferior: excessoTotal - margem, superior: excessoTotal + margem, esperado };
}
