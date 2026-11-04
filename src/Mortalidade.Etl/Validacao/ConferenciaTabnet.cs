using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Validacao;

/// <summary>Totais oficiais de um ano no Tabnet, por residência. A UF 0 é "Ignorado/exterior".</summary>
public sealed record TotaisTabnet(long Total, IReadOnlyDictionary<int, long> PorUf, IReadOnlyDictionary<Sexo, long> PorSexo);

/// <summary>Confere o cubo de um ano com o gabarito do Tabnet: total, ano × UF e ano × sexo, com igualdade exata.</summary>
public static class ConferenciaTabnet
{
    public static IReadOnlyList<string> Divergencias(int ano, IReadOnlyCollection<CelulaCubo> celulas, IReadOnlyDictionary<int, TotaisTabnet> gabarito)
    {
        if (!gabarito.TryGetValue(ano, out var esperado))
            return [$"{ano}: ano ausente do gabarito."];

        var calculado = Totalizar(celulas);
        var divergencias = new List<string>();
        if (calculado.Total != esperado.Total)
            divergencias.Add($"{ano} total: cubo {calculado.Total}, Tabnet {esperado.Total}");

        Comparar(esperado.PorUf, calculado.PorUf, uf => $"uf {uf}");
        Comparar(esperado.PorSexo, calculado.PorSexo, sexo => $"sexo {SexoNome.De(sexo)}");
        return divergencias;

        void Comparar<TChave>(IReadOnlyDictionary<TChave, long> oficial, IReadOnlyDictionary<TChave, long> cubo, Func<TChave, string> nome)
            where TChave : notnull
        {
            foreach (var (chave, obitos) in oficial)
            {
                var noCubo = cubo.GetValueOrDefault(chave);
                if (noCubo != obitos)
                    divergencias.Add($"{ano} {nome(chave)}: cubo {noCubo}, Tabnet {obitos}");
            }
        }
    }

    private static TotaisTabnet Totalizar(IEnumerable<CelulaCubo> celulas)
    {
        long total = 0;
        var porUf = new Dictionary<int, long>();
        var porSexo = new Dictionary<Sexo, long>();
        foreach (var celula in celulas)
        {
            total += celula.Obitos;
            porUf[celula.Uf.CodigoIbge] = porUf.GetValueOrDefault(celula.Uf.CodigoIbge) + celula.Obitos;
            porSexo[celula.Sexo] = porSexo.GetValueOrDefault(celula.Sexo) + celula.Obitos;
        }

        return new TotaisTabnet(total, porUf, porSexo);
    }
}
