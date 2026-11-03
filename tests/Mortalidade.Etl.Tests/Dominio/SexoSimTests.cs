using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class SexoSimTests
{
    [Theory]
    [InlineData("1", Sexo.Masculino)]
    [InlineData("M", Sexo.Masculino)]
    [InlineData("2", Sexo.Feminino)]
    [InlineData("f", Sexo.Feminino)]
    [InlineData("0", Sexo.Ignorado)]
    [InlineData("9", Sexo.Ignorado)]
    [InlineData("", Sexo.Ignorado)]
    [InlineData(null, Sexo.Ignorado)]
    public void DeCodigo_MapeiaSemDescartar(string? codigo, Sexo esperado)
    {
        Assert.Equal(esperado, SexoSim.DeCodigo(codigo));
    }
}
