using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Validacao;

public sealed record LinhaGabarito(int Ano, string Dimensao, string Categoria, long Obitos);

/// <summary>Confere o cubo de um ano com o gabarito do Tabnet: total, ano × UF e ano × sexo, com igualdade exata.</summary>
public static class ConferenciaTabnet
{
    public static IReadOnlyList<string> Divergencias(int ano, IReadOnlyCollection<CelulaCubo> celulas, IEnumerable<LinhaGabarito> gabarito)
    {
        var esperado = gabarito.Where(g => g.Ano == ano).ToList();
        if (esperado.Count == 0)
            return [$"{ano}: ano ausente do gabarito."];

        long total = 0;
        var porUf = new Dictionary<int, long>();
        var porSexo = new Dictionary<Sexo, long>();
        foreach (var celula in celulas)
        {
            total += celula.Obitos;
            porUf[celula.Uf.CodigoIbge] = porUf.GetValueOrDefault(celula.Uf.CodigoIbge) + celula.Obitos;
            porSexo[celula.Sexo] = porSexo.GetValueOrDefault(celula.Sexo) + celula.Obitos;
        }

        long Calculado(LinhaGabarito g) => g.Dimensao switch
        {
            "total" => total,
            "uf" => porUf.GetValueOrDefault(int.Parse(g.Categoria)),
            "sexo" => SexoNome.Ler(g.Categoria) is { } sexo ? porSexo.GetValueOrDefault(sexo) : -1,
            _ => -1
        };

        return esperado
            .Where(g => Calculado(g) != g.Obitos)
            .Select(g => $"{ano} {g.Dimensao} {g.Categoria}: cubo {Calculado(g)}, Tabnet {g.Obitos}")
            .ToList();
    }

    public static IReadOnlyList<LinhaGabarito> LerGabarito(IEnumerable<string> linhasCsv) =>
        linhasCsv.Skip(1)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Split(';'))
            .Select(c => new LinhaGabarito(int.Parse(c[0]), c[1], c[2], long.Parse(c[3])))
            .ToList();
}
