import { ufsDoLocal } from './agregacao';
import type { Dimensoes, Filtro } from './tipos';

type Marco = Dimensoes['marcos'][number];

/**
 * Marcos de `dimensoes.json` que afetam o recorte: o período cruza o ano e, nos territoriais, o local contém só parte
 * das UFs envolvidas (o Brasil inteiro, com GO e TO, não muda quando o Tocantins é separado de Goiás).
 */
export function marcosDoRecorte(filtro: Filtro, dimensoes: Dimensoes): Marco[] {
  const ufs = ufsDoLocal(dimensoes, filtro.local);
  return dimensoes.marcos.filter(m => {
    const cruza = filtro.anoInicio < m.ano && filtro.anoFim >= m.ano;
    const noLocal = m.ufs.filter(uf => ufs[uf] === 1).length;
    return cruza && (m.ufs.length === 0 || (noLocal > 0 && noLocal < m.ufs.length));
  });
}

/** Avisos metodológicos que valem para o recorte atual, na ordem em que aparecem na página. */
export function avisosDoRecorte(filtro: Filtro, dimensoes: Dimensoes): string[] {
  const avisos: string[] = [];
  const grupo = filtro.grupo === null ? null : dimensoes.grupos[filtro.grupo];

  for (const marco of marcosDoRecorte(filtro, dimensoes)) {
    if (marco.tipo === 'cid') {
      avisos.push(grupo?.comparabilidade_reduzida
        ? `Comparabilidade reduzida: "${grupo.nome}" muda mais de 10% além da tendência na troca da CID-9 pela CID-10, em ${marco.ano}. Compare os dois lados de ${marco.ano} com cautela.`
        : `Em ${marco.ano} a causa básica passa a ser codificada pela CID-10. Os grupos de causa foram harmonizados entre as duas revisões.`);
    } else {
      avisos.push(`Até ${marco.ano - 1} o território do Tocantins era parte de Goiás; a partir de ${marco.ano} os dois são contados separadamente.`);
    }
  }

  const extrapolados = dimensoes.anos.filter(a => a.fonte_populacao === 'extrapolada' && a.ano >= filtro.anoInicio && a.ano <= filtro.anoFim);
  if (filtro.metrica !== 'obitos' && extrapolados.length > 0) {
    avisos.push(`A população de ${extrapolados.map(a => a.ano).join(', ')} não tem fonte oficial: foi extrapolada do crescimento dos anos seguintes.`);
  }

  if (filtro.metrica === 'padronizada' && filtro.faixa !== null) {
    avisos.push(filtro.faixa.inicio === filtro.faixa.fim
      ? 'Com uma só faixa etária, a taxa padronizada equivale à taxa específica da faixa.'
      : 'A taxa padronizada usa os pesos da OMS só das faixas selecionadas; óbitos com idade ignorada ficam de fora do recorte etário.');
  }

  return avisos;
}
