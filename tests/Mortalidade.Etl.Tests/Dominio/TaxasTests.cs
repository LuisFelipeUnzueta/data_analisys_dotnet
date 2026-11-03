using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Tests.Dominio;

public class TaxasTests
{
    private static readonly PopulacaoPadrao Oms =
        PopulacaoPadrao.Carregar(File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Dados", "oms_2000_2025.csv"))).Valor;

    private static IEnumerable<FaixaEtaria> FaixasConhecidas => FaixaEtaria.Todas.Where(f => f != FaixaEtaria.Ignorada);

    [Fact]
    public void Oms_SomaUmMilhaoECobreAs17Faixas()
    {
        Assert.Equal(17, Oms.Pesos.Count);
        Assert.Equal(1_000_000, Oms.Pesos.Values.Sum());
        Assert.Equal(15_445, Oms.Pesos[FaixaEtaria.DeIdadeEmAnos(80).Valor]);
    }

    [Fact]
    public void Bruta_CalculaPorCemMil()
    {
        Assert.Equal(649.3, Taxas.Bruta(1_349_801, 207_900_000)!.Value, 1);
        Assert.Null(Taxas.Bruta(10, 0));
    }

    [Fact]
    public void Padronizada_PopulacaoIgualAoPadrao_IgualATaxaBruta()
    {
        // Mesma estrutura etária do padrão: a padronização não muda a taxa.
        var porFaixa = FaixasConhecidas.ToDictionary(f => f, f =>
            (Obitos: Oms.Pesos[f] * 0.01 * (f.Inicio!.Value + 1), Populacao: (long)Oms.Pesos[f]));
        var bruta = Taxas.Bruta(porFaixa.Values.Sum(v => v.Obitos), porFaixa.Values.Sum(v => v.Populacao))!.Value;

        Assert.Equal(bruta, Taxas.Padronizada(porFaixa, Oms)!.Value, 6);
    }

    [Fact]
    public void Padronizada_TaxaConstantePorFaixa_IgualAEssaTaxa()
    {
        var porFaixa = FaixasConhecidas.ToDictionary(f => f, f => (Obitos: 5.0 * (f.Inicio!.Value + 1), Populacao: 1000L * (f.Inicio!.Value + 1)));

        Assert.Equal(500, Taxas.Padronizada(porFaixa, Oms)!.Value, 6);
    }

    [Fact]
    public void Padronizada_FaixaSemPopulacao_NaoEDefinida()
    {
        var porFaixa = FaixasConhecidas.Skip(1).ToDictionary(f => f, _ => (Obitos: 1.0, Populacao: 100L));

        Assert.Null(Taxas.Padronizada(porFaixa, Oms));
    }

    [Theory]
    [InlineData("faixa;peso\n0-4;1")]
    [InlineData("faixa;peso\n0-4;1\n0-4;x")]
    public void Carregar_ArquivoInvalido_RetornaFalha(string conteudo)
    {
        Assert.True(PopulacaoPadrao.Carregar(conteudo.Split('\n')).IsFailure);
    }
}
