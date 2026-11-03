namespace Mortalidade.Etl.Dominio;

/// <summary>
/// Correspondência causa básica → grupo harmonizado, carregada de <c>data/cause_map/grupos.csv</c>.
/// Faixas casam por prefixo; entre grupos específicos vence a faixa mais longa, e grupos "resto" são o fallback.
/// </summary>
public sealed class MapaCausas
{
    private static readonly string[] Colunas = ["codigo", "nome", "capitulo", "resto", "cid9", "cid10", "comparabilidade"];

    private readonly IReadOnlyList<Faixa> _faixas;

    private MapaCausas(IReadOnlyList<GrupoCausa> grupos, IReadOnlyList<Faixa> faixas)
    {
        Grupos = grupos;
        _faixas = faixas;
    }

    public IReadOnlyList<GrupoCausa> Grupos { get; }

    public static Result<MapaCausas> Carregar(IEnumerable<string> linhasCsv)
    {
        using var linhas = linhasCsv.GetEnumerator();
        if (!linhas.MoveNext() || !linhas.Current.Trim().Split(';').SequenceEqual(Colunas))
            return Result<MapaCausas>.Falha("mapa.cabecalho_invalido", $"Cabeçalho esperado: {string.Join(';', Colunas)}.");

        var grupos = new List<GrupoCausa>();
        var faixas = new List<Faixa>();
        var numero = 1;
        while (linhas.MoveNext())
        {
            numero++;
            if (string.IsNullOrWhiteSpace(linhas.Current))
                continue;

            var campos = linhas.Current.Split(';');
            if (campos.Length != Colunas.Length || campos[3] is not ("0" or "1") || campos[6] is not ("alta" or "reduzida"))
                return Result<MapaCausas>.Falha("mapa.linha_invalida", $"Linha {numero} inválida: {linhas.Current}");

            if (grupos.Any(g => g.Codigo == campos[0]))
                return Result<MapaCausas>.Falha("mapa.codigo_duplicado", $"Grupo duplicado na linha {numero}: {campos[0]}.");

            var grupo = new GrupoCausa(campos[0], campos[1], campos[2], campos[6] == "reduzida");
            var resto = campos[3] == "1";
            foreach (var (revisao, spec) in new[] { (RevisaoCid.Cid9, campos[4]), (RevisaoCid.Cid10, campos[5]) })
            {
                foreach (var parte in spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    var limites = parte.Split('-');
                    var (inicio, fim) = (limites[0], limites.Length > 1 ? limites[1] : limites[0]);
                    if (limites.Length > 2 || inicio.Length != fim.Length || string.CompareOrdinal(inicio, fim) > 0)
                        return Result<MapaCausas>.Falha("mapa.faixa_invalida", $"Faixa inválida na linha {numero}: {parte}.");

                    faixas.Add(new Faixa(grupo, revisao, inicio, fim, resto));
                }
            }

            grupos.Add(grupo);
        }

        return Result<MapaCausas>.Sucesso(new MapaCausas(grupos, faixas));
    }

    public Result<GrupoCausa> Resolver(CausaBasica causa)
    {
        var codigo = causa.Codigo.Trim().ToUpperInvariant();
        var candidatas = _faixas.Where(f => f.Revisao == causa.Revisao && f.Contem(codigo)).ToList();

        var especificas = candidatas.Where(f => !f.Resto).ToList();
        var escolhidas = especificas.Count > 0
            ? especificas.Where(f => f.Inicio.Length == especificas.Max(e => e.Inicio.Length)).ToList()
            : candidatas;

        var grupos = escolhidas.Select(f => f.Grupo).Distinct().ToList();
        return grupos.Count switch
        {
            1 => Result<GrupoCausa>.Sucesso(grupos[0]),
            0 => Result<GrupoCausa>.Falha("causa.sem_grupo", $"Causa {causa.Revisao} {codigo} sem grupo."),
            _ => Result<GrupoCausa>.Falha("causa.conflito",
                $"Causa {causa.Revisao} {codigo} casa com {string.Join(", ", grupos.Select(g => g.Codigo))}.")
        };
    }

    private sealed record Faixa(GrupoCausa Grupo, RevisaoCid Revisao, string Inicio, string Fim, bool Resto)
    {
        public bool Contem(string codigo) =>
            string.CompareOrdinal(Inicio, Prefixo(codigo, Inicio.Length)) <= 0
            && string.CompareOrdinal(Prefixo(codigo, Fim.Length), Fim) <= 0;

        private static string Prefixo(string codigo, int tamanho) =>
            codigo.Length > tamanho ? codigo[..tamanho] : codigo;
    }
}
