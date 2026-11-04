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

    // Resultados pré-alocados: DeCodigoIbge roda uma vez por óbito.
    private static readonly Dictionary<int, Result<Uf>> PorCodigo = Todas.ToDictionary(u => u.CodigoIbge, Result<Uf>.Sucesso);

    /// <summary>Tocantins foi criado em 1988 a partir do norte de Goiás; o projeto o conta separado a partir de 1989.</summary>
    public const int AnoCriacaoTocantins = 1989;

    // Antigo Território de Fernando de Noronha, incorporado a Pernambuco em 1988; o SIM ainda usa o código 20.
    private const int CodigoFernandoDeNoronha = 20;

    public static Uf Tocantins { get; } = PorCodigo[17].Valor;

    public static Uf Goias { get; } = PorCodigo[52].Valor;

    public static Uf Pernambuco { get; } = PorCodigo[26].Valor;

    private Uf(int codigoIbge, string sigla)
    {
        CodigoIbge = codigoIbge;
        Sigla = sigla;
    }

    public int CodigoIbge { get; }

    public string Sigla { get; }

    public Regiao Regiao => (Regiao)(CodigoIbge / 10);

    public static Result<Uf> DeCodigoIbge(int codigo) =>
        PorCodigo.GetValueOrDefault(codigo)
        ?? Result<Uf>.Falha("uf.codigo_invalido", $"Código IBGE de UF inválido: {codigo}.");

    /// <summary>Como <see cref="DeCodigoIbge"/>, mas aceita o código histórico de Fernando de Noronha (20 → PE), como faz o Tabnet.</summary>
    public static Result<Uf> DeCodigoHistorico(int codigo) =>
        DeCodigoIbge(codigo == CodigoFernandoDeNoronha ? Pernambuco.CodigoIbge : codigo);

    public override string ToString() => Sigla;
}
