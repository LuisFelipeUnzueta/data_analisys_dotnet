namespace Mortalidade.Etl.Dominio;

/// <summary>Pesos da população-padrão por faixa quinquenal (até 80+), carregados de <c>data/padrao/*.csv</c>.</summary>
public sealed class PopulacaoPadrao
{
    private PopulacaoPadrao(IReadOnlyDictionary<FaixaEtaria, double> pesos) => Pesos = pesos;

    public IReadOnlyDictionary<FaixaEtaria, double> Pesos { get; }

    public static Result<PopulacaoPadrao> Carregar(IEnumerable<string> linhasCsv)
    {
        var faixas = FaixaEtaria.Todas.Where(f => f != FaixaEtaria.Ignorada).ToDictionary(f => f.Rotulo);
        var pesos = new Dictionary<FaixaEtaria, double>();
        foreach (var campos in linhasCsv.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Split(';')))
        {
            if (campos.Length != 2 || !faixas.TryGetValue(campos[0], out var faixa) || !double.TryParse(campos[1], System.Globalization.CultureInfo.InvariantCulture, out var peso) || peso < 0)
                return Result<PopulacaoPadrao>.Falha("padrao.linha_invalida", $"Linha inválida: {string.Join(';', campos)}.");
            pesos[faixa] = peso;
        }

        return pesos.Count == faixas.Count
            ? Result<PopulacaoPadrao>.Sucesso(new PopulacaoPadrao(pesos))
            : Result<PopulacaoPadrao>.Falha("padrao.faixas_incompletas", $"Esperadas {faixas.Count} faixas, lidas {pesos.Count}.");
    }
}

/// <summary>Taxas por 100 mil habitantes.</summary>
public static class Taxas
{
    public const double Base = 100_000;

    public static double? Bruta(double obitos, long populacao) =>
        populacao > 0 ? obitos / populacao * Base : null;

    /// <summary>
    /// Método direto: Σ peso_f · (óbitos_f / população_f) / Σ peso_f. Óbitos devem vir com a idade ignorada redistribuída.
    /// Sem população em alguma faixa de peso positivo, a taxa não é definida.
    /// </summary>
    public static double? Padronizada(IReadOnlyDictionary<FaixaEtaria, (double Obitos, long Populacao)> porFaixa, PopulacaoPadrao padrao)
    {
        double soma = 0, pesos = 0;
        foreach (var (faixa, peso) in padrao.Pesos)
        {
            var (obitos, populacao) = porFaixa.GetValueOrDefault(faixa);
            if (populacao <= 0)
            {
                if (peso > 0)
                    return null;
                continue;
            }

            soma += peso * obitos / populacao;
            pesos += peso;
        }

        return pesos > 0 ? soma / pesos * Base : null;
    }
}
