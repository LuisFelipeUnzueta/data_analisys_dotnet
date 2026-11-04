using System.Collections.Concurrent;

namespace Mortalidade.Etl.Dominio;

/// <param name="FaixasCid9">Faixas por prefixo ("410-414", "4275"); idem <paramref name="FaixasCid10"/>.</param>
public sealed record DefinicaoGrupo(
    string Codigo, string Nome, string Capitulo, bool Resto, bool ComparabilidadeReduzida,
    IReadOnlyList<string> FaixasCid9, IReadOnlyList<string> FaixasCid10);

/// <summary>
/// Correspondência causa básica → grupo harmonizado (definida em <c>data/cause_map/grupos.csv</c>, lida por <c>LeitorMapaCausas</c>).
/// Faixas casam por prefixo; entre grupos específicos vence a faixa mais longa, e grupos "resto" são o fallback.
/// </summary>
public sealed class MapaCausas
{
    private readonly Dictionary<RevisaoCid, Faixa[]> _faixas;
    private readonly ConcurrentDictionary<CausaBasica, Result<GrupoCausa>> _resolvidas = new();

    private MapaCausas(IReadOnlyList<GrupoCausa> grupos, Dictionary<RevisaoCid, Faixa[]> faixas)
    {
        Grupos = grupos;
        _faixas = faixas;
    }

    public IReadOnlyList<GrupoCausa> Grupos { get; }

    /// <summary>Monta o mapa a partir das definições já lidas; valida códigos únicos e faixas "A-B" do mesmo tamanho.</summary>
    public static Result<MapaCausas> Criar(IReadOnlyList<DefinicaoGrupo> definicoes)
    {
        var grupos = new List<GrupoCausa>();
        var codigos = new HashSet<string>();
        var faixas = new Dictionary<RevisaoCid, List<Faixa>> { [RevisaoCid.Cid9] = [], [RevisaoCid.Cid10] = [] };
        foreach (var definicao in definicoes)
        {
            if (!codigos.Add(definicao.Codigo))
                return Result<MapaCausas>.Falha("mapa.codigo_duplicado", $"Grupo duplicado: {definicao.Codigo}.");

            var grupo = new GrupoCausa(grupos.Count, definicao.Codigo, definicao.Nome, definicao.Capitulo, definicao.ComparabilidadeReduzida);
            foreach (var (revisao, partes) in new[] { (RevisaoCid.Cid9, definicao.FaixasCid9), (RevisaoCid.Cid10, definicao.FaixasCid10) })
            {
                foreach (var parte in partes)
                {
                    var limites = parte.Split('-');
                    var (inicio, fim) = (limites[0], limites.Length > 1 ? limites[1] : limites[0]);
                    if (limites.Length > 2 || inicio.Length != fim.Length || string.CompareOrdinal(inicio, fim) > 0)
                        return Result<MapaCausas>.Falha("mapa.faixa_invalida", $"Faixa inválida no grupo {definicao.Codigo}: {parte}.");

                    faixas[revisao].Add(new Faixa(grupo, inicio, fim, definicao.Resto));
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
