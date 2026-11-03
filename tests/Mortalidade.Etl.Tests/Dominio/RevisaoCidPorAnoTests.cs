using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class RevisaoCidPorAnoTests
{
    [Theory]
    [InlineData(1979, RevisaoCid.Cid9)]
    [InlineData(1995, RevisaoCid.Cid9)]
    [InlineData(1996, RevisaoCid.Cid10)]
    [InlineData(2022, RevisaoCid.Cid10)]
    public void De_TrocaDeRevisaoEm1996(int ano, RevisaoCid esperada)
    {
        Assert.Equal(esperada, RevisaoCidPorAno.De(Ano.Criar(ano).Valor));
    }
}
