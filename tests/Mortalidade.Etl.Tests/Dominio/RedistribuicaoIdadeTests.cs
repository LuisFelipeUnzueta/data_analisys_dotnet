using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class RedistribuicaoIdadeTests
{
    private static readonly Ano Ano2010 = Ano.Criar(2010).Valor;
    private static readonly Uf Sp = Uf.DeCodigoIbge(35).Valor;
    private static readonly GrupoCausa Isquemicas = new(0, "isquemicas", "Isquêmicas", "circ", false);
    private static readonly GrupoCausa Homicidio = new(1, "homicidio", "Homicídio", "ext", false);
    private static readonly FaixaEtaria Adultos = FaixaEtaria.DeIdadeEmAnos(40).Valor;
    private static readonly FaixaEtaria Idosos = FaixaEtaria.DeIdadeEmAnos(80).Valor;

    private static CelulaCubo Celula(GrupoCausa grupo, FaixaEtaria faixa, long obitos, Sexo sexo = Sexo.Masculino) =>
        new(Ano2010, Sp, grupo, sexo, faixa, obitos);

    private static double Ajustado(IEnumerable<CelulaCubo> celulas, GrupoCausa grupo, FaixaEtaria faixa) =>
        celulas.Where(c => c.GrupoCausa == grupo && c.FaixaEtaria == faixa).Sum(c => c.ObitosAjustados);

    [Fact]
    public void Aplicar_RepartirIgnoradosNaProporcaoDoEstrato()
    {
        var celulas = RedistribuicaoIdade.Aplicar([
            Celula(Isquemicas, Adultos, 30), Celula(Isquemicas, Idosos, 70), Celula(Isquemicas, FaixaEtaria.Ignorada, 10)]).Valor;

        Assert.Equal(33, Ajustado(celulas, Isquemicas, Adultos), 6);
        Assert.Equal(77, Ajustado(celulas, Isquemicas, Idosos), 6);
        Assert.Equal(0, Ajustado(celulas, Isquemicas, FaixaEtaria.Ignorada));
        Assert.Equal(110, celulas.Sum(c => c.Obitos));
    }

    [Fact]
    public void Aplicar_EstratoSoComIgnorados_UsaDistribuicaoDaUfESexo()
    {
        var celulas = RedistribuicaoIdade.Aplicar([
            Celula(Isquemicas, Adultos, 20), Celula(Isquemicas, Idosos, 80), Celula(Homicidio, FaixaEtaria.Ignorada, 5)]).Valor;

        Assert.Equal(1, Ajustado(celulas, Homicidio, Adultos), 6);
        Assert.Equal(4, Ajustado(celulas, Homicidio, Idosos), 6);
        Assert.Equal(105, celulas.Sum(c => c.ObitosAjustados), 6);
    }

    [Fact]
    public void Aplicar_SexoSemNenhumaIdadeConhecida_UsaDistribuicaoDaUf()
    {
        var celulas = RedistribuicaoIdade.Aplicar([
            Celula(Isquemicas, Adultos, 50), Celula(Isquemicas, Idosos, 50),
            Celula(Isquemicas, FaixaEtaria.Ignorada, 2, Sexo.Ignorado)]).Valor;

        Assert.Equal(1, celulas.Where(c => c.Sexo == Sexo.Ignorado && c.FaixaEtaria == Adultos).Sum(c => c.ObitosAjustados), 6);
    }

    [Fact]
    public void Aplicar_UfSemNenhumaIdadeConhecida_RetornaFalha()
    {
        var resultado = RedistribuicaoIdade.Aplicar([Celula(Isquemicas, FaixaEtaria.Ignorada, 3)]);

        Assert.Equal("idade.sem_distribuicao", resultado.Erro!.Codigo);
    }
}
