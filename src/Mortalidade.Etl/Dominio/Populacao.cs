namespace Mortalidade.Etl.Dominio;

public sealed record Populacao(Ano Ano, Uf Uf, Sexo Sexo, FaixaEtaria FaixaEtaria, long Habitantes);
