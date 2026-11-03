namespace Mortalidade.Etl.Dominio;

/// <summary>
/// Redistribui os óbitos de idade ignorada de um ano entre as faixas conhecidas, preenchendo <see cref="CelulaCubo.ObitosAjustados"/>.
/// Proporção dentro de UF × grupo × sexo; sem idade conhecida no estrato, usa UF × sexo (todas as causas) e depois a UF.
/// </summary>
public static class RedistribuicaoIdade
{
    public static Result<IReadOnlyList<CelulaCubo>> Aplicar(IReadOnlyCollection<CelulaCubo> celulasDoAno)
    {
        var porUfSexo = Distribuicoes(celulasDoAno, c => (c.Uf, c.Sexo));
        var porUf = Distribuicoes(celulasDoAno, c => (c.Uf, Sexo.Ignorado));
        var resultado = new List<CelulaCubo>();

        foreach (var estrato in celulasDoAno.GroupBy(c => (c.Uf, c.GrupoCausa, c.Sexo)))
        {
            var conhecidas = estrato.Where(c => c.FaixaEtaria != FaixaEtaria.Ignorada).ToList();
            var ignorados = estrato.Where(c => c.FaixaEtaria == FaixaEtaria.Ignorada).Sum(c => c.Obitos);
            var totalConhecido = conhecidas.Sum(c => c.Obitos);

            resultado.AddRange(estrato.Where(c => c.FaixaEtaria == FaixaEtaria.Ignorada).Select(c => c with { ObitosAjustados = 0 }));
            if (totalConhecido > 0)
            {
                var fator = 1 + (double)ignorados / totalConhecido;
                resultado.AddRange(conhecidas.Select(c => c with { ObitosAjustados = c.Obitos * fator }));
                continue;
            }

            if (ignorados == 0)
                continue;

            var distribuicao = porUfSexo.GetValueOrDefault((estrato.Key.Uf, estrato.Key.Sexo))
                ?? porUf.GetValueOrDefault((estrato.Key.Uf, Sexo.Ignorado));
            if (distribuicao is null)
                return Result<IReadOnlyList<CelulaCubo>>.Falha("idade.sem_distribuicao",
                    $"{estrato.Key.Uf}: nenhum óbito com idade conhecida para redistribuir {ignorados} ignorados.");

            var modelo = estrato.First();
            resultado.AddRange(distribuicao.Select(d =>
                modelo with { FaixaEtaria = d.Key, Obitos = 0, ObitosAjustados = ignorados * d.Value }));
        }

        return Result<IReadOnlyList<CelulaCubo>>.Sucesso(resultado);
    }

    private static Dictionary<(Uf, Sexo), Dictionary<FaixaEtaria, double>> Distribuicoes(
        IEnumerable<CelulaCubo> celulas, Func<CelulaCubo, (Uf, Sexo)> chave) =>
        celulas.Where(c => c.FaixaEtaria != FaixaEtaria.Ignorada && c.Obitos > 0)
            .GroupBy(chave)
            .ToDictionary(g => g.Key, g =>
            {
                var total = (double)g.Sum(c => c.Obitos);
                return g.GroupBy(c => c.FaixaEtaria).ToDictionary(f => f.Key, f => f.Sum(c => c.Obitos) / total);
            });
}
