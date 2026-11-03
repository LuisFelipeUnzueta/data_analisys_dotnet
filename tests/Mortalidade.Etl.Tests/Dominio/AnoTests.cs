using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class AnoTests
{
    [Fact]
    public void Criar_AnoAnteriorA1979_RetornaFalha()
    {
        var resultado = Ano.Criar(1978);

        Assert.True(resultado.IsFailure);
        Assert.Equal("ano.anterior_serie", resultado.Erro!.Codigo);
    }

    [Theory]
    [InlineData(1979)]
    [InlineData(2022)]
    public void Criar_AnoDaSerie_RetornaSucesso(int valor)
    {
        var resultado = Ano.Criar(valor);

        Assert.True(resultado.IsSuccess);
        Assert.Equal(valor, resultado.Valor.Valor);
    }
}
