using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class UfTests
{
    [Fact]
    public void Todas_Contem27Ufs()
    {
        Assert.Equal(27, Uf.Todas.Count);
    }

    [Theory]
    [InlineData(13, "AM", Regiao.Norte)]
    [InlineData(29, "BA", Regiao.Nordeste)]
    [InlineData(35, "SP", Regiao.Sudeste)]
    [InlineData(43, "RS", Regiao.Sul)]
    [InlineData(53, "DF", Regiao.CentroOeste)]
    public void DeCodigoIbge_CodigoValido_RetornaUfComRegiao(int codigo, string sigla, Regiao regiao)
    {
        var uf = Uf.DeCodigoIbge(codigo).Valor;

        Assert.Equal(sigla, uf.Sigla);
        Assert.Equal(regiao, uf.Regiao);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(34)]
    [InlineData(99)]
    public void DeCodigoIbge_CodigoInvalido_RetornaFalha(int codigo)
    {
        var resultado = Uf.DeCodigoIbge(codigo);

        Assert.True(resultado.IsFailure);
        Assert.Equal("uf.codigo_invalido", resultado.Erro!.Codigo);
    }
}
