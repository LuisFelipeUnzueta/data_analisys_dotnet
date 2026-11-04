import type { Metrica } from './tipos';

/** Texto de cada métrica, usado no painel "Como ler as métricas" e na linha sob a série temporal. */
export const DESCRICOES: Record<Metrica, { titulo: string; resumo: string; detalhe: string }> = {
  obitos: {
    titulo: 'Óbitos',
    resumo: 'Número de mortes registradas no recorte, contadas pela UF de residência.',
    detalhe:
      'Mostra o tamanho do problema, mas não serve para comparar lugares ou anos: uma UF com mais habitantes ' +
      'sempre terá mais óbitos, mesmo que o risco de morrer seja menor.'
  },
  bruta: {
    titulo: 'Taxa bruta',
    resumo: 'Óbitos divididos pela população do recorte, por 100 mil habitantes.',
    detalhe:
      'Corrige o tamanho da população e mede o risco médio de morrer no recorte. Mas depende da idade das pessoas: ' +
      'uma população mais velha tem taxa bruta maior mesmo que o risco em cada idade seja igual ou menor.'
  },
  padronizada: {
    titulo: 'Taxa padronizada por idade',
    resumo: 'Taxa que o recorte teria se tivesse a mesma estrutura de idade de uma população-padrão fixa.',
    detalhe:
      'Calcula a taxa de cada faixa etária e faz a média com os pesos da população-padrão da OMS (2000–2025). ' +
      'Assim, diferenças de idade entre lugares e anos deixam de pesar, e é a métrica certa para comparar UFs ' +
      'ou acompanhar a tendência ao longo do tempo. O número em si é teórico: serve para comparar, não para contar mortes.'
  }
};
