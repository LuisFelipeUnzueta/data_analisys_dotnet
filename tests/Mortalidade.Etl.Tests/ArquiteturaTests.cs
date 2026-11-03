using Mortalidade.Etl.Dominio;
using NetArchTest.Rules;

namespace Mortalidade.Etl.Tests;

public class ArquiteturaTests
{
    [Fact]
    public void Dominio_NaoDependeDeLeituraCuboValidacaoNemParquet()
    {
        var resultado = Types.InAssembly(typeof(Ano).Assembly)
            .That()
            .ResideInNamespace("Mortalidade.Etl.Dominio")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Mortalidade.Etl.Leitura",
                "Mortalidade.Etl.Cubo",
                "Mortalidade.Etl.Validacao",
                "Mortalidade.Etl.Pipeline",
                "Parquet")
            .GetResult();

        Assert.True(resultado.IsSuccessful, string.Join(", ", resultado.FailingTypeNames ?? []));
    }
}
