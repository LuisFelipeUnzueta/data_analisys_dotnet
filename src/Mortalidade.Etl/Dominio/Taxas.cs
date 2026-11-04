namespace Mortalidade.Etl.Dominio;

/// <summary>Pesos da população-padrão por faixa quinquenal (até 80+); lidos de <c>data/padrao/*.csv</c> por <c>LeitorPopulacaoPadrao</c>.</summary>
public sealed class PopulacaoPadrao
{
    private PopulacaoPadrao(IReadOnlyDictionary<FaixaEtaria, double> pesos) => Pesos = pesos;

    public IReadOnlyDictionary<FaixaEtaria, double> Pesos { get; }

    /// <summary>Exige um peso não negativo para cada faixa conhecida (sem a Ignorada).</summary>
    public static Result<PopulacaoPadrao> Criar(IReadOnlyDictionary<FaixaEtaria, double> pesos)
    {
        var faixasConhecidas = FaixaEtaria.Todas.Count - 1;
        if (pesos.ContainsKey(FaixaEtaria.Ignorada) || pesos.Values.Any(p => p < 0))
            return Result<PopulacaoPadrao>.Falha("padrao.peso_invalido", "Pesos devem ser não negativos e não incluir a faixa Ignorada.");

        return pesos.Count == faixasConhecidas
            ? Result<PopulacaoPadrao>.Sucesso(new PopulacaoPadrao(pesos))
            : Result<PopulacaoPadrao>.Falha("padrao.faixas_incompletas", $"Esperadas {faixasConhecidas} faixas, lidas {pesos.Count}.");
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

    /// <summary>Taxas bruta e padronizada de um recorte (células e população já filtradas), em uma passada por coleção.</summary>
    public static (double? Bruta, double? Padronizada) DoRecorte(
        IEnumerable<CelulaCubo> celulas, IEnumerable<Populacao> populacao, PopulacaoPadrao padrao)
    {
        var obitosPorFaixa = new double[FaixaEtaria.Todas.Count];
        var populacaoPorFaixa = new long[FaixaEtaria.Todas.Count];
        long obitos = 0;
        foreach (var celula in celulas)
        {
            obitos += celula.Obitos;
            obitosPorFaixa[celula.FaixaEtaria.Indice] += celula.ObitosAjustados;
        }

        foreach (var linha in populacao)
            populacaoPorFaixa[linha.FaixaEtaria.Indice] += linha.Habitantes;

        var porFaixa = FaixaEtaria.Todas.ToDictionary(f => f, f => (obitosPorFaixa[f.Indice], populacaoPorFaixa[f.Indice]));
        return (Bruta(obitos, populacaoPorFaixa.Sum()), Padronizada(porFaixa, padrao));
    }
}
