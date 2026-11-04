using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;
using Mortalidade.Etl.Validacao;

namespace Mortalidade.Etl.Pipeline;

/// <summary>
/// Os arquivos versionados em <c>data/</c> já lidos e validados. Os anos processados são os do gabarito do Tabnet
/// (só anos finais), e a série de população vai até o último deles.
/// </summary>
public sealed record Entradas(
    IReadOnlyDictionary<int, TotaisTabnet> Gabarito,
    MapaCausas Mapa,
    PopulacaoPadrao Padrao,
    IReadOnlyList<Populacao> Populacao,
    IReadOnlyDictionary<int, FontePopulacao> FontePopulacao)
{
    public int PrimeiroAno => Gabarito.Keys.Min();

    public int UltimoAno => Gabarito.Keys.Max();

    public static Result<Entradas> Carregar(string raiz) =>
        LeitorGabaritoTabnet.Ler(Linhas(raiz, "tabnet/gabarito.csv")).Bind(gabarito =>
        LeitorMapaCausas.Ler(Linhas(raiz, "cause_map/grupos.csv")).Bind(mapa =>
        LeitorPopulacaoPadrao.Ler(Linhas(raiz, "padrao/oms_2000_2025.csv")).Bind(padrao =>
        LeitorPopulacaoFontes.Ler(Linhas(raiz, "ibge/populacao_fontes.csv"))
            .Bind(pontos => EstimativaPopulacional.Estimar(pontos, gabarito.Keys.Max()))
            .Map(populacao => new Entradas(gabarito, mapa, padrao, populacao.Populacao, populacao.FontePorAno)))));

    private static IEnumerable<string> Linhas(string raiz, string arquivo) => File.ReadLines(Path.Combine(raiz, arquivo));
}
