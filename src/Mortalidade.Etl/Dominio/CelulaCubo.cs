namespace Mortalidade.Etl.Dominio;

/// <param name="ObitosAjustados">
/// Óbitos com a idade ignorada redistribuída entre as faixas conhecidas (zero na faixa <c>Ignorada</c>);
/// usado só na taxa padronizada. A soma por estrato é igual à de <paramref name="Obitos"/>.
/// </param>
public sealed record CelulaCubo(Ano Ano, Uf Uf, GrupoCausa GrupoCausa, Sexo Sexo, FaixaEtaria FaixaEtaria, long Obitos, double ObitosAjustados = 0);
