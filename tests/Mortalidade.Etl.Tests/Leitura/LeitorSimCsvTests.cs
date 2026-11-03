using System.Text;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;

namespace Mortalidade.Etl.Tests.Leitura;

public class LeitorSimCsvTests
{
    private static List<Result<RegistroObito>> Ler(int ano, params string[] linhas)
    {
        using var csv = new MemoryStream(Encoding.Latin1.GetBytes(string.Join("\r\n", linhas) + "\r\n"));
        return LeitorSimCsv.Ler(csv, Ano.Criar(ano).Valor).ToList();
    }

    [Fact]
    public void Ler_LayoutCid9_UsaMuniresECompletaRegistro()
    {
        var registros = Ler(1979,
            "\"CONTADOR\";\"TIPOBITO\";\"DATAOBITO\";\"SEXO\";\"IDADE\";\"MUNIOCOR\";\"MUNIRES\";\"CAUSABAS\"",
            "\"1\";\"2\";\"790600\";\"1\";\"463\";\"1100304\";\"1200000\";\"436X\"");

        var obito = Assert.Single(registros).Valor;
        Assert.Equal("AC", obito.UfResidencia.Sigla);
        Assert.Equal(new CausaBasica(RevisaoCid.Cid9, "436X"), obito.Causa);
        Assert.Equal(Sexo.Masculino, obito.Sexo);
        Assert.Equal("60-64", obito.FaixaEtaria.Rotulo);
    }

    [Fact]
    public void Ler_Layout2023EmMinusculasESemZerosAEsquerda()
    {
        var registros = Ler(2023,
            "\"contador\";\"tipobito\";\"dtobito\";\"idade\";\"sexo\";\"codmunres\";\"codmunocor\";\"causabas\"",
            "\"1\";\"2\";\"1012023\";\"5\";\"2\";\"355030\";\"355030\";\"i219\"");

        var obito = Assert.Single(registros).Valor;
        Assert.Equal("SP", obito.UfResidencia.Sigla);
        Assert.Equal(new CausaBasica(RevisaoCid.Cid10, "I219"), obito.Causa);
        Assert.Equal("0-4", obito.FaixaEtaria.Rotulo);
    }

    [Fact]
    public void Ler_DescartaObitoFetal()
    {
        var registros = Ler(2010,
            "TIPOBITO;IDADE;SEXO;CODMUNRES;CODMUNOCOR;CAUSABAS",
            "1;000;1;355030;355030;P95",
            "2;470;2;355030;355030;C509");

        Assert.Equal("C509", Assert.Single(registros).Valor.Causa.Codigo);
    }

    [Theory]
    [InlineData("2000107", "1234567", "PE")]
    [InlineData("0000000", "3304557", "RJ")]
    [InlineData("", "530010", "DF")]
    public void Ler_UfDeResidencia_ComNoronhaEFallbackDeOcorrencia(string residencia, string ocorrencia, string sigla)
    {
        var registros = Ler(1990, "TIPOBITO;IDADE;SEXO;MUNIRES;MUNIOCOR;CAUSABAS",
            $"2;450;1;{residencia};{ocorrencia};4109");

        Assert.Equal(sigla, Assert.Single(registros).Valor.UfResidencia.Sigla);
    }

    [Fact]
    public void Ler_SemUfValida_RetornaFalha()
    {
        var registros = Ler(1990, "TIPOBITO;IDADE;SEXO;MUNIRES;MUNIOCOR;CAUSABAS", "2;450;1;9900000;;4109");

        Assert.Equal("sim.uf_invalida", Assert.Single(registros).Erro!.Codigo);
    }

    [Fact]
    public void Ler_SemColunaObrigatoria_RetornaFalhaEPara()
    {
        var registros = Ler(1990, "TIPOBITO;IDADE;SEXO;MUNIRES;CAUSABAS", "2;450;1;3304557;4109");

        Assert.Equal("sim.coluna_ausente", Assert.Single(registros).Erro!.Codigo);
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("000", null)]
    [InlineData("0", null)]
    [InlineData("999", null)]
    [InlineData("030", 0)]
    [InlineData("1", 0)]
    [InlineData("215", 0)]
    [InlineData("311", 0)]
    [InlineData("400", 0)]
    [InlineData("479", 79)]
    [InlineData("505", 105)]
    [InlineData("812", null)]
    [InlineData("4X5", null)]
    public void IdadeEmAnos_DecodificaUnidadesDoSim(string campo, int? esperado)
    {
        Assert.Equal(esperado, LeitorSimCsv.IdadeEmAnos(campo));
    }
}
