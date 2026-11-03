# SIM (Sistema de Informação sobre Mortalidade) — microdados CSV no OpenDATASUS (estado em 03/11/2026)

> Método: além de buscas web, os metadados foram lidos diretamente do portal (página `dadosabertos.saude.gov.br/dataset/sim`, JSON `__NEXT_DATA__` embutido), dos cabeçalhos HTTP (HEAD) dos ZIPs no S3 e de **leituras parciais (HTTP Range, só em memória) do início de cada ZIP** de 1979 a 2026 (cabeçalho do CSV + ~5–50 mil primeiras linhas). Afirmações marcadas "(observado na amostra)" vêm dessas leituras, não da documentação oficial, e valem apenas para o início de cada arquivo (as linhas estão ordenadas aproximadamente por UF/município, então a amostra é enviesada para RO/AC/AM).
> Fonte do dataset: https://dadosabertos.saude.gov.br/dataset/sim (o antigo `https://opendatasus.saude.gov.br/dataset/sim` hoje responde 302 para `https://dadosabertos.saude.gov.br`).

## 1. Último ano disponível, definitivo vs. preliminar, frequência de atualização e defasagem frente ao Tabnet

### Takeaway
O CSV do OpenDATASUS cobre **1979–2026**: 1979–2024 são dados finais (definitivos), 2025 é "preliminar" e 2026 é "2ª prévia". Em relação ao Tabnet/FTP, o CSV **não fica para trás no ano mais recente** (os três mostram até 2026), mas a versão de 2025 no CSV (atualizada em 15/04/2026) é anterior à extração usada no Tabnet (28/07/2026) e no FTP (30/08/2026). Não existe calendário fixo: as atualizações vêm em lotes.

### Cited Findings
- O dataset "Sistema de Informação sobre Mortalidade – SIM" (id `sim`, licença CC-BY, 146 recursos) teve `metadata_modified` em 2026-10-29. Ele descreve o conteúdo como "bancos de dados de disseminação de mortalidade (DO_BDD)", com óbitos fetais e não fetais — [Portal Dados Abertos SUS – SIM](https://dadosabertos.saude.gov.br/dataset/sim)
- Os recursos CSV se chamam "Mortalidade Geral 1979" … "Mortalidade Geral 2024", "**Mortalidade Geral 2025 - preliminar**" e "**Mortalidade Geral 2026 2ª prévia**". Para cada ano também há XML e JSON — [Portal Dados Abertos SUS – SIM](https://dadosabertos.saude.gov.br/dataset/sim)
- `last_modified` de cada recurso CSV no CKAN:
  - 1979–1982: 2025-12-15;
  - 1983–2021: 2026-09-14;
  - 2022 e 2023: 2026-10-29;
  - 2024: 2025-11-11;
  - 2025 (preliminar): 2026-04-15;
  - 2026: sem valor (null).

  — [Portal Dados Abertos SUS – SIM](https://dadosabertos.saude.gov.br/dataset/sim)
- O `Last-Modified` (HTTP) dos objetos no S3 é 31/08/2026 para quase todos os anos (reenvio em lote). As exceções são 2022 e 2023 (29/10/2026) e 2026 (02/10/2026). Exemplo: https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/Mortalidade_Geral_2024_csv.zip (HEAD em 03/11/2026)
- Tabnet (Óbitos CID-10 por UF): "Dados finais disponíveis até 2024 - data de extração 02/12/2025", "Dados de 2025 - Preliminares - data de extração 28/07/2026", "Dados de 2026 - 2a prévia - data de extração 01/09/2026" — [Tabnet SIM CID-10](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def)
- No FTP do DATASUS, `CID10/DORES` (dados finais) vai até 2024: DOBR2024.dbc tem data de 23/12/2025 e 137,7 MB. Já `PRELIM/DORES` contém 2025 e 2026: DOBR2025.dbc de 30/08/2026 (125,3 MB) e DOBR2026.dbc de 28/09/2026 (73,4 MB) — [FTP DATASUS SIM](ftp://ftp.datasus.gov.br/dissemin/publicos/SIM/)
- Uma API REST de mortalidade também é listada como recurso: https://apidadosabertos.saude.gov.br/v1/#/Vigilância%20e%20Meio%20Ambiente/get_vigilancia_e_meio_ambiente_sistema_de_informacao_sobre_mortalidade — [Portal Dados Abertos SUS – SIM](https://dadosabertos.saude.gov.br/dataset/sim)

### Inferences
- A linha do tempo do CSV de 2024 é incoerente. O CKAN marca 2024 em 11/11/2025, antes da extração "final" do Tabnet (02/12/2025) e do DOBR2024 definitivo no FTP (23/12/2025). Ou seja, o CSV de 2024 pode ter sido gerado de uma base anterior ao fechamento definitivo, mesmo sem o sufixo "preliminar". Para validar o ETL, comparar os totais de 2024 do CSV com o Tabnet.
- Não há uma cadência regular. O padrão observado é: o ano N fecha como definitivo por volta de dezembro de N+1 a janeiro de N+2, e saem prévias/preliminares de N+1 e N+2 ao longo do ano. Os anos já fechados também são regravados de vez em quando (2022–2023 foram republicados em 29/10/2026). Por isso o ETL deve guardar hash/ETag/Last-Modified de cada ZIP e reprocessar o que mudou.
- Para a série: tratar 2025 e 2026 como provisórios e marcar `status` (definitivo/preliminar/prévia) a partir do nome do recurso no CKAN.

### Gaps
- Não achei nota oficial que fixe a frequência de atualização dos CSVs nem uma data oficial de "fechamento" de 2024 no OpenDATASUS.
- Não confirmei se o CSV 2024 é idêntico ao DOBR2024.dbc definitivo do FTP (nem contagem de linhas comparada).

## 2. URLs de download, padrão de nomes e cobertura CID-9 (1979–1995)

### Takeaway
Padrão único para todos os anos (1979–2026): `https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/Mortalidade_Geral_{AAAA}_csv.zip`. Cada ZIP traz um único arquivo `Mortalidade_Geral_{AAAA}.csv` (deflate). **Os anos CID-9 (1979–1995) estão publicados em CSV** no OpenDATASUS, com layout próprio. Nomes antigos como `DO79OPEN.csv`/`DOxxOPEN.csv` não aparecem no dataset atual.

### Cited Findings
- URLs CSV, de 1979 até 2026: `https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/Mortalidade_Geral_1979_csv.zip` … `Mortalidade_Geral_2026_csv.zip`. Os equivalentes em `.../SIM/json/Mortalidade_Geral_{AAAA}_json.zip` e `.../SIM/xml/Mortalidade_Geral_{AAAA}_xml.zip` — [Portal Dados Abertos SUS – SIM](https://dadosabertos.saude.gov.br/dataset/sim)
- Conteúdo de cada ZIP (observado no diretório central do ZIP via Range): uma entrada `Mortalidade_Geral_{AAAA}.csv`, método 8 (deflate). Exemplo de URL: https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/Mortalidade_Geral_1979_csv.zip
- Dicionário oficial publicado no dataset: https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/Dicionario_SIM_2025.pdf ("Estrutura do SIM", 9 páginas, "Arquivo atualizado em 06/2025", CGIAE/DASNT/SVS) — [Portal Dados Abertos SUS – SIM](https://dadosabertos.saude.gov.br/dataset/sim)
- Fonte alternativa (DBC) no FTP:
  - `ftp://ftp.datasus.gov.br/dissemin/publicos/SIM/CID9/DORES/DOR{UF}{AA}.DBC`, de 1979 a 1995, incluindo `DORBR{AA}.dbc` (Brasil);
  - `.../CID10/DORES/DO{UF}{AAAA}.dbc`, de 1996 a 2024, incluindo `DOBR{AAAA}.dbc`;
  - `.../PRELIM/DORES/DO{UF}{AAAA}.dbc`, para 2025 e 2026;
  - pastas extras: `CID9/DOFET`, `CID9/DOIGN` e `CID10/DOFET` (DOFET, DOEXT, DOINF, DOMAT).

  — [FTP DATASUS SIM](ftp://ftp.datasus.gov.br/dissemin/publicos/SIM/)

### Inferences
- O ETL pode montar as URLs pelo padrão. Mesmo assim, o mais robusto é ler a lista de recursos da página do dataset, para captar o rótulo preliminar/prévia e novos anos. A API CKAN `/api/3/action/package_show` não respondeu JSON no domínio novo (devolveu a página HTML do Next.js); os metadados vieram do JSON `__NEXT_DATA__` da página.
- Em todos os anos amostrados, TIPOBITO = 2 (não fetal). É provável que "Mortalidade Geral" seja só a base de óbitos não fetais (DORES), mas não confirmei no arquivo inteiro. **Mesmo assim, filtrar TIPOBITO = 2.**

### Gaps
- Não encontrei registro histórico dos nomes `DO79OPEN.csv`/`DOxxOPEN.csv`. Se existiram, foram substituídos pelo padrão `Mortalidade_Geral_*`.
- A presença de óbitos fetais no CSV não foi verificada no arquivo inteiro.

## 3. Layout dos campos e mudanças ao longo dos anos

### Takeaway
Em todos os anos o CSV usa `;` como separador, valores entre aspas duplas (campos vazios ficam sem aspas), quebra de linha CRLF e cabeçalho na primeira linha. Há **dois layouts principais**:
- **CID-9 (1979–1995):** `MUNIRES`, `DATAOBITO`, `CAUSABAS` CID-9 com 4 caracteres;
- **CID-10 (1996+):** `CODMUNRES`, `DTOBITO`, `CAUSABAS` CID-10.

Dentro de cada layout, colunas são acrescentadas ou reordenadas em vários anos. A partir de 2022 os nomes das colunas variam de caixa. **Ler as colunas pelo nome, sem diferenciar maiúsculas de minúsculas, e nunca pela posição.**

### Cited Findings — estrutura física (observado na amostra de cada arquivo)
- Separador `;`, aspas `"`, CRLF. Primeira linha de 1979: `"CONTADOR";"REGISTRO";"DATAREG";"TIPOBITO";"DATAOBITO";...;"MUNIRES";...;"CAUSABAS";...;"UFINFORM"` — https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/Mortalidade_Geral_1979_csv.zip
- Número de colunas e anos em que o cabeçalho muda (observado). Fonte: ZIPs `Mortalidade_Geral_{AAAA}_csv.zip` em https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/

  | Anos | Nº de colunas | Mudança no cabeçalho |
  |---|---|---|
  | 1979–1994 | 46 | — |
  | 1995 | 48 | + RACACOR, ETNIA |
  | 1996–2000 | 39 | layout CID-10 |
  | 2001–2005 | 42 | + CODESTAB, ATESTANTE, UFINFORM |
  | 2006–2008 | 52 | + HORAOBITO, CAUSABAS_O, CB_PRE, TPPOS etc. |
  | 2009 | 53 | — |
  | 2010 | 56 | + ORIGEM |
  | 2011 | 62 | — |
  | 2012 | 76 | — |
  | 2013 | 72 | — |
  | 2014–2017 | 87 | — |
  | 2018–2021 | 87 | CONTADOR passa a ser a **última** coluna |
  | 2022 | 86 | `contador` em minúsculas; CAUSABAS_O, TPPOS, TP_ALTERA e CB_ALT no fim |
  | 2023 | 86 | **todos os nomes em minúsculas** (`causabas`, `codmunres`, `idade`, `sexo`, `dtobito`) |
  | 2024 | 88 | + COVID_CLAS, MAT_CLAS |
  | 2025–2026 | 86 | — |

- Codificação: nas amostras quase tudo é ASCII. A amostra de 1997 tem bytes não-ASCII, mas não determinei se o arquivo é UTF-8 ou Latin-1/CP1252. Os campos que o ETL precisa são numéricos ou alfanuméricos ASCII. Ver https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/Mortalidade_Geral_1997_csv.zip

### Cited Findings — município/UF de residência
- O dicionário 2025 define `CODMUNRES` como Caracter 7: "Código do município de residência"; em óbito fetal vale o município da mãe — [Dicionário SIM 2025](https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/Dicionario_SIM_2025.pdf)
- Observado:
  - **1979–1995:** campo `MUNIRES` com **7 dígitos** (ex.: `1200104`);
  - **1996–2005:** `CODMUNRES` com **7 dígitos** (ex.: `1200401`);
  - **2006 em diante:** `CODMUNRES` com **6 dígitos**, sem dígito verificador (ex.: `120033`, `411990`).
  - Em todos os casos, os 2 primeiros dígitos são o código IBGE da UF (11–53).
  - Há códigos `UF0000`/`UF00000`, ou seja, município ignorado com UF conhecida (ex.: `1200000`).

  Fonte: ZIPs de 1979, 1996, 2006 e 2024 em https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/
- O microdatasus normaliza para 6 dígitos com `substr(CODMUNRES, 0, 6)` — [microdatasus process_sim.R](https://github.com/rfsaldanha/microdatasus/blob/master/R/process_sim.R)

### Cited Findings — causa básica
- O dicionário define `CAUSABAS` como Caracter 4 ("*;letras; números"), "Causa básica da DO". `CAUSABAS_O` é a "Causa básica informada antes da resseleção". Os códigos de `TP_ALTERA` incluem "02 = CausaBas em branco", "03 = CausaBas com ausência do 4 caractere", "04 = Causas Asterisco", "06/07 = inválida para o sexo" etc. — [Dicionário SIM 2025](https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/Dicionario_SIM_2025.pdf)
- Observado:
  - **1979–1995:** CID-9 sempre com 4 caracteres; as categorias de 3 dígitos vêm completadas com `X` (ex.: `436X`, `486X`, `410X`), as demais têm 4 dígitos numéricos (ex.: `7989`, `8199`, `0091`).
  - **1996+:** CID-10 sem ponto, com 3 ou 4 caracteres (ex.: `I64`, `R98`, `I219`, `B342`). Nas amostras a causa básica de 3 caracteres aparece sem `X`, mas as LINHAA–LINHAII trazem `*` e `X` (ex.: `*R54X`, `*I10X`).
  - `CAUSABAS_O` existe a partir de 2006, e em parte dos registros vem diferente de `CAUSABAS` (ex.: 2025: `J690` vs `A979`).

  Fonte: ZIPs em https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/

### Cited Findings — sexo
- O dicionário 2025 traz: "M,1 – masculino; F,2 – feminino; I,0,9 - ignorado" — [Dicionário SIM 2025](https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/Dicionario_SIM_2025.pdf)
- Observado em todos os anos: `1`, `2`, `0`, e às vezes `9` (1995, 2010) ou vazio (1992, 1993, 1997, 1998, 1999). `M`/`F`/`I` **não apareceram** nas amostras. Fonte: ZIPs em https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/
- O microdatasus decodifica: "0" e "9" → NA; "1" → Masculino; "2" → Feminino — [microdatasus process_sim.R](https://github.com/rfsaldanha/microdatasus/blob/master/R/process_sim.R)

### Cited Findings — idade (IDADE, 3 dígitos)
- O microdatasus trata "000" e "999" como NA. O 1º dígito é a unidade e os 2 seguintes são a quantidade: 0 = minutos, 1 = horas, 2 = dias, 3 = meses, 4 = anos, 5 = 100 + valor (anos) — [microdatasus process_sim.R](https://github.com/rfsaldanha/microdatasus/blob/master/R/process_sim.R)
- O dicionário 2025 descreve IDADE como dois subcampos. Os subcampos de quantidade são: minutos 01–59, horas 01–23, dias 01–29, meses 01–11, anos 00–99; "9 - ignorado". **Porém o texto lista as unidades como "1 = minuto, 2 = hora, 3 = mês, 4 = ano, 5 = maior que 100 anos"**, o que omite "dias" e contradiz os dados e o microdatasus — [Dicionário SIM 2025](https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/Dicionario_SIM_2025.pdf); contradito por [microdatasus process_sim.R](https://github.com/rfsaldanha/microdatasus/blob/master/R/process_sim.R) e pelos dados (em 2005 há valores `0xx` como `010`, `030`, `059`, compatíveis com minutos).
- Observado:
  - **CID-9 (1979–1995):** 1º dígito de 0 a 5; o único valor com 1º dígito 0 é `000` (742 de 48.732 linhas da amostra de 1979; 909 em 1990), portanto `000` = ignorado. Valores `5xx` (ex.: `500`–`572`) = 100+.
  - **1996+:** IDADE vazia ocorre (461 de 35.205 na amostra de 1996); `999` = ignorado.
  - **2022 e 2023 (versão de 29/10/2026):** os zeros à esquerda foram removidos (IDADE com 1–2 caracteres, ex.: `15`, `7`) e DTOBITO aparece com 7 caracteres (ex.: `8042022`, `1012023`).

  Fonte: ZIPs de 1979, 1990, 1996, 2005, 2022 e 2023 em https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/

### Cited Findings — data do óbito
- O dicionário define DTOBITO como Caracter 8, "Data no padrão ddmmaaaa" — [Dicionário SIM 2025](https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/Dicionario_SIM_2025.pdf)
- Observado:
  - **1979–1990:** `DATAOBITO` com 6 caracteres `aammdd`, quase sempre com dia `00` (ex.: `790600`); só há ano e mês.
  - **1991:** maioria com 4 caracteres `aamm` (ex.: `9108`).
  - **1992–1995:** `aammdd` com o dia preenchido, mas com casos `940000` e `95` (só o ano).
  - **1996–2021:** `DTOBITO` com 8 caracteres `ddmmaaaa`; em 1996–1999 há casos com 4 caracteres (só o ano, `1996`).
  - **2022–2023:** 7 caracteres quando o dia é menor que 10 (zero à esquerda perdido).

  Fonte: ZIPs em https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/

### Inferences (regras propostas para o ETL)
- **Ano:** usar o ano do nome do arquivo, não a data. As datas incompletas ou mal formatadas nos dois layouts tornam a data pouco confiável para isso.
- **Nomes de coluna:** tratar sem diferenciar maiúsculas de minúsculas.
- **UF:** `MUNIRES ?? CODMUNRES`, pegando os 2 primeiros dígitos (após trim). Validar contra a lista de UFs IBGE (11–17, 21–29, 31–33, 35, 41–43, 50–53). Se o campo estiver vazio ou com código inválido, classificar como "UF ignorada".
- **Causa básica:** `CAUSABAS`, aplicando trim e removendo `*`, `.` e o `X` final. Separar CID-9 (arquivos ≤1995) de CID-10 (≥1996): os códigos não são comparáveis sem tabela de correspondência.
- **Sexo:** `1` → M; `2` → F; `M`/`F` por segurança; demais valores (`0`, `9`, `I`, vazio) → ignorado.
- **IDADE:** fazer `PadLeft(3,'0')` (corrige 2022–2023). Depois:
  - `000`, `999` ou vazio → ignorado;
  - 1º dígito 0–3 → menos de 1 ano (faixa 0–4);
  - 4 → `valor` anos;
  - 5 → 100 + `valor` anos (entra em 80+);
  - outro 1º dígito → ignorado.

  Faixas: 0–4, 5–9, …, 75–79, 80+, além de "ignorada".
- **TIPOBITO:** filtrar = `2`, se o objetivo for óbitos não fetais.

### Gaps
- Não consegui ler o "Estrutura_SIM_Anterior.pdf" (FTP, de 2011) nem o `CID9/DOCS/INTRO.pdf` para confirmar oficialmente o código IDADE e os valores de SEXO no período CID-9. As regras acima para 1979–1995 vêm do que foi observado nos dados.
- Não sei se a remoção de zeros em 2022–2023 também atinge CODMUNRES ou CAUSABAS. Nas amostras, CODMUNRES manteve 6 dígitos.
- Nos campos usados, M/F/I e idades com 1º dígito 6–8 não foram observados no arquivo inteiro, só na amostra.

## 4. Documentação oficial (dicionários / "Estrutura do SIM")

### Takeaway
O dicionário atual é o "Estrutura do SIM", atualizado em 06/2025, publicado no OpenDATASUS e no FTP. Para o período CID-9, a documentação fica no FTP, em `CID9/DOCS`.

### Cited Findings
- Dicionário no OpenDATASUS: https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/Dicionario_SIM_2025.pdf ("Estrutura do SIM", CGIAE/DASNT/SVS, "Arquivo atualizado em 06/2025") — [Portal Dados Abertos SUS – SIM](https://dadosabertos.saude.gov.br/dataset/sim)
- Arquivos em `ftp://ftp.datasus.gov.br/dissemin/publicos/SIM/CID10/DOCS/`:
  - `Estrutura_do_SIM_2025.pdf` (07/07/2025, 634 KB);
  - `Estrutura_SIM_Anterior.pdf` (2011, 56 KB);
  - `Estrutura_SIM_para_CD.pdf` (2020);
  - `Docs_Tabs_CID10.zip`;
  - `Legislacao_PDF.pdf`;
  - `Portaria.pdf`.

  — [FTP DATASUS SIM CID10/DOCS](ftp://ftp.datasus.gov.br/dissemin/publicos/SIM/CID10/DOCS/)
- Arquivos em `ftp://ftp.datasus.gov.br/dissemin/publicos/SIM/CID9/DOCS/`: `Docs-Tabs-CID9.zip`, `INTRO.pdf`, `MORT98.HLP`, `MTAB16M.pdf` — [FTP DATASUS SIM CID9/DOCS](ftp://ftp.datasus.gov.br/dissemin/publicos/SIM/CID9/DOCS/)
- O código do microdatasus documenta a decodificação de IDADE, SEXO e CODMUNRES — [microdatasus process_sim.R](https://github.com/rfsaldanha/microdatasus/blob/master/R/process_sim.R)

### Inferences
- O dicionário de 2025 tem um erro na lista de unidades de IDADE (ver seção 3). Para a regra de idade, usar os dados e o microdatasus como referência.

### Gaps
- Não revisei o código do PySUS.
- Não abri os PDFs de CID-9.

## 5. Ressalvas de qualidade por período

### Takeaway
As principais armadilhas para o ETL:
- datas do período CID-9 sem dia, e às vezes sem mês;
- código de município de 7 dígitos até 2005 e de 6 dígitos a partir de 2006;
- `000` como idade ignorada no período CID-9;
- cabeçalhos com nomes e ordem variáveis, inclusive em minúsculas em 2023;
- zeros à esquerda removidos em 2022–2023;
- anos 2025–2026 provisórios, e 2024 com origem a confirmar.

### Cited Findings
- Todos os itens de layout das seções 2 e 3 (observado). Fonte: https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/
- Códigos de município ignorado no formato `UF0000[0]`. Exemplos: 827 casos na amostra de 1979 e 320 na de 1996 (observado). Fonte: https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/Mortalidade_Geral_1979_csv.zip
- Os códigos de `TP_ALTERA` indicam correções automáticas da causa básica: causa asterisco, causa inválida para o sexo, causas implausíveis/triviais. Isso explica divergências entre `CAUSABAS` e `CAUSABAS_O` — [Dicionário SIM 2025](https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/Dicionario_SIM_2025.pdf)

### Inferences
- Usar `CAUSABAS`, a causa final após a resseleção, por ser a mesma do Tabnet.
- 1979–1995 e 1996+ usam classificações diferentes (CID-9 e CID-10). A série de causas exige agrupamento por capítulo ou tabela de compatibilização.

### Gaps
- Não encontrei documento oficial que liste as ressalvas de qualidade por ano.

## 6. Tamanho e volume por ano (para dimensionar o ETL)

### Takeaway
Os ZIPs vão de cerca de 10 MB (1979) a 107 MB (2021). Descompactados, os CSVs vão de 113 MB a 633 MB, somando cerca de **12 GB** ao todo. Estimativa de 0,7 milhão (1979) a 1,87 milhão (2021) de registros por ano, cerca de 50 milhões no total. Usar leitura em streaming (direto do ZIP, linha a linha).

### Cited Findings
Tamanho compactado (Content-Length, via HEAD) e descompactado (diretório central do ZIP). Fonte: https://s3.sa-east-1.amazonaws.com/ckan.saude.gov.br/SIM/csv/Mortalidade_Geral_{AAAA}_csv.zip

| Ano | ZIP (MB) | CSV (MB) | Registros estimados* |
|---|---|---|---|
| 1979 | 9,7 | 113,3 | ~699 mil |
| 1985 | 10,4 | 122,9 | ~767 mil |
| 1990 | 10,8 | 126,0 | ~800 mil |
| 1995 | 20,1 | 146,5 | ~904 mil |
| 1996 | 16,4 | 117,9 | ~937 mil |
| 2000 | 23,9 | 143,5 | ~1,01 mi |
| 2005 | 27,7 | 169,3 | ~1,07 mi |
| 2006 | 40,2 | 230,8 | ~1,07 mi |
| 2010 | 45,3 | 261,8 | ~1,18 mi |
| 2014 | 72,1 | 427,2 | ~1,25 mi |
| 2019 | 81,3 | 459,9 | ~1,36 mi |
| 2020 | 91,6 | 531,9 | ~1,58 mi |
| 2021 | 106,7 | 632,7 | ~1,87 mi |
| 2022 | 95,1 | 525,5 | ~1,55 mi |
| 2023 | 91,4 | 502,3 | ~1,48 mi |
| 2024 | 95,4 | 526,4 | ~1,54 mi |
| 2025 (preliminar) | 58,6 | 314,0 | ~0,91 mi |
| 2026 (2ª prévia) | 56,3 | 315,6 | ~0,92 mi |

\* Estimativa = tamanho descompactado ÷ comprimento médio das linhas na amostra inicial. **Não é contagem oficial**: erro esperado de ±5–10%, porque a amostra é enviesada para a região Norte.

### Inferences
- O ano de 2025 ainda incompleto (~0,9 mi contra ~1,5 mi esperado) sugere que o CSV preliminar de 2025 (atualizado em 15/04/2026) está bem incompleto. Esse ponto exige confirmação.
- Com 12 GB de CSV, uma passada sequencial em .NET (`ZipArchive` + `StreamReader` + split simples por `;`) é viável em minutos. Não é preciso carregar nada em memória.

### Gaps
- Não obtive contagens oficiais de registros por ano a partir do CSV. Validar contra os totais do Tabnet, pelo [Tabnet SIM CID-10](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?sim/cnv/obt10uf.def) e pelo Tabnet CID-9.
