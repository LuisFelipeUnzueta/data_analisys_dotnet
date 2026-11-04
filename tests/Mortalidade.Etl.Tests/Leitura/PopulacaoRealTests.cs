using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;

namespace Mortalidade.Etl.Tests.Leitura;

/// <summary>Testes sobre data/ibge/populacao_fontes.csv e a série estimada a partir dele (1979 ao último ano das fontes).</summary>
public class PopulacaoRealTests
{
    private static readonly IReadOnlyList<PontoPopulacao> Pontos =
        LeitorPopulacaoFontes.Ler(File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Dados", "populacao_fontes.csv"))).Valor;

    private static readonly IReadOnlyList<Populacao> Serie = EstimativaPopulacional.Estimar(Pontos, Pontos.Max(p => p.Ano)).Valor.Populacao;

    private static long Total(int ano) => Serie.Where(p => p.Ano.Valor == ano).Sum(p => p.Habitantes);

    private static readonly int UltimoAno = Pontos.Max(p => p.Ano);

    [Fact]
    public void AnosAncora_PreservamOTotalDaFonte()
    {
        // O total vem do próprio arquivo: o teste segue válido quando as fontes são atualizadas.
        foreach (var ano in Pontos.Select(p => p.Ano).Distinct())
        {
            var fonte = Pontos.Where(p => p.Ano == ano).Sum(p => p.Habitantes);
            Assert.InRange(Total(ano), fonte - 30, fonte + 30);
        }
    }

    [Fact]
    public void Censo1980_TemOTotalDoSidra()
    {
        Assert.Equal(119_009_778, Pontos.Where(p => p.Ano == 1980).Sum(p => p.Habitantes));
    }

    [Fact]
    public void Serie_CobreTodosOsEstratosDe1979AoUltimoAno()
    {
        foreach (var ano in Enumerable.Range(1979, UltimoAno - 1978))
        {
            var ufs = ano < Uf.AnoCriacaoTocantins ? 26 : 27;
            Assert.Equal(ufs * 2 * 17, Serie.Count(p => p.Ano.Valor == ano));
        }
    }

    [Fact]
    public void Serie_CresceSemSaltosAnuaisNoTotalNacional()
    {
        var variacoes = Enumerable.Range(1980, UltimoAno - 1979)
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
