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

- `global.json` fixa o SDK 8.0.4xx (a máquina também tem o SDK 10); mantenha `net8.0` e C# 12.
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

## Pontos em aberto

Último ano consolidado no CSV do OpenDATASUS, fonte/versão da população IBGE, população-padrão da taxa padronizada, lista final de grupos de causa. Não invente valores para eles; pergunte.
