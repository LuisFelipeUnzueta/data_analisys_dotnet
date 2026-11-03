# População padrão para taxas de mortalidade padronizadas por idade (método direto), Brasil 1979–presente

> Notas de pesquisa, nov/2026. Escopo: site público interativo com mortalidade por UF, causa, sexo e faixas de 5 anos até 80+.
> Legenda: **[NÃO CONFIRMADO]** = item não verificado em fonte primária nesta pesquisa.

## 1. Opções de população padrão e pesos por faixa de 5 anos (com colapso em 80+)

### Takeaway
Há quatro famílias de opções: Segi/Doll (1960/1966), OMS 2000–2025 (Ahmad et al. 2001), Europeia 2013 (ESP 2013) e uma população brasileira fixa (Censo 2000, 2010 ou 2022). Uma novidade relevante para "late 2026" é que a **OMS publicou uma nova população padrão "WHO 2026" (2026–2050)** que substitui a de 2000–2025. Os pesos dela não foram extraídos nesta pesquisa. Para colapsar em 80+ basta **somar** os pesos das faixas 80–84, 85–89, 90–94, 95–99 e 100+, porque no método direto o peso de um grupo agregado é a soma dos pesos dos subgrupos.

### Cited Findings
- **OMS 2000–2025 (versão SEER\*Stat, por milhão):** 0–4: 88.569; 5–9: 86.870; 10–14: 85.970; 15–19: 84.670; 20–24: 82.171; 25–29: 79.272; 30–34: 76.073; 35–39: 71.475; 40–44: 65.877; 45–49: 60.379; 50–54: 53.681; 55–59: 45.484; 60–64: 37.187; 65–69: 29.590; 70–74: 22.092; 75–79: 15.195; 80–84: 9.097; 85–89: 4.398; 90–94: 1.500; 95–99: 400; 100+: 50. Total: 1.000.000. — [SEER – World (WHO 2000-2025) Standard](https://seer.cancer.gov/stdpopulations/world.who.html)
- Os percentuais originais da OMS (8,86; 8,69; …; 80–84: 0,91; 85–89: 0,44; 90–94: 0,15; 95–99: 0,04; 100+: 0,005) somam **100,035%**, não 100%. O SEER reescalou os valores para 1 milhão e ajustou 90–94 de 1.499 para 1.500 para fechar o total. — [SEER – WHO std](https://seer.cancer.gov/stdpopulations/world.who.html)
- No layout de 19 grupos o SEER publica a OMS 2000–2025 com 0: 17.917; 1–4: 70.652; …; 80–84: 9.097; **85+: 6.348**. Segi 1960: 0: 24.000; 1–4: 96.000; 5–9: 100.000; 10–14: 90.000; 15–19: 90.000; 20–24: 80.000; 25–29: 80.000; 30–34: 60.000; 35–39: 60.000; 40–44: 60.000; 45–49: 60.000; 50–54: 50.000; 55–59: 40.000; 60–64: 40.000; 65–69: 30.000; 70–74: 20.000; 75–79: 10.000; 80–84: 5.000; 85+: 5.000. Europeia "EU-27 + EFTA 2011–2030" (a ESP 2013): 0: 10.000; 1–4: 40.000; 5–9 a 15–19: 55.000 cada; 20–24 e 25–29: 60.000; 30–34: 65.000; 35–39 a 50–54: 70.000 cada; 55–59: 65.000; 60–64: 60.000; 65–69: 55.000; 70–74: 50.000; 75–79: 40.000; 80–84: 25.000; 85+: 25.000. Europeia antiga (escandinava 1960): 80–84: 10.000; 85+: 10.000. — [SEER – Standard Populations, 19 age groups](https://seer.cancer.gov/stdpopulations/stdpop.19ages.html)
- A referência original da OMS é o GPE Discussion Paper No. 31, *Age Standardization of Rates: A New WHO Standard* (Ahmad, Boschi-Pinto, Lopez, Murray, Lozano, Inoue, 2001). O paper revisa os padrões Segi ("mundial") e escandinavo ("europeu") e propõe uma estrutura etária que reflete a média mundial projetada para 2000–2025. — [WHO GPE Paper 31 (PDF)](https://cdn.who.int/media/docs/default-source/gho-documents/global-health-estimates/gpe_discussion_paper_series_paper31_2001_age_standardization_rates.pdf), via resumo de busca. O texto do PDF não pôde ser extraído (streams comprimidos).
- **Nova população padrão OMS 2026 (2026–2050):** o documento "Revised WHO standard population for calculating age-standardized rates" (série GHE, Technical Paper WHO/HSA/DDA/GHE/2026.4) foi derivado das projeções da ONU, destina-se ao uso nos próximos 25 anos (2026–2050) e substitui o padrão de 2001. Ele é apresentado por idade simples com grupo aberto final, e as taxas calculadas com ele "não são comparáveis" às calculadas com o padrão anterior ou com qualquer outro. — [WHO GHE 2023 – standard population (PDF)](https://cdn.who.int/media/docs/default-source/gho-documents/global-health-estimates/ghe2023_who_standard_population.pdf?sfvrsn=7aae2c85_1); [WHO GHE 2023 COD methods, Annex Table F (PDF)](https://cdn.who.int/media/docs/default-source/gho-documents/global-health-estimates/ghe2023_cod_methods.pdf?sfvrsn=4d284ba5_1). Ambos vistos só via resumo de busca. **[NÃO CONFIRMADO: pesos numéricos, data exata de publicação e grupo aberto final.]**
- O GBD usa um padrão próprio, a "GBD world population age standard", atualizado no GBD 2019. Ele dá cerca de 2,03% para menores de 1 ano e cerca de 7,91% para 1–4 anos. — [BES Journal, suplemento](https://www.besjournal.com/fileSWYXYHJKX/journal/article/file/41728a74-6f41-4ccf-9827-09fb4ae28cb2.pdf). **[NÃO CONFIRMADO: tabela completa.]**
- **Censo 2010, Brasil, população residente por grupo de idade (SIDRA, tabela 200):** total 190.755.799; 0–4: 13.806.733; 5–9: 14.967.767; 10–14: 17.167.135; 15–19: 16.986.788; 20–24: 17.240.864; 25–29: 17.102.917; 30–34: 15.744.616; 35–39: 13.888.191; 40–44: 13.008.496; 45–49: 11.834.647; 50–54: 10.134.322; 55–59: 8.284.433; 60–64: 6.503.287; 65–69: 4.852.789; 70–74: 3.744.738; 75–79: 2.570.686; 80–84: 1.661.523; 85–89: 816.803; 90–94: 319.959; 95–99: 96.430; 100+: 22.676. — [IBGE SIDRA API t/200](https://apisidra.ibge.gov.br/values/t/200/n1/all/v/93/p/2010/c2/0/c1/0/c58/all)

### Tabela consolidada de pesos (por 1.000.000), faixas de 5 anos até 80+

Os pesos de 80+ e 0–4 são somas feitas por mim a partir das fontes citadas. Os pesos do Censo 2010 vêm de população/total × 10⁶, arredondados; a soma arredondada pode diferir de 1.000.000 em poucas unidades.

| Faixa | OMS 2000–2025 (SEER) | Segi/Doll 1960/66 | ESP 2013 | Brasil Censo 2010 |
|---|---:|---:|---:|---:|
| 0–4 | 88.569 | 120.000 | 50.000 | 72.379 |
| 5–9 | 86.870 | 100.000 | 55.000 | 78.466 |
| 10–14 | 85.970 | 90.000 | 55.000 | 89.995 |
| 15–19 | 84.670 | 90.000 | 55.000 | 89.050 |
| 20–24 | 82.171 | 80.000 | 60.000 | 90.382 |
| 25–29 | 79.272 | 80.000 | 60.000 | 89.659 |
| 30–34 | 76.073 | 60.000 | 65.000 | 82.538 |
| 35–39 | 71.475 | 60.000 | 70.000 | 72.806 |
| 40–44 | 65.877 | 60.000 | 70.000 | 68.194 |
| 45–49 | 60.379 | 60.000 | 70.000 | 62.041 |
| 50–54 | 53.681 | 50.000 | 70.000 | 53.127 |
| 55–59 | 45.484 | 40.000 | 65.000 | 43.430 |
| 60–64 | 37.187 | 40.000 | 60.000 | 34.092 |
| 65–69 | 29.590 | 30.000 | 55.000 | 25.440 |
| 70–74 | 22.092 | 20.000 | 50.000 | 19.631 |
| 75–79 | 15.195 | 10.000 | 40.000 | 13.476 |
| **80+** | **15.445** (= 9.097 + 4.398 + 1.500 + 400 + 50) | **10.000** (5.000 + 5.000) | **50.000** (25.000 + 25.000) | **15.294** (2.917.391 hab.) |
| Total | 1.000.000 | 1.000.000 | 1.000.000 | ≈1.000.000 |

Fontes: [SEER WHO](https://seer.cancer.gov/stdpopulations/world.who.html), [SEER 19 ages](https://seer.cancer.gov/stdpopulations/stdpop.19ages.html), [SIDRA t/200](https://apisidra.ibge.gov.br/values/t/200/n1/all/v/93/p/2010/c2/0/c1/0/c58/all).

### Inferences
- **Colapso em 80+ (OMS):** usar 15.445 por milhão. Pelos percentuais originais seria 1,545%, mas eles somam 100,035%; por isso prefira a versão SEER por milhão, que fecha em 1.000.000 e é reprodutível. Se usar os percentuais originais, normalize dividindo pela soma (100,035). Documentar a versão adotada no site evita diferenças pequenas na 2ª casa decimal em relação a outros cálculos.
- Na OMS 2000–2025 o peso de 80+ (≈1,54%) é quase igual ao do Brasil 2010 (≈1,53%), mas os pesos das faixas jovens diferem bastante. A ESP 2013 dá 80+ ≈ 5%, mais de três vezes mais. A Segi dá 1%.
- A ESP 2013 vem de uma população envelhecida e pesa muito as idades altas. Por isso as taxas padronizadas por ela ficam bem maiores e mais dominadas por DCNT do idoso.

### Gaps
- Pesos numéricos da nova **OMS 2026 (2026–2050)**: os PDFs da OMS não puderam ser decodificados nesta sessão. É preciso abrir a Annex Table F do documento GHE 2023 COD methods ou o technical paper WHO/HSA/DDA/GHE/2026.4.
- Pesos do Censo 2022 por faixa de 5 anos até 100+: não extraídos (SIDRA, tabela do Censo 2022 por grupo de idade, a confirmar).
- Pesos da Segi original de 1960, antes da modificação de Doll: o SEER traz apenas a versão de uso corrente (80–84 e 85+ com 5.000 cada).

## 2. Prós e contras para comparar 1979–presente e UFs

### Takeaway
No método direto, qualquer padrão fixo garante comparabilidade interna, entre anos e entre UFs. A escolha afeta o **nível** das taxas, a **comparabilidade externa** e, em alguma medida, a **ordenação** e a **inclinação das tendências** quando as curvas etárias de mortalidade diferem entre populações. O padrão OMS 2000–2025 equilibra bem esses fatores para o Brasil: o peso dele em 80+ fica próximo do Brasil 2010 e ele é um padrão internacional muito difundido.

### Cited Findings
- O resultado da padronização direta "depende fortemente da população padrão escolhida" (material didático USP). — [USP e-Disciplinas](https://edisciplinas.usp.br/mod/resource/view.php?id=1644586)
- Doll (1969) já observava que a padronização não resolve o problema de a mortalidade/incidência variar com a idade de modo diferente entre países. — [Doll, Br J Cancer, PMC2008310](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC2008310/)
- O paper da OMS propôs o novo padrão porque os padrões Segi e escandinavo não refletiam a estrutura etária futura do mundo. A meta era refletir a estrutura média mundial de 2000–2025. — [WHO GPE Paper 31](https://cdn.who.int/media/docs/default-source/gho-documents/global-health-estimates/gpe_discussion_paper_series_paper31_2001_age_standardization_rates.pdf) (via resumo de busca)
- A OMS afirma que taxas com o padrão WHO 2026 não são comparáveis às calculadas com o padrão anterior ou outro padrão. — [WHO GHE 2023 standard population](https://cdn.who.int/media/docs/default-source/gho-documents/global-health-estimates/ghe2023_who_standard_population.pdf?sfvrsn=7aae2c85_1) (via resumo de busca)
- Estudo espanhol citado em resultado de busca: a escolha do padrão muda as taxas ajustadas por causa dos pesos dados às idades mais avançadas. — resumo de busca sem URL primária confiável. **[NÃO CONFIRMADO]**

### Quadro comparativo (inferência sobre os pesos da seção 1)

| Critério | OMS 2000–2025 | Segi/Doll | ESP 2013 | Brasil fixo (2010 ou 2022) | OMS 2026 |
|---|---|---|---|---|---|
| Comparabilidade internacional | Alta (OMS/GHE, muitos artigos) | Alta só em câncer (IARC/registros) | Europa (Eurostat) | Baixa | Futura; ainda pouca literatura |
| Comparabilidade com a literatura brasileira | Boa | Boa em câncer (INCA) | Baixa | Boa (DCNT/MS/SES) | Baixa por ora |
| Interpretabilidade para leitor brasileiro | Média | Baixa (população "jovem" de 1960) | Baixa (Europa envelhecida) | Alta ("como se o Brasil tivesse a estrutura de 2010") | Média |
| Peso 80+ | 1,54% | 1,0% | 5,0% | 1,53% (2010) | **[NÃO CONFIRMADO]**; provavelmente maior |
| Estabilidade / reprodutibilidade | Pesos públicos e fixos | Fixos | Fixos | 2010 fixo; 2022 também, mas exige escolher a tabela do IBGE | Novo; pode ter revisões |

### Inferences
- Para 1979–2024+ com envelhecimento forte, o padrão não "corrige" o envelhecimento de modo diferente; qualquer padrão fixo remove o efeito da estrutura etária. O que muda é o peso relativo de causas típicas do idoso no número final. Padrões mais velhos (ESP 2013) dão destaque a doenças cardiovasculares e neoplasias em idosos; padrões jovens (Segi) dão destaque a causas externas e mortalidade infantil.
- Um padrão Brasil 2022 seria mais "atual" para o leitor, mas quebraria a comparabilidade com a literatura que usa Brasil 2000 ou 2010. Entre os padrões brasileiros, o 2010 tem mais precedentes recentes (SES-SP, ver seção 3).
- A OMS 2026 só deve ser adotada se o projeto quiser alinhar com as estimativas GHE mais novas. Como ainda não há pesos confirmados nem literatura brasileira usando o padrão, é arriscado escolhê-la agora como padrão principal.

### Gaps
- Não foi encontrado estudo brasileiro que quantifique a sensibilidade de rankings de UF ou tendências 1979–2020 à escolha do padrão (OMS × Segi × Brasil).

## 3. O que fontes oficiais brasileiras e a literatura usam

### Takeaway
Não há padrão único no Brasil. O **INCA** oferece taxas ajustadas pela **população mundial** (Segi/Doll) e pela **população brasileira**. Boletins de DCNT (MS/SES) usam o **Censo 2010 brasileiro**. Muitos artigos usam a **OMS 2000–2025**. Estudos com GBD usam o padrão GBD ou relatam Doll. A metodologia formal do "Saúde Brasil" não foi confirmada.

### Cited Findings
- **INCA – Atlas On-line de Mortalidade:** a interface oferece ajuste pela "população mundial" e pela "população brasileira" (modelos 03, 05 e 08). O modelo 04 usa apenas a mundial. A seção de Notas Técnicas aparece vazia. — [INCA Atlas On-line de Mortalidade](https://mortalidade.inca.gov.br/MortalidadeWeb/)
- O informativo INCA "Atlas on-line de mortalidade: usos e aplicações" (2014) apresenta taxas ajustadas pela população mundial e pela população brasileira de 2000, segundo resumo de busca. — [Repositório INCA, coleção Informativo Vigilância do Câncer 2014](https://ninho.inca.gov.br/jspui/handle/123456789/7339). **[NÃO CONFIRMADO: ano da população brasileira usada hoje (2000 ou 2010).]**
- O INCA adota como referência a população mundial de Segi (1960) modificada por Doll et al. (1966), segundo boletim da Prefeitura de São Paulo sobre câncer. — [Boletim eletrônico SMS-SP n.04 Câncer](https://drive.prefeitura.sp.gov.br/cidade/secretarias/upload/saude/arquivos/boletimeletronico/n04cancer.pdf)
- O Atlas de Mortalidade por Câncer em Alagoas (1996–2013) apresenta taxas brutas e ajustadas pela população mundial de **Doll et al. (1966)** e da **OMS (2001)**. — [Atlas mortalidade câncer Alagoas](https://dados.al.gov.br/catalogo/dataset/6ebd5243-4534-4ae2-9ba7-6ea88323ee2a/resource/eea13b25-1b1c-4ea3-b25a-f6e2c753530f/download/documentotecnico08012015163700atlasdemortalidadeporcanceremalagoas1996a2013.pdf)
- O Atlas de mortalidade por câncer no Brasil 1979–1999 (INCA, 2002) é a publicação histórica que cobre o mesmo início de série (1979). — [INCA, Atlas 1979–1999 (PDF)](https://ninho.inca.gov.br/jspui/bitstream/123456789/8977/1/Atlas%20de%20mortalidade%20por%20c%c3%a2ncer%20no%20Brasil%201979-1999%20-%202002.pdf). O padrão usado não foi verificado no texto: **[NÃO CONFIRMADO]**.
- **SES-SP (CVE), boletim de mortalidade prematura por DCNT:** padronização direta com população padrão do **Brasil, Censo 2010**, para 30–69 anos. O boletim ressalta que taxas padronizadas servem apenas para comparação. — [Boletim CVE-SP DCNT mortalidade prematura](https://saude.sp.gov.br/resources/cve-centro-de-vigilancia-epidemiologica/areas-de-vigilancia/doencas-cronicas-nao-transmissiveis/dados/boletim_epidemio_dcnt_mortalidade_prematura.pdf)
- **PROADESS/Fiocruz:** de 2009 a 2014 usou padronização direta por sexo e idade para Brasil, regiões e UFs. Depois passou ao método **indireto** por causa de estratos pequenos. — [Nota Técnica PROADESS 1](https://www.proadess.icict.fiocruz.br/Nota%20Tecnica%20PROADESS_1_Padronizacao%20de%20indicadores.pdf)
- Estudo de DCNT 2000–2019 por UF (Arq Bras Cardiol) usou padronização direta com a **população Brasil 2000**. — [Arq Bras Cardiol (SciELO)](https://www.scielo.br/j/abc/a/YbbkGvFjdCgXVZdpn9SCzQb/?lang=en)
- Estudo GBD Brasil/UFs 1990–2019 (Ciência & Saúde Coletiva 2021) relatou como padrão a população mundial apresentada por Doll et al. — [Ciênc. saúde coletiva 2021;26(10):4483](https://scielosp.org/pdf/csc/2021.v26n10/4483-4496/pt)
- O GBD 2019 usa o padrão próprio "GBD world population age standard". — [BES Journal, suplemento](https://www.besjournal.com/fileSWYXYHJKX/journal/article/file/41728a74-6f41-4ccf-9827-09fb4ae28cb2.pdf)
- Estudo brasileiro sobre doença cerebrovascular padronizou em faixas de 5 anos (20+) pela "nova população-padrão mundial" da OMS. — [CORE, PDF](https://core.ac.uk/download/293605673.pdf)
- Estudo de DCV com SIM bruto × SIM corrigido × GBD 2017 por UF: ao analisar por UF, as tendências do SIM bruto se invertem em algumas regiões, e o uso do SIM sem correção pode levar a erros de interpretação. — [PMC8452190](https://pmc.ncbi.nlm.nih.gov/articles/PMC8452190)
- Para mortalidade por sexo/UF, o Boletim Epidemiológico SVS v.52 n.29 (mulheres 2010 e 2019) usa projeções populacionais 2010–2060. — [Boletim SVS v.52 n.29](https://ensino.ensp.fiocruz.br/perfil_de_mortalidade/documents/Boletim-epidemiologico-SVS-v.52-n.29.pdf)

### Inferences
- Em câncer, a comparação natural é com o INCA (Segi/Doll). Em DCNT e boletins oficiais, Brasil 2010. Em comparações internacionais gerais, OMS 2000–2025. Um site com vários grupos de causas precisa de um padrão único por coerência, e oferecer um segundo como opção atende a públicos específicos.

### Gaps
- Metodologia oficial do "Saúde Brasil" (MS/SVS): não localizei a nota metodológica que define a população padrão por edição. **[NÃO CONFIRMADO]**
- RIPSA/IDB: não confirmei se algum indicador do IDB publica taxas padronizadas e com qual padrão. O IDB tradicionalmente publica taxas brutas e proporções, mas isso é **[NÃO CONFIRMADO]** nesta pesquisa.
- Ano da "população brasileira" no Atlas INCA atual (2000 ou 2010).

## 4. Idade ignorada e apresentação de taxas padronizadas × brutas ao público

### Takeaway
A prática na literatura brasileira varia. Muitos estudos não redistribuem idade ignorada quando a proporção é baixa. Não encontrei diretriz oficial (MS/RIPSA) específica. Para comunicação pública, a recomendação recorrente é deixar claro que a taxa padronizada é um indicador **comparativo**, não a "taxa real".

### Cited Findings
- Estudo de DCNT no Brasil: a qualidade da informação é avaliada por causas mal definidas, códigos-lixo, sub-registro e idade/sexo ignorados. As correções de sexo e idade ignorados **não foram realizadas** porque o preenchimento era bom no período. — [Resultado de busca: PMC8159516 (PDF)](https://rcastoragev2.blob.core.windows.net/254f580eda612145ec874fb079f5eb63/PMC8159516.pdf) (atribuição exata ao documento **[NÃO CONFIRMADO]**)
- Estudos de DCNT redistribuem causas mal definidas por faixa etária e UF, citando Duncan et al. — [Resultado de busca: PMC8384283 (PDF)](https://rcastoragev2.blob.core.windows.net/658be56edc08756e0263e64ed68bc0e0/PMC8384283.pdf)
- "As taxas padronizadas devem ser utilizadas apenas para análises comparativas." — [Boletim CVE-SP DCNT](https://saude.sp.gov.br/resources/cve-centro-de-vigilancia-epidemiologica/areas-de-vigilancia/doencas-cronicas-nao-transmissiveis/dados/boletim_epidemio_dcnt_mortalidade_prematura.pdf)
- A tabela 200 do Censo 2010 traz "Idade ignorada" como "..." (sem valor), ou seja, o denominador censitário já vem sem idade ignorada. — [SIDRA t/200](https://apisidra.ibge.gov.br/values/t/200/n1/all/v/93/p/2010/c2/0/c1/0/c58/all)

### Inferences (proposta técnica, não fonte oficial)
- **Idade ignorada:** se esses óbitos forem excluídos, a taxa padronizada fica subestimada na proporção deles, porque o denominador não tem "ignorados". Proposta: redistribuir proporcionalmente à distribuição etária dos óbitos com idade conhecida, dentro do mesmo estrato (ano × UF × sexo × causa, ou um nível mais agregado se o estrato for pequeno), **antes** da padronização. Exibir a % de idade ignorada como indicador de qualidade, sobretudo nos anos 1979–1990 e em UFs do Norte/Nordeste. Sexo ignorado: excluir das séries por sexo e manter no total. As duas regras precisam ser documentadas.
- **Público:** mostrar a taxa bruta como padrão para "quantos morrem por 100 mil hoje" e a padronizada para comparar anos e UFs, com rótulo explícito (por exemplo, "padronizada por idade, população padrão OMS 2000–2025"), tooltip explicativo e alternância entre as duas. Para causa × UF ao longo de 1979–presente, alertar sobre causas mal definidas, mudança CID-9→CID-10 em 1996 e sub-registro (ver [PMC8452190](https://pmc.ncbi.nlm.nih.gov/articles/PMC8452190)).

### Gaps
- Não encontrei norma do MS/DATASUS/RIPSA sobre redistribuição de idade ignorada antes da padronização.
- A proporção histórica de óbitos com idade ignorada no SIM por ano/UF não foi levantada nesta pesquisa.

### Recomendação (síntese para o redator)
- **Padrão principal: OMS 2000–2025 (versão SEER por milhão), com 80+ = 15.445.** É internacional e amplamente usado em artigos brasileiros. Tem peso 80+ próximo da estrutura brasileira de 2010, o que evita exagerar causas do idoso. Os pesos são fixos e públicos, e ele não depende do envelhecimento observado entre 1979 e 2024.
- **Padrão alternativo opcional: Brasil Censo 2010** (pesos da tabela da seção 1), para dialogar com boletins MS/SES de DCNT e facilitar a leitura do público brasileiro. Em câncer, citar o INCA (Segi/Doll) como referência externa.
- **Monitorar a OMS 2026:** registrar em ADR a possível migração quando os pesos estiverem confirmados. Como as taxas não são comparáveis entre padrões, a migração exige recalcular toda a série e rotular a versão.
