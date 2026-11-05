import { FAIXA_IGNORADA, type Dimensoes, type Filtro, type Local, type Metrica } from './tipos';

const METRICAS: Metrica[] = ['obitos', 'bruta', 'padronizada'];
const CODIGOS_SEXO = { m: 1, f: 2 } as const;

/** Recorte inicial da página: toda a série, Brasil, todas as causas e idades. */
export function filtroPadrao(dimensoes: Dimensoes): Filtro {
  return {
    anoInicio: dimensoes.anos[0]!.ano,
    anoFim: dimensoes.anos.at(-1)!.ano,
    local: { tipo: 'brasil' },
    grupo: null,
    sexo: null,
    faixa: null,
    metrica: 'padronizada'
  };
}

/** Query string do recorte, só com o que difere do padrão. A causa vai pelo código: o índice muda se `grupos.csv` for reordenado. */
export function filtroParaUrl(filtro: Filtro, dimensoes: Dimensoes): string {
  const padrao = filtroPadrao(dimensoes);
  const parametros = new URLSearchParams();
  if (filtro.anoInicio !== padrao.anoInicio) parametros.set('de', String(filtro.anoInicio));
  if (filtro.anoFim !== padrao.anoFim) parametros.set('ate', String(filtro.anoFim));
  const local = filtro.local;
  if (local.tipo === 'uf') parametros.set('uf', dimensoes.ufs.find(u => u.codigo === local.codigo)!.sigla);
  if (local.tipo === 'regiao') parametros.set('regiao', String(local.codigo));
  if (filtro.grupo !== null) parametros.set('causa', dimensoes.grupos[filtro.grupo]!.codigo);
  if (filtro.sexo !== null) parametros.set('sexo', filtro.sexo === CODIGOS_SEXO.m ? 'm' : 'f');
  if (filtro.faixa !== null) parametros.set('idade', `${filtro.faixa.inicio}-${filtro.faixa.fim}`);
  if (filtro.metrica !== padrao.metrica) parametros.set('metrica', filtro.metrica);
  const texto = parametros.toString();
  return texto === '' ? '' : `?${texto}`;
}

/** Lê o recorte da URL; o link pode ser antigo ou editado à mão, então o que não for válido volta ao padrão. */
export function filtroDaUrl(busca: string, dimensoes: Dimensoes): Filtro {
  const parametros = new URLSearchParams(busca);
  const padrao = filtroPadrao(dimensoes);
  const ano = (nome: string, alternativa: number) => {
    const valor = Number(parametros.get(nome));
    return dimensoes.anos.some(a => a.ano === valor) ? valor : alternativa;
  };
  const anoInicio = ano('de', padrao.anoInicio);
  const anoFim = ano('ate', padrao.anoFim);
  const sexo = parametros.get('sexo');
  return {
    ...(anoInicio <= anoFim ? { anoInicio, anoFim } : { anoInicio: padrao.anoInicio, anoFim: padrao.anoFim }),
    local: localDaUrl(parametros, dimensoes),
    grupo: dimensoes.grupos.find(g => g.codigo === parametros.get('causa'))?.indice ?? null,
    sexo: sexo === 'm' || sexo === 'f' ? CODIGOS_SEXO[sexo] : null,
    faixa: faixaDaUrl(parametros.get('idade')),
    metrica: METRICAS.find(m => m === parametros.get('metrica')) ?? padrao.metrica
  };
}

function localDaUrl(parametros: URLSearchParams, dimensoes: Dimensoes): Local {
  const uf = dimensoes.ufs.find(u => u.sigla === parametros.get('uf'));
  if (uf) return { tipo: 'uf', codigo: uf.codigo };
  const regiao = dimensoes.regioes.find(r => r.codigo === Number(parametros.get('regiao')));
  return regiao ? { tipo: 'regiao', codigo: regiao.codigo } : { tipo: 'brasil' };
}

/** "4-11" vira o intervalo; o intervalo completo é `null`, como no filtro da página. */
function faixaDaUrl(texto: string | null): Filtro['faixa'] {
  const achado = texto?.match(/^(\d+)-(\d+)$/);
  if (!achado) return null;
  const [inicio, fim] = [Number(achado[1]), Number(achado[2])];
  const completo = inicio === 0 && fim === FAIXA_IGNORADA - 1;
  return inicio <= fim && fim < FAIXA_IGNORADA && !completo ? { inicio, fim } : null;
}
