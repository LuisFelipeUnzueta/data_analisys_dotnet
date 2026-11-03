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
dotnet run --project src/Mortalidade.Etl
```

- `global.json` fixa o SDK 10.0.4xx; o projeto usa `net10.0` e C# 14 (diverge do padrão .NET 8 das preferências globais, por decisão do projeto).
- `Directory.Build.props` liga `TreatWarningsAsErrors`: warning quebra o build.
- CI (`.github/workflows/ci.yml`): restore, build Release, testes com cobertura.

## Arquitetura

Decisão deliberada de **engenharia pragmática**: um único projeto `src/Mortalidade.Etl` e um único `tests/Mortalidade.Etl.Tests`, em vez de camadas Clean Architecture em projetos separados.

- `Dominio/` é puro (sem I/O, sem pacotes). A regra está em `tests/Mortalidade.Etl.Tests/ArquiteturaTests.cs`: `Dominio` não depende de `Leitura`, `Cubo`, `Validacao` nem `Parquet`. Ao criar esses namespaces, mantenha os nomes para a regra valer.
- Pipeline planejado em `Program.cs`, como funções encadeadas: ler → harmonizar → agregar → validar → gravar.
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

- Série da v1: **1979 a 2024** (anos definitivos). 2025 (preliminar) e 2026 (prévia) ficam fora; o CSV de 2025 tem ~60% dos óbitos do Tabnet.
- População 1980–1999: interpolação própria (geométrica) por UF × sexo × faixa entre Censo 1980, Censo 1991, Contagem 1996 e o ano 2000 da série RIPSA `popsvs2024br`, para não haver salto na emenda com 2000–2024. Não usar a `popbr`.
- População de 1979: extrapolada de 1980–1981 por UF × sexo × faixa (crescimento geométrico aplicado para trás), marcada como estimada no cubo e na interface.
- Tocantins (17) só a partir de 1989; antes, óbitos e população do território ficam em Goiás, como nos dados originais. A queda de GO em 1989 é sinalizada na interface.
- UF 20 (antigo Território de Fernando de Noronha, ex.: município `2000107`) é contada como PE (26), como faz o Tabnet. Conferido em 1979: com essa regra, todas as UFs batem com o Tabnet.
- IDADE `000` em CID-9 (1979–1995) é idade ignorada; em CID-10 `0xx` são minutos. A decodificação depende da revisão da CID.
- Taxa padronizada: método direto com um único padrão, OMS 2000–2025 (versão SEER por milhão; 80+ = soma de 80–84 a 100+ = 15.445). Pesos versionados como dado, não no código. O padrão OMS 2026–2050 fica como migração futura.
- Idade ignorada na taxa padronizada: redistribuída proporcionalmente às faixas conhecidas dentro de ano × UF × causa × sexo; se o estrato não tiver nenhuma idade conhecida, usa a distribuição de ano × UF × sexo (todas as causas). O cubo mantém a categoria `Ignorada` e os totais brutos não mudam.
- Residência sem UF válida (após a regra da UF 20): usa a UF de ocorrência (`MUNIOCOR`/`CODMUNOCOR`). Se ela também for inválida, o óbito entra numa UF `Ignorada` visível. Cada imputação é contada no relatório de qualidade do run.
- Conferência com o Tabnet: igualdade exata por ano × UF e ano × sexo; qualquer divergência aborta o run e nada é publicado. O gabarito do Tabnet é gravado como fixture com a data da consulta.
- Detalhes de fontes, layout dos CSV e conferência com o Tabnet: `reports/Fontes e padrões da fase 0.md`.

## Pontos em aberto

Lista final de grupos de causa harmonizados (fase 1). Não invente valores; pergunte.
