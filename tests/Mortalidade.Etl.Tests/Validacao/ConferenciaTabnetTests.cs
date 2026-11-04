using Mortalidade.Etl.Leitura;
using Mortalidade.Etl.Cubo;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Validacao;

namespace Mortalidade.Etl.Tests.Validacao;

public class ConferenciaTabnetTests
{
    private static readonly MapaCausas Mapa = LeitorMapaCausas.Ler([
        "codigo;nome;capitulo;resto;cid9;cid10;comparabilidade",
        "isquemicas;Isquêmicas;circ;0;410-414;I20-I25;alta"]).Valor;

    private static RegistroObito Obito(int uf, Sexo sexo, string causa = "I219") => new(
        Ano.Criar(2010).Valor, Uf.DeCodigoIbge(uf).Valor, new CausaBasica(RevisaoCid.Cid10, causa), sexo,
        FaixaEtaria.DeIdadeEmAnos(70).Valor);

    private static IReadOnlyList<CelulaCubo> Cubo(params RegistroObito[] obitos)
    {
        var agregador = new AgregadorCubo(Mapa, Ano.Criar(2010).Valor);
        foreach (var obito in obitos)
            agregador.Adicionar(Result<RegistroObito>.Sucesso(obito));
        return agregador.Celulas();
    }

    private static readonly IReadOnlyDictionary<int, TotaisTabnet> Gabarito = LeitorGabaritoTabnet.Ler([
        "ano;dimensao;categoria;obitos;consulta",
        "2010;total;total;3;2026-11-03",
        "2010;uf;00;0;2026-11-03",
        "2010;uf;35;2;2026-11-03",
        "2010;uf;33;1;2026-11-03",
        "2010;sexo;masculino;2;2026-11-03",
        "2010;sexo;feminino;1;2026-11-03",
        "2010;sexo;ignorado;0;2026-11-03"]).Valor;

    [Theory]
    [InlineData("2010;uf;xx;1;2026-11-03")]
    [InlineData("2010;sexo;outro;1;2026-11-03")]
    [InlineData("2010;idade;0-4;1;2026-11-03")]
    public void LerGabarito_LinhaDesconhecida_RetornaFalha(string linha)
    {
        var resultado = LeitorGabaritoTabnet.Ler(["ano;dimensao;categoria;obitos;consulta", "2010;total;total;3;2026-11-03", linha]);

        Assert.Equal("gabarito.linha_invalida", resultado.Erro!.Codigo);
    }

    [Fact]
    public void Divergencias_CuboIgualAoTabnet_RetornaVazio()
    {
        var cubo = Cubo(Obito(35, Sexo.Masculino), Obito(35, Sexo.Feminino), Obito(33, Sexo.Masculino));

        Assert.Empty(ConferenciaTabnet.Divergencias(2010, cubo, Gabarito));
    }

    [Fact]
    public void Divergencias_UmObitoAMenos_ApontaTotalUfESexo()
    {
        var cubo = Cubo(Obito(35, Sexo.Masculino), Obito(35, Sexo.Feminino));

        var divergencias = ConferenciaTabnet.Divergencias(2010, cubo, Gabarito);

        Assert.Equal(3, divergencias.Count);
        Assert.Contains(divergencias, d => d.Contains("uf 33"));
    }

    [Fact]
    public void Divergencias_AnoForaDoGabarito_ApontaAusencia()
    {
        Assert.Single(ConferenciaTabnet.Divergencias(2024, Cubo(), Gabarito));
    }

    [Fact]
    public void Agregador_CausaSemGrupo_ContaFalhaENaoGeraCelula()
    {
        var agregador = new AgregadorCubo(Mapa, Ano.Criar(2010).Valor);
        agregador.Adicionar(Result<RegistroObito>.Sucesso(Obito(35, Sexo.Masculino, "Z999")));
        agregador.Adicionar(Result<RegistroObito>.Sucesso(Obito(35, Sexo.Masculino)));
        agregador.Adicionar(Result<RegistroObito>.Sucesso(Obito(35, Sexo.Masculino)));

        Assert.Equal(1, agregador.Falhas["causa.sem_grupo"]);
        Assert.Equal(2, Assert.Single(agregador.Celulas()).Obitos);
    }
}
