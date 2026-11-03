using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;

namespace Mortalidade.Etl.Tests.Leitura;

/// <summary>Testes sobre data/ibge/populacao_fontes.csv e a série 1979–2023 estimada a partir dele.</summary>
public class PopulacaoRealTests
{
    private static readonly IReadOnlyList<PontoPopulacao> Pontos =
        LeitorPopulacaoFontes.Ler(File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Dados", "populacao_fontes.csv"))).Valor;

    private static readonly IReadOnlyList<Populacao> Serie = EstimativaPopulacional.Estimar(Pontos, 2023).Valor.Populacao;

    private static long Total(int ano) => Serie.Where(p => p.Ano.Valor == ano).Sum(p => p.Habitantes);

    [Theory]
    [InlineData(1980, 119_009_778)]
    [InlineData(1991, 146_825_475)]
    [InlineData(1996, 157_070_163)]
    [InlineData(2000, 174_695_935)]
    [InlineData(2022, 210_862_983)]
    public void AnosAncora_PreservamOTotalDaFonte(int ano, long total)
    {
        Assert.InRange(Total(ano), total - 30, total + 30);
    }

    [Fact]
    public void Serie_CobreTodosOsEstratosDe1979a2023()
    {
        foreach (var ano in Enumerable.Range(1979, 45))
        {
            var ufs = ano < EstimativaPopulacional.AnoCriacaoTocantins ? 26 : 27;
            Assert.Equal(ufs * 2 * 17, Serie.Count(p => p.Ano.Valor == ano));
        }
    }

    [Fact]
    public void Serie_CresceSemSaltosAnuaisNoTotalNacional()
    {
        var variacoes = Enumerable.Range(1980, 44)
            .Select(ano => (ano, variacao: (double)Total(ano) / Total(ano - 1) - 1))
            .Where(v => v.variacao is < 0 or > 0.035)
            .Select(v => $"{v.ano}: {v.variacao:P2}")
            .ToList();

        Assert.Empty(variacoes);
    }

    [Fact]
    public void Tocantins_SaiDeGoiasEm1989()
    {
        var goias = Uf.DeCodigoIbge(52).Valor;
        long Goias(int ano) => Serie.Where(p => p.Ano.Valor == ano && p.Uf == goias).Sum(p => p.Habitantes);

        Assert.True(Goias(1989) < Goias(1988) * 0.85);
    }
}
