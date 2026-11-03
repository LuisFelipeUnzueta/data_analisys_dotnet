namespace Mortalidade.Etl.Dominio;

public sealed record Uf
{
    private static readonly IReadOnlyDictionary<int, string> Siglas = new Dictionary<int, string>
    {
        [11] = "RO", [12] = "AC", [13] = "AM", [14] = "RR", [15] = "PA", [16] = "AP", [17] = "TO",
        [21] = "MA", [22] = "PI", [23] = "CE", [24] = "RN", [25] = "PB", [26] = "PE", [27] = "AL", [28] = "SE", [29] = "BA",
        [31] = "MG", [32] = "ES", [33] = "RJ", [35] = "SP",
        [41] = "PR", [42] = "SC", [43] = "RS",
        [50] = "MS", [51] = "MT", [52] = "GO", [53] = "DF"
    };

    public static IReadOnlyCollection<Uf> Todas { get; } =
        Siglas.Select(par => new Uf(par.Key, par.Value)).ToArray();

    private Uf(int codigoIbge, string sigla)
    {
        CodigoIbge = codigoIbge;
        Sigla = sigla;
    }

    public int CodigoIbge { get; }

    public string Sigla { get; }

    public Regiao Regiao => (Regiao)(CodigoIbge / 10);

    public static Result<Uf> DeCodigoIbge(int codigo) =>
        Siglas.TryGetValue(codigo, out var sigla)
            ? Result<Uf>.Sucesso(new Uf(codigo, sigla))
            : Result<Uf>.Falha("uf.codigo_invalido", $"Código IBGE de UF inválido: {codigo}.");

    public override string ToString() => Sigla;
}
