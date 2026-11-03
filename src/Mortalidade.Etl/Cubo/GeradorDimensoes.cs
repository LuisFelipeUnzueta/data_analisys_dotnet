using System.Text.Encodings.Web;
using System.Text.Json;
using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Cubo;

/// <summary>
/// Gera <c>dimensoes.json</c>: os códigos usados nos Parquet, os pesos do padrão, a proveniência de cada ano publicado
/// e os marcos metodológicos. É a única definição das dimensões que o front consome.
/// </summary>
public static class GeradorDimensoes
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    private static readonly Dictionary<Regiao, string> NomesRegioes = new()
    {
        [Regiao.Norte] = "Norte",
        [Regiao.Nordeste] = "Nordeste",
        [Regiao.Sudeste] = "Sudeste",
        [Regiao.Sul] = "Sul",
        [Regiao.CentroOeste] = "Centro-Oeste"
    };

    public static string Gerar(
        MapaCausas mapa, PopulacaoPadrao padrao, IEnumerable<int> anosPublicados,
        IReadOnlyDictionary<int, FontePopulacao> fontePopulacao, DateTimeOffset geradoEm) =>
        JsonSerializer.Serialize(new
        {
            GeradoEm = geradoEm.ToString("O"),
            Anos = anosPublicados.Order().Select(ano => new
            {
                Ano = ano,
                RevisaoCid = (int)RevisaoCidPorAno.De(Ano.Criar(ano).Valor),
                FontePopulacao = fontePopulacao[ano].ToString().ToLowerInvariant(),
                PopulacaoEstimada = fontePopulacao[ano] is FontePopulacao.Interpolada or FontePopulacao.Extrapolada
            }),
            Regioes = NomesRegioes.Select(r => new { Codigo = (int)r.Key, Nome = r.Value }),
            Ufs = Uf.Todas.OrderBy(u => u.CodigoIbge).Select(u => new { Codigo = u.CodigoIbge, u.Sigla, Regiao = (int)u.Regiao }),
            Grupos = mapa.Grupos.Select(g => new { g.Indice, g.Codigo, g.Nome, g.Capitulo, g.ComparabilidadeReduzida }),
            Faixas = FaixaEtaria.Todas.Select(f => new { f.Indice, f.Rotulo, f.Inicio, f.Fim }),
            Sexos = Enum.GetValues<Sexo>().Select(s => new { Codigo = (int)s, Nome = SexoNome.De(s) }),
            Padrao = new
            {
                Nome = "OMS 2000–2025 (SEER, por milhão)",
                Pesos = padrao.Pesos.OrderBy(p => p.Key.Indice).Select(p => new { Faixa = p.Key.Indice, Peso = p.Value })
            },
            Marcos = new[]
            {
                new { Ano = RevisaoCidPorAno.PrimeiroAnoCid10, Descricao = "Troca da CID-9 pela CID-10" },
                new { Ano = EstimativaPopulacional.AnoCriacaoTocantins, Descricao = "Tocantins passa a ser contado separado de Goiás" }
            }
        }, Json);
}
