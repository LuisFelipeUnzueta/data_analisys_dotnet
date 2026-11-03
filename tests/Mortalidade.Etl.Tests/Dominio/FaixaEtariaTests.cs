using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class FaixaEtariaTests
{
    [Fact]
    public void Todas_Contem16QuinquenaisMaisAbertaEIgnorada()
    {
        Assert.Equal(18, FaixaEtaria.Todas.Count);
        Assert.Equal("0-4", FaixaEtaria.Todas[0].Rotulo);
        Assert.Equal("80+", FaixaEtaria.Todas[16].Rotulo);
        Assert.Same(FaixaEtaria.Ignorada, FaixaEtaria.Todas[17]);
    }

    [Theory]
    [InlineData(0, "0-4")]
    [InlineData(4, "0-4")]
    [InlineData(5, "5-9")]
    [InlineData(79, "75-79")]
    [InlineData(80, "80+")]
    [InlineData(115, "80+")]
    public void DeIdadeEmAnos_IdadeValida_RetornaFaixa(int anos, string rotulo)
    {
        Assert.Equal(rotulo, FaixaEtaria.DeIdadeEmAnos(anos).Valor.Rotulo);
    }

    [Fact]
    public void DeIdadeEmAnos_IdadeNula_RetornaIgnorada()
    {
        Assert.Same(FaixaEtaria.Ignorada, FaixaEtaria.DeIdadeEmAnos(null).Valor);
    }

    [Fact]
    public void DeIdadeEmAnos_IdadeNegativa_RetornaFalha()
    {
        Assert.True(FaixaEtaria.DeIdadeEmAnos(-1).IsFailure);
    }
}
