# Mortalidade no Brasil

Página interativa com a mortalidade no Brasil desde 1979, por região/UF, grupo de causa harmonizado (CID-9 e CID-10), sexo e faixa etária. Um ETL anual em .NET lê os microdados CSV do SIM (OpenDATASUS) e a população do IBGE, gera um cubo agregado em Parquet e o publica em hospedagem estática.

## Estrutura

```
src/Mortalidade.Etl/
  Dominio/      regras puras: dimensões (Ano, Uf, Regiao, Sexo, FaixaEtaria), harmonização e taxas
  Leitura/      leitores do CSV do SIM e da população do IBGE (fase 2)
  Cubo/         agregação, escrita em Parquet e JSON de dimensões (fase 2)
  Validacao/    conferência com o Tabnet e continuidade CID-9/CID-10 (fase 2)
  Program.cs    pipeline: ler → harmonizar → agregar → validar → gravar
data/cause_map/ tabelas de correspondência versionadas (fase 1)
tests/Mortalidade.Etl.Tests/
  Dominio/              unitários do domínio
  ArquiteturaTests.cs   Dominio não depende de Leitura, Cubo, Validacao nem Parquet
web/            front estático (fase 3)
docs/adr/       decisões arquitetônicas
```

Um único projeto, com o domínio isolado por namespace e protegido por um teste de arquitetura. O ETL só grava num diretório local, e quem publica (GitHub Pages ou S3) é o workflow. Por isso não há portas nem adaptadores de publicação: interfaces entram só quando houver uma segunda implementação real.

## Executar

Requer o SDK do .NET 10 (fixado em `global.json`).

```bash
dotnet build
dotnet test
dotnet run -c Release --project src/Mortalidade.Etl -- data/raw 1979 2023 out
```

O ETL lê os CSV anuais do SIM em `data/raw/` (`Mortalidade_Geral_{AAAA}.csv`, baixados do [OpenDATASUS](https://dadosabertos.saude.gov.br/dataset/sim)), confere cada ano com o Tabnet e grava em `out/`:

| Arquivo | Conteúdo |
|---|---|
| `obitos.parquet` | Óbitos por ano × UF × grupo de causa × sexo × faixa etária, com os óbitos de idade ignorada redistribuídos (`obitos_ajustados`) para a taxa padronizada |
| `populacao.parquet` | População por ano × UF × sexo × faixa etária |
| `dimensoes.json` | Códigos das dimensões, pesos da OMS 2000–2025, proveniência de cada ano e marcos metodológicos |

### Página (`web/`)

Svelte 5 + Vite, gráficos com Observable Plot e leitura do Parquet no navegador com hyparquet. A página baixa os três arquivos uma vez (≈ 4,4 MB) e recalcula os gráficos a cada filtro com uma passada pelo cubo.

```bash
cp out/* web/public/dados/
npm --prefix web install
npm --prefix web run dev     # http://localhost:5173
npm --prefix web test        # inclui conferência das taxas com o ETL quando há dados em web/public/dados
npm --prefix web run build   # gera web/dist
```

## Roadmap

| Fase | Entrega | Critério de saída |
|---|---|---|
| 0 | Preparação das fontes | Fontes, anos e população-padrão fechados |
| 1 | Harmonização CID-9/CID-10 | Nenhum salto sem explicação em 1995/1996 |
| 2 | Cubo de dados | Totais por ano conferem com o Tabnet |
| 3 | Interface | Página publicada em hospedagem estática |
| 4 | Operação | Primeira atualização anual sem intervenção manual |

## ADRs candidatas

1. Site estático sem servidor de aplicação.
2. UF de residência, não de ocorrência.
3. Grupos de causa harmonizados.
4. ETL em .NET num projeto único com domínio isolado, lendo apenas o CSV do OpenDATASUS.
5. Por que não Kafka, DynamoDB nem microsserviços.
6. População-padrão da taxa padronizada (pendente).
