namespace Mortalidade.Etl.Dominio;

public sealed record CelulaCubo(Ano Ano, Uf Uf, GrupoCausa GrupoCausa, Sexo Sexo, FaixaEtaria FaixaEtaria, long Obitos);
