using Mortalidade.Etl.Validacao;

namespace Mortalidade.Etl.Tests.Validacao;

public class ContinuidadeCidTests
{
    private static Dictionary<int, long> Serie(params long[] valores1992a1999) =>
        valores1992a1999.Select((v, i) => (ano: 1992 + i, v)).ToDictionary(x => x.ano, x => x.v);

    [Fact]
    public void Desvio_SerieEstavel_RetornaZero()
    {
        Assert.Equal(0, ContinuidadeCid.Desvio(Serie(100, 100, 100, 100, 100, 100, 100, 100))!.Value, 6);
    }

    [Fact]
    public void Desvio_SaltoEm1996_ComparaComATendenciaVizinha()
    {
        // Tendência de +10% ao ano; 1996 sobe 32% sobre 1995 → desvio de 20%.
        var serie = Serie(1000, 1100, 1210, 1331, 1757, 1933, 2126, 2339);

        Assert.Equal(0.20, ContinuidadeCid.Desvio(serie)!.Value, 2);
        Assert.True(ContinuidadeCid.ComparabilidadeReduzida(serie));
    }

    [Fact]
    public void Desvio_SemObitosEm1995_RetornaNulo()
    {
        Assert.Null(ContinuidadeCid.Desvio(new Dictionary<int, long> { [1996] = 10 }));
        Assert.False(ContinuidadeCid.ComparabilidadeReduzida(new Dictionary<int, long>()));
    }
}
