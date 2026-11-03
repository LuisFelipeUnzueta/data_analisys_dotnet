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

        var calculado = new Dictionary<(string, string), long> { [("total", "total")] = celulas.Sum(c => c.Obitos) };
        foreach (var celula in celulas)
        {
            Somar(("uf", celula.Uf.CodigoIbge.ToString()), celula.Obitos);
            Somar(("sexo", NomeSexo(celula.Sexo)), celula.Obitos);
        }

        return esperado
            .Where(g => calculado.GetValueOrDefault((g.Dimensao, g.Categoria)) != g.Obitos)
            .Select(g => $"{ano} {g.Dimensao} {g.Categoria}: cubo {calculado.GetValueOrDefault((g.Dimensao, g.Categoria))}, Tabnet {g.Obitos}")
            .ToList();

        void Somar((string, string) chave, long obitos) => calculado[chave] = calculado.GetValueOrDefault(chave) + obitos;
    }

    public static IReadOnlyList<LinhaGabarito> LerGabarito(IEnumerable<string> linhasCsv) =>
        linhasCsv.Skip(1)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Split(';'))
            .Select(c => new LinhaGabarito(int.Parse(c[0]), c[1], c[2], long.Parse(c[3])))
            .ToList();

    private static string NomeSexo(Sexo sexo) => sexo switch
    {
        Sexo.Masculino => "masculino",
        Sexo.Feminino => "feminino",
        _ => "ignorado"
    };
}
