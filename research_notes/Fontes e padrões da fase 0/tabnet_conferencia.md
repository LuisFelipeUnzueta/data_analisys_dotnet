# Conferência do ETL SIM contra totais oficiais do DATASUS Tabnet (situação em 03/11/2026)

> Método: além da pesquisa web, as páginas do Tabnet foram baixadas e consultadas **diretamente** em 03/11/2026 (POST em `tabcgi.exe`, formato `prn`). Os números marcados "consulta direta" vêm dessas respostas e devem ser citados como "Tabnet/DATASUS, consulta em 03/11/2026". O resultado de um POST não tem URL própria; a fonte citada é o formulário (`deftohtm.exe?...`) que gera a consulta.

## 1. Quais páginas Tabnet existem para mortalidade geral, anos cobertos e agrupamentos

### Takeaway
Hoje há duas páginas de "Mortalidade geral" por UF: `sim/cnv/obt09uf.def` (CID-9, 1979–1995) e `sim/cnv/obt10uf.def` (CID-10, 1996–2026). A página CID-10 já mistura anos definitivos (até 2024), preliminar (2025) e prévia (2026) num único formulário. `obt9uf.def` não existe, e a página antiga de preliminares (`pobt10uf.def`) está vazia.

### Cited Findings
- **CID-10**: formulário `http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def`, título "Mortalidade - Brasil", que envia para `/cgi/tabcgi.exe?sim/cnv/obt10uf.def` (POST). A lista "Arquivos" vai de `obtuf96.dbf` a `obtuf26.dbf` (1996–2026), um arquivo por ano — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Rodapé da resposta (consulta direta em 03/11/2026): "Dados finais disponíveis até 2024 - data de extração 02/12/2025. Dados de 2025 - Preliminares - data de extração 28/07/2026. Dados de 2026 - 2a prévia - data de extração 01/09/2026." — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- **CID-9**: o formulário é `http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt09uf.def`, com arquivos `obtuf79.dbf` a `obtuf95.dbf` (1979–1995) e nota técnica `sim/Mortalidade_Geral_1979_1995.pdf` — [Tabnet obt09uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt09uf.def)
- O endereço `sim/cnv/obt9uf.def` (citado no pedido) devolve "Arquivo DEF não encontrado: sim/cnv/obt9uf.def". O nome correto é `obt09uf.def` — [Tabnet obt9uf (erro)](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt9uf.def)
- `sim/cnv/pobt10uf.def` ("Mortalidade - Brasil - Dados preliminares") responde "Nenhum dado disponível". Os preliminares foram incorporados ao `obt10uf.def` — [Tabnet pobt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/pobt10uf.def)
- **Variáveis CID-10, campo Linha**: Região; Região/Unidade da Federação; Unidade da Federação; Capítulo CID-10; Grupo CID-10; Categoria CID-10; Causa - CID-BR-10; Causa mal definidas; Ano do Óbito; Ano/mês do Óbito; Mês do Óbito; Faixa Etária; Faixa Etária OPS; Faixa Etária det; Fx.Etária Menor 1A; Sexo; Cor/raça; Escolaridade; Estado civil; Local ocorrência. **Conteúdo**: "Óbitos p/Residênc" ou "Óbitos p/Ocorrênc" — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- **Variáveis CID-9, campo Linha**: Região; Região/UF; UF; Capítulo CID-9; Categoria CID-9; Grupo - CID-9; Causa - CID-BR-9; Ano do Óbito; Faixa Etária (e as variantes OPS, det e Menor 1A); Sexo; Escolaridade; Estado civil; Local ocorrência. Não há Cor/raça nem Mês — [Tabnet obt09uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt09uf.def)
- Nota técnica CID-10 (o arquivo se chama `Mortalidade_Geral_1996_2012.pdf`, mas o título interno é "Mortalidade geral - 1996 a 2026"): a causa pode ser tabulada em "22 Capítulos, 264 Grupos, 2045 Categorias, 144 elementos da Lista CID-BR 28, 4 elementos da lista de causas mal definidas" — [Notas Técnicas CID-10](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1996_2012.pdf)
- A Lista CID-BR-10 "foi elaborada a partir da Lista de Tabulação da CID-BR-9", adaptada pela SVS — [Notas Técnicas CID-10](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1996_2012.pdf)
- Nota técnica CID-9: 17 capítulos, 911 categorias, 123 grupos e 130 elementos da lista CID-BR — [Notas Técnicas CID-9](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1979_1995.pdf)
- As duas notas dizem que "não foi possível gerar uma lista que as compatibilizasse" (CID-9 × CID-10) e que "não existe uma tabela de conversão direta" — [Notas CID-10](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1996_2012.pdf); [Notas CID-9](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1979_1995.pdf)
- Em jan/2025 o SIM passou a usar a CID-10 v-2019. Foram incluídos 186 códigos (marcados com "&" nos CNV), 235 descrições mudaram ("@") e 105 códigos foram excluídos mas mantidos para anos anteriores ("#", com ">>" indicando o substituto, ex.: A90 → A97) — [Notas Técnicas CID-10](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1996_2012.pdf)

### Inferences
- Como as listas CID-BR-9 (130 itens) e CID-BR-10 (144 itens) têm a mesma origem, elas servem de ponto de partida para grupos de causa harmonizados. A tabela de equivalência, porém, tem de ser construída pelo projeto, porque o DATASUS diz que não há conversão oficial.
- Para validar os capítulos do ETL, o ideal é cruzar "Capítulo CID-x" × Ano. Os capítulos CID-9 e CID-10 não se correspondem um a um (17 contra 22).
- A CID-10 v-2019 traz códigos novos (ex.: A97, U-codes) a partir de 2025. O ETL precisa aceitá-los sem descartar registros.

### Gaps
- Não encontrei página Tabnet nacional específica de "lista de mortalidade CID-9 BR" além da variável "Causa - CID-BR-9" do `obt09uf.def`.
- Não verifiquei as páginas por município (`obt10br.def`/`obt09br.def`?) nem as de óbitos fetais/infantis.

## 2. Como extrair totais por ano × UF de residência (e sexo, faixa etária, capítulo)

### Takeaway
Um POST com `Linha=Unidade_da_Federação`, `Coluna=Ano_do_Óbito`, `Incremento=Óbitos_p/Residênc`, uma linha `Arquivos=obtufAA.dbf` por ano, `formato=prn` e `zeradas=exibirlz`, com parâmetros codificados em **ISO-8859-1**, devolve uma tabela separada por ";" dentro do HTML. Não há API oficial.

### Cited Findings
- O formulário usa estes `name`: `Linha`, `Coluna`, `Incremento`, `Arquivos` (múltiplo), filtros `S<Variável>` (ex.: `SRegião`, `SUnidade_da_Federação`, `SCapítulo_CID-10`, `SSexo`, `SFaixa_Etária`, valor padrão `TODAS_AS_CATEGORIAS__`), `formato` (`table`/`pre`/`prn`, este com colunas separadas por ";"), `zeradas=exibirlz` (exibe linhas zeradas), `opcoes=ordenar` e `mostre=Mostra` — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Os valores dos campos são os rótulos com espaço trocado por "_" e com acentos (ex.: `Unidade_da_Federação`, `Ano_do_Óbito`, `Óbitos_p/Residênc`). A página é servida em `charset=iso-8859-1` — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Consulta direta (03/11/2026): o POST para `http://tabnet.datasus.gov.br/cgi/tabcgi.exe?sim/cnv/obt10uf.def` com os parâmetros acima respondeu em segundos para os 31 anos. Na resposta, as linhas de dados começam com aspas (`"11 Rondônia";4606;...`), os nomes vêm com entidades HTML (é preciso `html.unescape`), zero aparece como "-" e há uma linha `"Total"` — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- O filtro de sexo usa os códigos 1=Masc e 2=Fem. Na tabulação Ano × Sexo aparece a coluna "Ign" — consulta direta, [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Faixa Etária tem 13 categorias, de "Menor 1 ano" a "80 anos e mais", mais "Idade ignorada" — [Notas Técnicas CID-9](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1979_1995.pdf); consulta direta
- O Tabnet só cruza duas variáveis (linha × coluna); para mais dimensões é preciso usar filtros ou os microdados — [SciELO CSC](https://www.scielo.br/j/csc/a/3MNnCg5x96RTWkgpb3bZknx/?lang=pt)

Script mínimo, testado em 03/11/2026 com Python 3 e só a biblioteca padrão:
```python
import urllib.request, urllib.parse, html, re
p=[("Linha","Unidade_da_Federação"),("Coluna","Ano_do_Óbito"),("Incremento","Óbitos_p/Residênc")]
p+=[("Arquivos","obtuf%02d.dbf"%(y%100)) for y in range(1996,2027)]
p+=[("zeradas","exibirlz"),("formato","prn"),("mostre","Mostra")]
data=urllib.parse.urlencode(p,encoding="latin-1").encode()
r=urllib.request.urlopen(urllib.request.Request(
  "http://tabnet.datasus.gov.br/cgi/tabcgi.exe?sim/cnv/obt10uf.def",data=data),timeout=180).read().decode("latin-1")
linhas=[l for l in html.unescape(re.sub(r"<[^>]+>","",r)).splitlines() if l.startswith('"')]
```
Para CID-9 basta trocar o `.def` por `obt09uf.def` e os anos por 1979–1995.

### Inferences
- Em .NET: `FormUrlEncodedContent` codifica em UTF-8, então o corpo tem de ser montado à mão com `HttpUtility.UrlEncode(valor, Encoding.Latin1)`. Na leitura, decodificar com Latin1 e `WebUtility.HtmlDecode`. Não fazer `Split(';')` antes de decodificar, porque as entidades HTML (`&ocirc;`) contêm ";".
- Gerar os fixtures de conferência com uma consulta por dimensão (UF×Ano, Sexo×Ano, Faixa×Ano, Capítulo×Ano), guardando data de consulta e rodapé de extração, e comparar com o ETL agregado.

### Gaps
- Não verifiquei se há um limite de anos ou de células por consulta. As consultas com 31 anos × 28 UFs funcionaram.

## 3. Residência ignorada, sexo/idade ignorados e residência × ocorrência

### Takeaway
No Tabnet, a categoria "Ignorado/exterior" por **residência** fica com zero em todos os anos (CID-9 e CID-10), e o total nacional por residência é igual ao total por ocorrência. Sexo e idade ignorados aparecem como categorias próprias.

### Cited Findings
- Consulta direta, Região × Ano, CID-10 1996–2026: por residência, "0 Ignorado/Exterior" = "-" em todos os anos; por ocorrência há 1 (2003), 1 (2004) e 3 (2005), 5 no total. Os totais anuais por residência e por ocorrência são idênticos (ex.: 2022 = 1.544.266) — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Consulta direta, CID-9 1979–1995: por residência, Ignorado/Exterior = "-". Por ocorrência = 3.225 no total (2.437 em 1991, 691 em 1992 etc.). Total igual nas duas visões — [Tabnet obt09uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt09uf.def)
- Na consolidação "é feita ... a redistribuição dos óbitos pelo local de residência, a qual é a forma tradicional de apresentar os dados de Mortalidade" — [Notas Técnicas CID-10](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1996_2012.pdf)
- Sexo: "masculino, feminino e ignorado". Ignorado CID-10 em 1996 = 2.312, em 2022 = 626; CID-9 em 1979 = 1.040, em 1995 = 3.619 (consulta direta) — [Notas CID-9](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1979_1995.pdf); [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Idade ignorada (consulta direta): 1979 = 5.872; 1995 = 10.695; 1996 = 9.892; 2010 = 3.967; 2022 = 2.337 — [Tabnet obt09uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt09uf.def); [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Em 1979 não há óbitos de Tocantins ("-"), porque a UF só foi criada depois. Em 1995 Tocantins tem 3.277 (consulta direta) — [Tabnet obt09uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt09uf.def)

### Inferences
- **Não confirmado**: o zero em "residência ignorada" sugere que o Tabnet atribui uma residência a esses registros (provavelmente o local de ocorrência) ou os inclui num município ou UF, já que o total não diminui. Se os microdados tiverem `CODMUNRES` ignorado/estrangeiro, o ETL precisa da mesma regra (por exemplo, residência ignorada → ocorrência) para bater por UF. Os totais nacionais devem bater de qualquer forma.
- Os óbitos de residentes no exterior ocorridos no Brasil parecem distribuídos entre as UFs na visão por residência. É preciso medir isso nos microdados.

### Gaps
- Não encontrei documento oficial que descreva a regra exata de imputação da residência ignorada/exterior no Tabnet.

## 4. Automação: bibliotecas, exemplos e estabilidade

### Takeaway
A automação por POST funciona e é simples (seção 2). Em R, o pacote `datasus` (rpradosiqueira) está sendo reescrito, fora do CRAN, com API `sim()`. Não há limite de requisições documentado. O HTTPS do domínio estava recusando conexões em 03/11/2026, enquanto o HTTP funcionava.

### Cited Findings
- O pacote R `datasus` (GitHub rpradosiqueira/datasus) está "under active redevelopment and is not available from CRAN". Instalação: `remotes::install_github("rpradosiqueira/datasus")`. Tem a função `sim()` (`conjunto`, `linha`, `coluna`, `periodo`, `filtros`), descoberta por `datasus_catalogo("sim")`/`datasus_opcoes()`, `sim_microdados()`, as antigas `sim_obt10_mun()` (deprecadas), opções de timeout/retry/cache e um workflow agendado que verifica mudanças nos formulários do Tabnet — [GitHub datasus](https://github.com/rpradosiqueira/datasus)
- O código interno (`R/tabnet-client.R`) busca metadados em `deftohtm.exe` e consulta `tabcgi.exe` — [rdrr.io tabnet-client.R](https://rdrr.io/cran/datasus/src/R/tabnet-client.R)
- Alternativa para microdados em R: pacote `healthbR`, com vinheta SIM — [healthbR SIM vignette](https://cran.csiro.au/web/packages/healthbR/vignettes/sim-mortality.html)
- Em 03/11/2026, um WebFetch HTTPS para `tabnet.datasus.gov.br` falhou com "ECONNREFUSED 189.28.139.31:443", e o `curl` por HTTP funcionou — observação direta, [Tabnet](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)

### Inferences
- Implementar retry e timeout longo e aceitar HTTP e HTTPS. O próprio pacote `datasus` monitora mudanças no schema, o que indica que nomes de campos e de `.dbf` mudam de tempos em tempos (ex.: `pobt10uf` desativado). Guardar o HTML do formulário como fixture para detectar quebras.
- Sem limite de requisições documentado, fazer poucas consultas largas (todos os anos de uma vez) e respeitar um intervalo entre elas.

### Gaps
- Não encontrei biblioteca Python mantida e específica para o Tabnet. Não houve tempo de avaliar repositórios menores no GitHub.

## 5. Divergências Tabnet × microdados e fontes alternativas de validação

### Takeaway
A principal causa de divergência é a **data de extração**: Tabnet e arquivos abertos são gerados em momentos diferentes, e os anos 2025–2026 ainda estão em revisão. IBGE (Registro Civil) e o Painel de Monitoramento da SVS servem como validações independentes, mas não devem bater exatamente.

### Cited Findings
- Datas de extração no Tabnet: finais até 2024 em 02/12/2025, 2025 em 28/07/2026 e 2026 (2ª prévia) em 01/09/2026 — consulta direta, [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- O MS faz "algumas correções nas informações ... principalmente quanto ao cruzamento de causa de óbito por sexo e idade" — [Notas Técnicas CID-10](http://tabnet.datasus.gov.br/cgi/sim/Mortalidade_Geral_1996_2012.pdf)
- Exemplo de revisão após publicação: arquivos SIM de 2019 atualizados em 01/04/2020, com "alteração das causas básicas de 4 registros e exclusão de 1 registro" (relatório municipal de Londrina) — [Perfil de Londrina 2026](https://repositorio.londrina.pr.gov.br/index.php/menu-planejamento/diretoria-de-planejamento/gpi/perfil-de-londrina-2026/75648-4-2-6-7/file)
- `opendatasus.saude.gov.br/dataset/sim` redireciona (302) para `https://dadosabertos.saude.gov.br/` (novo portal) — [OpenDataSUS SIM](https://opendatasus.saude.gov.br/dataset/sim)
- Página gov.br de mortalidade (sem data visível, possivelmente desatualizada): Tabnet CID-10 de 1996 a 2023; Painel/IVIS de 1996 a 2022, com 2022 preliminar; arquivos .dbc de 2001 a 2022 — [gov.br Mortalidade](https://www.gov.br/saude/pt-br/acesso-a-informacao/sic/dados-em-transparencia-ativa/svsa/mortalidade)
- Painéis da SVS/DAENT: Painel de Monitoramento da Mortalidade CID-10; Mortalidade Prematura por DCNT; Infantil e Fetal; Materna; Garbage Codes; Regularidade da Mortalidade — [SVS painéis de mortalidade](https://svs.aids.gov.br/daent/centrais-de-conteudos/paineis-de-monitoramento/mortalidade/)
- IBGE Estatísticas do Registro Civil 2024: 1.495.386 óbitos registrados em 2024 (+4,6% sobre 2023, que teve 1.429.575) — [Bahia Notícias](https://www.bahianoticias.com.br/noticia/311400-brasil-registra-15-milhao-de-mortes-em-2024-maior-crescimento-em-20-anos); outro veículo cita 1,516 milhão — [Itatiaia](https://www.itatiaia.com.br/brasil/brasil-geral/numero-de-mortes-no-brasil-volta-a-crescer-depois-de-dois-anos-em-queda/) (conflito não resolvido; confirmar no SIDRA)
- IBGE/MS (estudo de sub-registro, divulgado em 20/05/2026): 3,4% dos óbitos de 2024 não foram registrados em cartório (eram 4,89% em 2015); 1.479.877 óbitos notificados e registrados; 52.059 só notificados pelo sistema de saúde — [InfoMoney](https://www.infomoney.com.br/brasil/sub-registro-de-nascimentos-e-obitos-cai-mas-34-das-mortes-nao-sao-registradas/)
- A base nacional do SIM tem defasagem de cerca de 14 meses (ficha técnica SES-SC) — [SES-SC ficha](https://saude.sc.gov.br/index.php/pt/servicos/planejamento-em-saude/indicadores-de-pactuacao-interfederativa/fichas-de-qualificacao-dos-indicadores-2017-2021/download)

### Inferences
- SIM 2024 (1.532.015) é cerca de 2,4% maior que o Registro Civil 2024 (1.495.386). Diferença esperada: o SIM inclui óbitos sem registro cartorial, e o IBGE conta registros feitos até o 1º trimestre do ano seguinte. O IBGE serve como sanity check de ordem de grandeza, não como conferência exata.
- Tolerância sugerida: igualdade exata para anos finais com mesma data de extração; para 2025 e 2026, conferir só a ordem de grandeza e registrar as datas.
- Os microdados SIM-DO **não incluem** óbitos fetais (que ficam em DOFET). Se o ETL ler os dois, deve excluir os fetais antes de comparar com a mortalidade geral. (Inferência baseada na estrutura do SIM; a nota técnica cita variáveis "específicas para óbitos fetais", mas o Tabnet de mortalidade geral não as inclui.)

### Gaps
- Não encontrei fonte oficial que compare totais do Tabnet com os CSV/DBC abertos nem que documente registros excluídos de uma das fontes.
- Não confirmei os nomes atuais dos arquivos no dadosabertos.saude.gov.br (ex.: `DO24OPEN.csv`) nem se 2025/2026 estão publicados lá.
- Não acessei o conteúdo interno do Painel CID-10 (anos e data de atualização).

## 6. Totais nacionais de referência (consulta direta ao Tabnet, 03/11/2026, óbitos por residência)

### Takeaway
Use estes números como gabarito do ETL: 1979 = 711.742; 1995 = 893.877; 1996 = 908.883; 2010 = 1.136.947; 2022 = 1.544.266; 2024 (último definitivo) = 1.532.015. Os totais de 1979–1995 somam 13.693.256 e os de 1996–2026 somam 36.724.005.

### Cited Findings
Fonte de todos os números: MS/SVSA/CGIAE – SIM, via [Tabnet obt09uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt09uf.def) (1979–1995) e [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def) (1996–2026), consulta direta em 03/11/2026.

| Ano | Total | Masc | Fem | Sexo ign | Status |
|---|---|---|---|---|---|
| 1979 | 711.742 | 409.211 | 301.491 | 1.040 | CID-9 |
| 1980 | 750.727 | 431.161 | 318.483 | 1.083 | CID-9 |
| 1985 | 788.231 | 462.075 | 325.266 | 890 | CID-9 |
| 1990 | 817.284 | 481.696 | 334.738 | 850 | CID-9 |
| 1995 | 893.877 | 521.430 | 368.828 | 3.619 | CID-9 |
| 1996 | 908.883 | 530.069 | 376.502 | 2.312 | CID-10 final |
| 2000 | 946.686 | 552.127 | 393.606 | 953 | final |
| 2010 | 1.136.947 | 649.378 | 487.137 | 432 | final |
| 2019 | 1.349.801 | 745.519 | 603.725 | 557 | final |
| 2020 | 1.556.824 | 874.167 | 682.027 | 630 | final |
| 2021 | 1.832.649 | 1.015.350 | 816.616 | 683 | final (pico COVID) |
| 2022 | 1.544.266 | 844.920 | 698.720 | 626 | final |
| 2023 | 1.465.610 | 803.200 | 661.884 | 526 | final |
| 2024 | 1.532.015 | 836.659 | 694.845 | 511 | final (extração 02/12/2025) |
| 2025 | 1.534.588 | 836.382 | 697.705 | 501 | preliminar (28/07/2026) |
| 2026 | 912.010 | 495.407 | 416.310 | 293 | 2ª prévia, ano incompleto (01/09/2026) |

- Série anual CID-10 completa (total Brasil, residência), 1996→2026: 908883; 903516; 931895; 938658; 946686; 961492; 982807; 1002340; 1024073; 1006827; 1031691; 1047824; 1077007; 1103088; 1136947; 1170498; 1181166; 1210474; 1227039; 1264175; 1309774; 1312663; 1316719; 1349801; 1556824; 1832649; 1544266; 1465610; 1532015; 1534588; 912010 — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Série CID-9 1979→1995: 711742; 750727; 750276; 741614; 771203; 809825; 788231; 811556; 799621; 834338; 815774; 817284; 803836; 827652; 878106; 887594; 893877 — [Tabnet obt09uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt09uf.def)
- Por região, residência, 2022: Norte 96.552; Nordeste 403.132; Sudeste 695.469; Sul 245.221; Centro-Oeste 103.892 — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- UF 2022 (exemplos): SP 354.056; MG 162.615; RJ 150.806; BA 107.641; RS 104.096; RR 3.246; AC 4.159 — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Capítulos CID-10, 2022: IX Circulatório 400.154; II Neoplasias 244.009; X Respiratório 176.073; XX Causas externas 152.945; I Infecciosas 131.800; XVIII Mal definidas 82.597. Capítulos XIX, XXI e XXII = 0 — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- Capítulos CID-9, 1995: VII Circulatório 244.602; XVI Mal definidas 144.830; XVII Causas externas 114.888; II Neoplasmas 99.058; VIII Respiratório 83.297 — [Tabnet obt09uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt09uf.def)
- Faixa etária, 2022: Menor 1 ano 32.257; 80+ 495.283; Idade ignorada 2.337 — [Tabnet obt10uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)

### Inferences
- A queda de "mal definidas" entre 1995 (16% dos óbitos no capítulo XVI) e 2022 (5% no capítulo XVIII) mostra que comparar causas entre CID-9 e CID-10 exige cuidado. É uma mudança de qualidade da informação, não epidemiológica.
- Os valores de 2025 e 2026 vão mudar. Os testes automatizados devem fixar só os anos ≤ 2024, sempre com a data de extração registrada.

### Gaps
- Não verifiquei se os microdados abertos de 2024 têm a mesma data de extração (02/12/2025) do Tabnet.
