namespace Mortalidade.Etl.Dominio;

public sealed record FaixaEtaria
{
    private const int Largura = 5;
    private const int InicioAberta = 80;

    public static FaixaEtaria Ignorada { get; } = new(null, null);

    public static IReadOnlyList<FaixaEtaria> Todas { get; } =
        Enumerable.Range(0, InicioAberta / Largura)
            .Select(i => new FaixaEtaria(i * Largura, i * Largura + Largura - 1))
            .Append(new FaixaEtaria(InicioAberta, null))
            .Append(Ignorada)
            .ToArray();

    private FaixaEtaria(int? inicio, int? fim)
    {
        Inicio = inicio;
        Fim = fim;
    }

    public int? Inicio { get; }

    public int? Fim { get; }

    public string Rotulo => (Inicio, Fim) switch
    {
        (null, _) => "Ignorada",
        (var i, null) => $"{i}+",
        var (i, f) => $"{i}-{f}"
    };

    public static Result<FaixaEtaria> DeIdadeEmAnos(int? anos)
    {
        if (anos is null)
            return Result<FaixaEtaria>.Sucesso(Ignorada);

        if (anos < 0)
            return Result<FaixaEtaria>.Falha("faixa_etaria.idade_negativa", $"Idade negativa: {anos}.");

        var indice = Math.Min(anos.Value, InicioAberta) / Largura;
        return Result<FaixaEtaria>.Sucesso(Todas[indice]);
    }

    public override string ToString() => Rotulo;
}
