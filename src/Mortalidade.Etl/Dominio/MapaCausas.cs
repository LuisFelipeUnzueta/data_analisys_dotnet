using System.Collections.Concurrent;

namespace Mortalidade.Etl.Dominio;

/// <summary>
/// Correspondência causa básica → grupo harmonizado, carregada de <c>data/cause_map/grupos.csv</c>.
/// Faixas casam por prefixo; entre grupos específicos vence a faixa mais longa, e grupos "resto" são o fallback.
/// </summary>
public sealed class MapaCausas
{
    private static readonly string[] Colunas = ["codigo", "nome", "capitulo", "resto", "cid9", "cid10", "comparabilidade"];

    private readonly Dictionary<RevisaoCid, Faixa[]> _faixas;
    private readonly ConcurrentDictionary<CausaBasica, Result<GrupoCausa>> _resolvidas = new();

    private MapaCausas(IReadOnlyList<GrupoCausa> grupos, Dictionary<RevisaoCid, Faixa[]> faixas)
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
        var codigos = new HashSet<string>();
        var faixas = new Dictionary<RevisaoCid, List<Faixa>> { [RevisaoCid.Cid9] = [], [RevisaoCid.Cid10] = [] };
        var numero = 1;
        while (linhas.MoveNext())
        {
            numero++;
            if (string.IsNullOrWhiteSpace(linhas.Current))
                continue;

            var campos = linhas.Current.Split(';');
            if (campos.Length != Colunas.Length || campos[3] is not ("0" or "1") || campos[6] is not ("alta" or "reduzida"))
                return Result<MapaCausas>.Falha("mapa.linha_invalida", $"Linha {numero} inválida: {linhas.Current}");

            if (!codigos.Add(campos[0]))
                return Result<MapaCausas>.Falha("mapa.codigo_duplicado", $"Grupo duplicado na linha {numero}: {campos[0]}.");

            var grupo = new GrupoCausa(grupos.Count, campos[0], campos[1], campos[2], campos[6] == "reduzida");
            var resto = campos[3] == "1";
            foreach (var (revisao, spec) in new[] { (RevisaoCid.Cid9, campos[4]), (RevisaoCid.Cid10, campos[5]) })
            {
                foreach (var parte in spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    var limites = parte.Split('-');
                    var (inicio, fim) = (limites[0], limites.Length > 1 ? limites[1] : limites[0]);
                    if (limites.Length > 2 || inicio.Length != fim.Length || string.CompareOrdinal(inicio, fim) > 0)
                        return Result<MapaCausas>.Falha("mapa.faixa_invalida", $"Faixa inválida na linha {numero}: {parte}.");

                    faixas[revisao].Add(new Faixa(grupo, inicio, fim, resto));
                }
            }

            grupos.Add(grupo);
        }

        return Result<MapaCausas>.Sucesso(new MapaCausas(grupos, faixas.ToDictionary(f => f.Key, f => f.Value.ToArray())));
    }

    /// <summary>Resolve uma causa; o resultado fica em cache por código (thread-safe), então cada código é resolvido uma vez.</summary>
    public Result<GrupoCausa> Resolver(CausaBasica causa) => _resolvidas.GetOrAdd(causa, ResolverSemCache);

    private Result<GrupoCausa> ResolverSemCache(CausaBasica causa)
    {
        var codigo = causa.Codigo.Trim().ToUpperInvariant();
        GrupoCausa? especifico = null, resto = null, conflito = null;
        var tamanhoEspecifico = 0;

        foreach (var faixa in _faixas[causa.Revisao])
        {
            if (!faixa.Contem(codigo))
                continue;

            if (faixa.Resto)
            {
                if (resto is null)
                    resto = faixa.Grupo;
                else if (resto != faixa.Grupo && especifico is null)
                    conflito = faixa.Grupo;
            }
            else if (faixa.Inicio.Length > tamanhoEspecifico)
            {
                (especifico, tamanhoEspecifico, conflito) = (faixa.Grupo, faixa.Inicio.Length, null);
            }
            else if (faixa.Inicio.Length == tamanhoEspecifico && faixa.Grupo != especifico)
            {
                conflito = faixa.Grupo;
            }
        }

        var escolhido = especifico ?? resto;
        if (conflito is not null)
            return Result<GrupoCausa>.Falha("causa.conflito",
                $"Causa {causa.Revisao} {codigo} casa com {escolhido!.Codigo}, {conflito.Codigo}.");

        return escolhido is null
            ? Result<GrupoCausa>.Falha("causa.sem_grupo", $"Causa {causa.Revisao} {codigo} sem grupo.")
            : Result<GrupoCausa>.Sucesso(escolhido);
    }

    private sealed record Faixa(GrupoCausa Grupo, string Inicio, string Fim, bool Resto)
    {
        public bool Contem(string codigo) =>
            Inicio.AsSpan().SequenceCompareTo(Prefixo(codigo, Inicio.Length)) <= 0
            && Prefixo(codigo, Fim.Length).SequenceCompareTo(Fim) <= 0;

        private static ReadOnlySpan<char> Prefixo(string codigo, int tamanho) =>
            codigo.AsSpan(0, Math.Min(codigo.Length, tamanho));
    }
}
