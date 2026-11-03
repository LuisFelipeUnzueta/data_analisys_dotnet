using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class MapaCausasTests
{
    private const string Cabecalho = "codigo;nome;capitulo;resto;cid9;cid10;comparabilidade";

    private static MapaCausas Mapa(params string[] linhas) =>
        MapaCausas.Carregar([Cabecalho, .. linhas]).Valor;

    private static string Resolver(MapaCausas mapa, RevisaoCid revisao, string codigo)
    {
        var resultado = mapa.Resolver(new CausaBasica(revisao, codigo));
        return resultado.IsSuccess ? resultado.Valor.Codigo : resultado.Erro!.Codigo;
    }

    [Fact]
    public void Resolver_FaixaMaisLongaVenceEntreEspecificos()
    {
        var mapa = Mapa(
            "cardiacas;Cardíacas;circ;0;415-429;I26-I52;alta",
            "mal_definidas;Mal definidas;md;0;780-799,4275;R00-R99,I469;alta");

        Assert.Equal("mal_definidas", Resolver(mapa, RevisaoCid.Cid9, "4275"));
        Assert.Equal("cardiacas", Resolver(mapa, RevisaoCid.Cid9, "4280"));
        Assert.Equal("mal_definidas", Resolver(mapa, RevisaoCid.Cid10, "I469"));
        Assert.Equal("cardiacas", Resolver(mapa, RevisaoCid.Cid10, "I461"));
    }

    [Fact]
    public void Resolver_SemEspecifico_UsaResto()
    {
        var mapa = Mapa(
            "isquemicas;Isquêmicas;circ;0;410-414;I20-I25;alta",
            "circ_resto;Restante;circ;1;390-459;I00-I99;alta");

        Assert.Equal("isquemicas", Resolver(mapa, RevisaoCid.Cid10, "I219"));
        Assert.Equal("circ_resto", Resolver(mapa, RevisaoCid.Cid10, "I710"));
        Assert.Equal("circ_resto", Resolver(mapa, RevisaoCid.Cid9, "436X"));
    }

    [Fact]
    public void Resolver_UsaSomenteAFaixaDaRevisaoDaCausa()
    {
        var mapa = Mapa("tetano;Tétano;inf;0;037,7713;A33-A35;alta");

        Assert.Equal("tetano", Resolver(mapa, RevisaoCid.Cid9, "7713"));
        Assert.Equal("causa.sem_grupo", Resolver(mapa, RevisaoCid.Cid10, "7713"));
    }

    [Fact]
    public void Resolver_EmpateEntreEspecificos_RetornaConflito()
    {
        var mapa = Mapa(
            "a;A;x;0;;C70-C72;alta",
            "b;B;x;0;;C71;alta");

        Assert.Equal("causa.conflito", Resolver(mapa, RevisaoCid.Cid10, "C719"));
    }

    [Fact]
    public void Resolver_CodigoForaDeTodasAsFaixas_RetornaSemGrupo()
    {
        var mapa = Mapa("diabetes;Diabetes;end;0;250;E10-E14;alta");

        Assert.Equal("causa.sem_grupo", Resolver(mapa, RevisaoCid.Cid10, "Z999"));
    }

    [Fact]
    public void Carregar_LeComparabilidade()
    {
        var mapa = Mapa("septicemia;Septicemia;inf;0;038;A40-A41;reduzida");

        Assert.True(mapa.Grupos.Single().ComparabilidadeReduzida);
    }

    [Theory]
    [InlineData("codigo;nome;resto;cid9;cid10", "mapa.cabecalho_invalido")]
    [InlineData(Cabecalho + "\nx;X;c;2;001;A00;alta", "mapa.linha_invalida")]
    [InlineData(Cabecalho + "\nx;X;c;0;001;A00;media", "mapa.linha_invalida")]
    [InlineData(Cabecalho + "\nx;X;c;0;009-001;A00;alta", "mapa.faixa_invalida")]
    [InlineData(Cabecalho + "\nx;X;c;0;001-0099;A00;alta", "mapa.faixa_invalida")]
    [InlineData(Cabecalho + "\nx;X;c;0;001;A00;alta\nx;Y;c;0;002;A01;alta", "mapa.codigo_duplicado")]
    public void Carregar_ConteudoInvalido_RetornaFalha(string conteudo, string erro)
    {
        var resultado = MapaCausas.Carregar(conteudo.Split('\n'));

        Assert.True(resultado.IsFailure);
        Assert.Equal(erro, resultado.Erro!.Codigo);
    }
}
