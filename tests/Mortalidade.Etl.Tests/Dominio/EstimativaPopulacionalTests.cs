using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class EstimativaPopulacionalTests
{
    private static readonly Uf Sp = Uf.DeCodigoIbge(35).Valor;
    private static readonly Uf Go = Uf.DeCodigoIbge(52).Valor;
    private static readonly Uf To = Uf.DeCodigoIbge(17).Valor;
    private static readonly FaixaEtaria Jovens = FaixaEtaria.DeIdadeEmAnos(20).Valor;
    private static readonly FaixaEtaria Idosos = FaixaEtaria.DeIdadeEmAnos(80).Valor;

    private static PontoPopulacao Ponto(int ano, Uf uf, FaixaEtaria faixa, long habitantes, FontePopulacao fonte = FontePopulacao.Censo) =>
        new(ano, fonte, uf, Sexo.Feminino, faixa, habitantes);

    private static IReadOnlyList<Populacao> Estimar(params PontoPopulacao[] pontos) =>
        EstimativaPopulacional.Estimar(pontos, 2000).Valor.Populacao;

    private static long Habitantes(IEnumerable<Populacao> serie, int ano, Uf uf, FaixaEtaria faixa) =>
        serie.Single(p => p.Ano.Valor == ano && p.Uf == uf && p.FaixaEtaria == faixa).Habitantes;

    [Fact]
    public void Estimar_InterpolaGeometricamenteEntreAncoras()
    {
        var serie = Estimar(Ponto(1980, Sp, Jovens, 1000), Ponto(2000, Sp, Jovens, 4000, FontePopulacao.Ripsa2024));

        Assert.Equal(1000, Habitantes(serie, 1980, Sp, Jovens));
        Assert.Equal(2000, Habitantes(serie, 1990, Sp, Jovens));
        Assert.Equal(4000, Habitantes(serie, 2000, Sp, Jovens));
    }

    [Fact]
    public void Estimar_ExtrapolaUmAnoParaTrasCom1980a1981()
    {
        // Crescimento anual de ~7,18% (dobra em 10 anos) → 1979 = 1000 / 1,0718.
        var serie = Estimar(Ponto(1980, Sp, Jovens, 1000), Ponto(1990, Sp, Jovens, 2000), Ponto(2000, Sp, Jovens, 4000));

        Assert.Equal(933, Habitantes(serie, 1979, Sp, Jovens));
    }

    [Fact]
    public void Estimar_RedistribuiIdadeIgnoradaNaProporcaoDasConhecidas()
    {
        var serie = Estimar(
            Ponto(1980, Sp, Jovens, 300), Ponto(1980, Sp, Idosos, 100), Ponto(1980, Sp, FaixaEtaria.Ignorada, 40),
            Ponto(2000, Sp, Jovens, 300), Ponto(2000, Sp, Idosos, 100));

        Assert.Equal(330, Habitantes(serie, 1980, Sp, Jovens));
        Assert.Equal(110, Habitantes(serie, 1980, Sp, Idosos));
        Assert.DoesNotContain(serie, p => p.FaixaEtaria == FaixaEtaria.Ignorada);
    }

    [Fact]
    public void Estimar_TocantinsEntraEmGoiasAntesDe1989()
    {
        var serie = Estimar(
            Ponto(1980, Go, Jovens, 900), Ponto(1980, To, Jovens, 100),
            Ponto(2000, Go, Jovens, 900), Ponto(2000, To, Jovens, 100));

        Assert.Equal(1000, Habitantes(serie, 1988, Go, Jovens));
        Assert.DoesNotContain(serie, p => p.Ano.Valor == 1988 && p.Uf == To);
        Assert.Equal(900, Habitantes(serie, 1989, Go, Jovens));
        Assert.Equal(100, Habitantes(serie, 1989, To, Jovens));
    }

    [Fact]
    public void Estimar_AncoraSemTocantins_InterpolaGoiasHistoricoESeparaTocantinsPelaProporcaoPosterior()
    {
        // Censo 1980 tem o território de TO dentro de GO; 1991 já separa (10% em TO).
        var serie = Estimar(
            Ponto(1980, Go, Jovens, 1000), Ponto(1980, To, Jovens, 0),
            Ponto(1991, Go, Jovens, 900), Ponto(1991, To, Jovens, 100),
            Ponto(2000, Go, Jovens, 900), Ponto(2000, To, Jovens, 100));

        Assert.Equal(1000, Habitantes(serie, 1985, Go, Jovens));
        Assert.Equal(900, Habitantes(serie, 1989, Go, Jovens));
        Assert.Equal(100, Habitantes(serie, 1989, To, Jovens));
    }

    [Fact]
    public void Estimar_InformaAFonteDeCadaAno()
    {
        var fontes = EstimativaPopulacional.Estimar(
            [Ponto(1980, Sp, Jovens, 1000), Ponto(1996, Sp, Jovens, 1500, FontePopulacao.Contagem), Ponto(2000, Sp, Jovens, 2000, FontePopulacao.Ripsa2024)],
            2000).Valor.FontePorAno;

        Assert.Equal(FontePopulacao.Extrapolada, fontes[1979]);
        Assert.Equal(FontePopulacao.Censo, fontes[1980]);
        Assert.Equal(FontePopulacao.Interpolada, fontes[1990]);
        Assert.Equal(FontePopulacao.Contagem, fontes[1996]);
        Assert.Equal(FontePopulacao.Ripsa2024, fontes[2000]);
    }

    [Fact]
    public void Estimar_SemAncoraFinal_RetornaFalha()
    {
        var resultado = EstimativaPopulacional.Estimar([Ponto(1980, Sp, Jovens, 1000)], 2001);

        Assert.Equal("populacao.ancora_ausente", resultado.Erro!.Codigo);
    }
}
