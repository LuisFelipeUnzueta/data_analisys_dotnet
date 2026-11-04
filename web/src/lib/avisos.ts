import type { Dimensoes, Filtro } from './tipos';

const UF_TOCANTINS = 17;
const UF_GOIAS = 52;
const REGIOES_COM_TO_OU_GO = new Set([1, 5]);

/** Avisos metodológicos que valem para o recorte atual, na ordem em que aparecem na página. */
export function avisosDoRecorte(filtro: Filtro, dimensoes: Dimensoes): string[] {
  const avisos: string[] = [];
  const cruza = (ano: number) => filtro.anoInicio < ano && filtro.anoFim >= ano;
  const grupo = filtro.grupo === null ? null : dimensoes.grupos[filtro.grupo];

  if (cruza(1996)) {
    avisos.push(
      grupo?.comparabilidade_reduzida
        ? `Comparabilidade reduzida: "${grupo.nome}" muda mais de 10% além da tendência na troca da CID-9 pela CID-10, em 1996. Compare os dois lados de 1996 com cautela.`
        : 'Em 1996 a causa básica passa a ser codificada pela CID-10. Os grupos de causa foram harmonizados entre as duas revisões.'
    );
  }

  const estimados = dimensoes.anos.filter(a => a.populacao_estimada && a.ano >= filtro.anoInicio && a.ano <= filtro.anoFim);
  if (filtro.metrica !== 'obitos' && estimados.some(a => a.fonte_populacao === 'extrapolada')) {
    avisos.push('A população de 1979 não tem fonte oficial: foi extrapolada do crescimento entre 1980 e 1981.');
  }

  const local = filtro.local;
  const afetaTocantins = local.tipo === 'brasil' ? false
    : local.tipo === 'uf' ? local.codigo === UF_TOCANTINS || local.codigo === UF_GOIAS
    : REGIOES_COM_TO_OU_GO.has(local.codigo);
  if (afetaTocantins && cruza(1989)) {
    avisos.push('Até 1988 o território do Tocantins era parte de Goiás; a partir de 1989 os dois são contados separadamente.');
  }

  if (filtro.metrica === 'padronizada' && filtro.faixa !== null) {
    avisos.push('Com uma faixa etária selecionada, a taxa padronizada equivale à taxa específica da faixa.');
  }

  return avisos;
}
