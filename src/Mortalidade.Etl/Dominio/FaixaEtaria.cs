namespace Mortalidade.Etl.Dominio;

public sealed record FaixaEtaria
{
    private const int Largura = 5;
    private const int InicioAberta = 80;

    /// <summary>16 faixas quinquenais, 80+ e Ignorada, nesta ordem; <see cref="Indice"/> é a posição aqui.</summary>
    public static IReadOnlyList<FaixaEtaria> Todas { get; } =
        Enumerable.Range(0, InicioAberta / Largura)
            .Select(i => new FaixaEtaria(i * Largura, i * Largura + Largura - 1, i))
            .Append(new FaixaEtaria(InicioAberta, null, InicioAberta / Largura))
            .Append(new FaixaEtaria(null, null, InicioAberta / Largura + 1))
            .ToArray();

    public static FaixaEtaria Ignorada => Todas[^1];

    // Resultados pré-alocados: DeIdadeEmAnos roda uma vez por óbito.
    private static readonly Result<FaixaEtaria>[] Sucessos = Todas.Select(Result<FaixaEtaria>.Sucesso).ToArray();

    private static readonly Dictionary<string, FaixaEtaria> PorRotulo = Todas.ToDictionary(f => f.Rotulo);

    private FaixaEtaria(int? inicio, int? fim, int indice)
    {
        Inicio = inicio;
        Fim = fim;
        Indice = indice;
        Rotulo = (inicio, fim) switch
        {
            (null, _) => "Ignorada",
            (var i, null) => $"{i}+",
            var (i, f) => $"{i}-{f}"
        };
    }

    public int? Inicio { get; }

    public int? Fim { get; }

    public int Indice { get; }

    public string Rotulo { get; }

    public static Result<FaixaEtaria> DeIdadeEmAnos(int? anos)
    {
        if (anos is null)
            return Sucessos[^1];

        if (anos < 0)
            return Result<FaixaEtaria>.Falha("faixa_etaria.idade_negativa", $"Idade negativa: {anos}.");

        return Sucessos[Math.Min(anos.Value, InicioAberta) / Largura];
    }

    public static FaixaEtaria? DeRotulo(string rotulo) => PorRotulo.GetValueOrDefault(rotulo);

    public override string ToString() => Rotulo;
}
