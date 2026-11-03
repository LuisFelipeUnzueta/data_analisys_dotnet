# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Projeto

Portfólio pessoal: página estática com a mortalidade no Brasil desde 1979 (SIM/OpenDATASUS + população IBGE), por UF de residência, grupo de causa harmonizado CID-9/CID-10, sexo e faixa etária. Um ETL anual em .NET gera um cubo agregado em Parquet; o front em `web/` (fase 3) lê o Parquet direto no navegador.

**Não siga os padrões da 55TECH aqui.** O `AGENTS.md`, as skills `agent-spec-*` e o `.codex/` em `C:\55TECH` não se aplicam a este repositório.

## Comandos

```bash
dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~FaixaEtariaTests"   # um teste ou classe
dotnet run -c Release --project src/Mortalidade.Etl -- data/raw 1979 2023   # lê os CSV e confere com o Tabnet
```

- Rode a partir da raiz do repositório: o ETL lê `data/cause_map/grupos.csv` e `data/tabnet/gabarito_1979_2023.csv` por caminho relativo. Os CSV do SIM ficam em `data/raw/` (ignorado pelo git, ~12 GB); os 45 anos rodam em ~25 s.
- `data/tabnet/gabarito_1979_2023.csv` é o gabarito (ano × total/UF/sexo, por residência) extraído do Tabnet por POST, com a data da consulta. A UF `00` é "Ignorado/exterior" (zero em todos os anos).

- `global.json` fixa o SDK 10.0.4xx; o projeto usa `net10.0` e C# 14 (diverge do padrão .NET 8 das preferências globais, por decisão do projeto).
- `Directory.Build.props` liga `TreatWarningsAsErrors`: warning quebra o build.
- CI (`.github/workflows/ci.yml`): restore, build Release, testes com cobertura.

## Arquitetura

Decisão deliberada de **engenharia pragmática**: um único projeto `src/Mortalidade.Etl` e um único `tests/Mortalidade.Etl.Tests`, em vez de camadas Clean Architecture em projetos separados.

- `Dominio/` é puro (sem I/O, sem pacotes). A regra está em `tests/Mortalidade.Etl.Tests/ArquiteturaTests.cs`: `Dominio` não depende de `Leitura`, `Cubo`, `Validacao` nem `Parquet`. Ao criar esses namespaces, mantenha os nomes para a regra valer.
- Pipeline em `Program.cs`, um ano por vez em paralelo: `Leitura/LeitorSimCsv` → `Cubo/AgregadorCubo` (resolve o grupo com cache por código) → `Validacao/ConferenciaTabnet`. Gravação em Parquet ainda não existe.
- CSV lido com o pacote `Sep` (rápido, sem alocação por campo); colunas por nome, sem diferenciar caixa.
- **Sem portas/interfaces** até existir uma segunda implementação real. Leitores recebem `Stream` e os testes usam arquivos de fixture, não fakes.
- **O ETL só grava num diretório local.** Download das fontes e publicação (GitHub Pages ou S3) ficam nos workflows, não no código, por isso não há retry/decorators.
- A correspondência CID → grupo é **dado** (`data/cause_map/*.csv`, versionado), carregado num dicionário `(RevisaoCid, código) → GrupoCausa`; não crie uma Strategy por revisão.
- Fora de escopo de propósito: CQRS, MediatR, event sourcing, Kafka, DynamoDB, microsserviços.

## Regras de domínio que não são óbvias no código

- Série começa em 1979 (`Ano.Primeiro`); a revisão da CID troca em 1996 (`RevisaoCidPorAno`). A continuidade 1995/1996 é o caso central dos testes golden-file da harmonização.
- Localização é a **UF de residência**, não a de ocorrência. A região vem do primeiro dígito do código IBGE da UF.
- Sexo e idade ignorados viram categoria visível (`Sexo.Ignorado`, `FaixaEtaria.Ignorada`); registros nunca são descartados. Causas mal definidas serão um grupo próprio.
- Faixas quinquenais até `80+`. O domínio recebe idade em anos; decodificar o campo `IDADE` do SIM (código de 3 dígitos) é responsabilidade do leitor.
- Erros de validação usam `Result<T>` com código de erro (`"uf.codigo_invalido"`), não exceções.
- Falhas devem abortar o run em vez de degradar em silêncio: coluna obrigatória ausente no CSV, código CID sem grupo, totais anuais divergentes do Tabnet.
- As dimensões (faixas, grupos de causa) são definidas uma vez no ETL, que gera o JSON consumido pelo front.

## Decisões da fase 0

- Série da v1: **1979 a 2023**. 2024 entra automaticamente quando o CSV republicado bater com o Tabnet (o atual tem extração anterior à definitiva). 2025 (preliminar) e 2026 (prévia) ficam fora; o CSV de 2025 tem ~60% dos óbitos do Tabnet. O ETL processa ano a ano: um ano que não bate não é publicado, mas não derruba os anos já conferidos.
- População 1980–1999: interpolação própria (geométrica) por UF × sexo × faixa entre Censo 1980, Censo 1991, Contagem 1996 e o ano 2000 da série RIPSA `popsvs2024br`, para não haver salto na emenda com 2000–2024. Não usar a `popbr`.
- População de 1979: extrapolada de 1980–1981 por UF × sexo × faixa (crescimento geométrico aplicado para trás), marcada como estimada no cubo e na interface.
- Tocantins (17) só a partir de 1989; antes, óbitos e população do território ficam em Goiás, como nos dados originais. A queda de GO em 1989 é sinalizada na interface.
- UF 20 (antigo Território de Fernando de Noronha, ex.: município `2000107`) é contada como PE (26), como faz o Tabnet. Com as regras desta seção, total, ano × UF, ano × sexo e idade ignorada batem exatamente com o Tabnet em 1979–2023 (conferência de 03/11/2026), sem nenhuma imputação por UF de ocorrência e com `TIPOBITO = 2` em 100% dos registros. O CSV de 2024 (extração anterior à definitiva) tem 4.646 óbitos a menos que o Tabnet (1.527.369 × 1.532.015), espalhados por todas as UFs.
- IDADE ignorada = vazio, `000` ou `999`, em qualquer ano; `0xx` com xx ≥ 1 são minutos. 1979–1995 usam só `000`; 1999 em diante passa a usar `999`. Conferido com o Tabnet em 1979–2023.
- Taxa padronizada: método direto com um único padrão, OMS 2000–2025 (versão SEER por milhão; 80+ = soma de 80–84 a 100+ = 15.445). Pesos versionados como dado, não no código. O padrão OMS 2026–2050 fica como migração futura.
- Idade ignorada na taxa padronizada: redistribuída proporcionalmente às faixas conhecidas dentro de ano × UF × causa × sexo; se o estrato não tiver nenhuma idade conhecida, usa a distribuição de ano × UF × sexo (todas as causas). O cubo mantém a categoria `Ignorada` e os totais brutos não mudam.
- Residência sem UF válida (após a regra da UF 20): usa a UF de ocorrência (`MUNIOCOR`/`CODMUNOCOR`). Se ela também for inválida, o leitor devolve a falha `sim.uf_invalida`, que reprova o ano (não há categoria de UF ignorada no cubo; 1979–2023 têm zero casos).
- Conferência com o Tabnet: igualdade exata por ano × UF e ano × sexo; qualquer divergência aborta o run e nada é publicado. O gabarito do Tabnet é gravado como fixture com a data da consulta.
- Detalhes de fontes, layout dos CSV e conferência com o Tabnet: `reports/Fontes e padrões da fase 0.md`.

## Harmonização CID-9/CID-10 (fase 1)

- `data/cause_map/grupos.csv` é a tabela de correspondência: 87 grupos com faixas CID-9 e CID-10 por prefixo (`410-414`, `I20-I25`, `4275`). Grupos `resto=1` são o fallback do capítulo; entre específicos vence a faixa mais longa, e empate é erro. Cobre 100% dos códigos de 1979–2023.
- CID-9 de causas externas vem sem o "E" (`800-999`); códigos de 3 dígitos vêm completados com `X` (`436X`).
- `mal_definidas` segue o conceito do Tabnet: R00–R99 mais I46.1, I46.9, I95.9, I99, J96.0, J96.9 e P28.5 (CID-9: 780–799 mais 427.5, 458.9 e 770.8). Diferença: R95/798.0 (morte súbita na infância) ficam dentro do grupo.
- Continuidade: a razão 1996/1995 de cada grupo é comparada com a média das razões vizinhas (1992–1999). Desvio acima de 10% marca `comparabilidade=reduzida` (23 grupos), que vira aviso junto ao gráfico. Ajustes de fronteira feitos para reduzir saltos: 239.6 em `neo_snc`, 887 fora de `quedas`, 590 em `renais`, prematuridade e trauma de parto fundidos em `perinatais_resto`.

- Implementação: `Dominio/MapaCausas.cs` (resolve causa → grupo) e `Validacao/ContinuidadeCid.cs` (desvio 1995/1996). `tests/.../Validacao/HarmonizacaoGoldenTests.cs` usa a tabela real e fixtures em `tests/Mortalidade.Etl.Tests/Fixtures/` extraídas dos CSV: códigos observados 1979–2023, óbitos por código 1992–1999 e o golden de totais por grupo. Mudar uma faixa do `grupos.csv` quebra o golden de propósito: regere o golden e a coluna `comparabilidade` e revise a diferença.

## Pontos em aberto

Revisão da lista de grupos por alguém que conheça codificação de causas de morte. Não invente valores; pergunte.
