using Mortalidade.Etl.Cubo;
using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Pipeline;

/// <summary>Grava o que a página consome: o cubo e a população dos anos aprovados e o JSON de dimensões.</summary>
public static class PublicacaoSaida
{
    public static async Task GravarAsync(
        string diretorio, Entradas entradas, IReadOnlyList<ResultadoAno> aprovados,
        IReadOnlyDictionary<int, (double? Bruta, double? Padronizada)> taxasBrasil)
    {
        var anos = aprovados.Select(r => r.Ano).ToHashSet();
        Directory.CreateDirectory(diretorio);

        await using (var arquivo = File.Create(Path.Combine(diretorio, "obitos.parquet")))
            await EscritorParquet.GravarObitosAsync(arquivo, aprovados.OrderBy(r => r.Ano).Select(r => r.Celulas));

        await using (var arquivo = File.Create(Path.Combine(diretorio, "populacao.parquet")))
            await EscritorParquet.GravarPopulacaoAsync(arquivo, entradas.Populacao.Where(p => anos.Contains(p.Ano.Valor)).ToList());

        var taxasPublicadas = taxasBrasil.Where(t => anos.Contains(t.Key)).ToDictionary();
        await File.WriteAllTextAsync(Path.Combine(diretorio, "dimensoes.json"),
            GeradorDimensoes.Gerar(entradas.Mapa, entradas.Padrao, taxasPublicadas, entradas.FontePopulacao, DateTimeOffset.UtcNow));
    }
}
