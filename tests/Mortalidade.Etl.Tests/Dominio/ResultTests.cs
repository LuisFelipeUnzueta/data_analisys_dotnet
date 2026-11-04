using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class ResultTests
{
    [Fact]
    public void MapEBind_EncadeiamSucessos()
    {
        var resultado = Result<int>.Sucesso(2)
            .Bind(v => Result<int>.Sucesso(v * 10))
            .Map(v => $"valor {v}");

        Assert.Equal("valor 20", resultado.Valor);
    }

    [Fact]
    public void Bind_PrimeiraFalhaInterrompeACadeia()
    {
        var executou = false;
        var resultado = Result<int>.Sucesso(2)
            .Bind(_ => Result<int>.Falha("passo.um", "falhou"))
            .Map(v => { executou = true; return v; });

        Assert.False(executou);
        Assert.Equal("passo.um", resultado.Erro!.Codigo);
    }

    [Fact]
    public void Uf_DeCodigoHistorico_LevaFernandoDeNoronhaParaPernambuco()
    {
        Assert.Same(Uf.Pernambuco, Uf.DeCodigoHistorico(20).Valor);
        Assert.Same(Uf.Goias, Uf.DeCodigoHistorico(52).Valor);
        Assert.True(Uf.DeCodigoHistorico(20).IsSuccess && Uf.DeCodigoIbge(20).IsFailure);
    }
}
