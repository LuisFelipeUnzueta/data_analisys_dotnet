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

    /// <param name="taxasBrasil">
    /// Taxas nacionais de cada ano publicado (as chaves são os anos publicados). O front confere o próprio cálculo
    /// contra elas, então as duas implementações das taxas (C# e TypeScript) não divergem em silêncio.
    /// </param>
    public static string Gerar(
        MapaCausas mapa, PopulacaoPadrao padrao, IReadOnlyDictionary<int, (double? Bruta, double? Padronizada)> taxasBrasil,
        IReadOnlyDictionary<int, FontePopulacao> fontePopulacao, DateTimeOffset geradoEm) =>
        JsonSerializer.Serialize(new
        {
            GeradoEm = geradoEm.ToString("O"),
            Anos = taxasBrasil.OrderBy(t => t.Key).Select(t => new
            {
                Ano = t.Key,
                RevisaoCid = (int)RevisaoCidPorAno.De(Ano.Criar(t.Key).Valor),
                FontePopulacao = fontePopulacao[t.Key].ToString().ToLowerInvariant(),
                PopulacaoEstimada = fontePopulacao[t.Key] is FontePopulacao.Interpolada or FontePopulacao.Extrapolada,
                TaxaBrutaBrasil = Arredondar(t.Value.Bruta),
                TaxaPadronizadaBrasil = Arredondar(t.Value.Padronizada)
            }),
            Regioes = Enum.GetValues<Regiao>().Select(r => new { Codigo = (int)r, Nome = RegiaoNome.De(r) }),
            Ufs = Uf.Todas.OrderBy(u => u.CodigoIbge).Select(u => new { Codigo = u.CodigoIbge, u.Sigla, Regiao = (int)u.Regiao }),
            Grupos = mapa.Grupos.Select(g => new { g.Indice, g.Codigo, g.Nome, g.Capitulo, g.ComparabilidadeReduzida }),
            Faixas = FaixaEtaria.Todas.Select(f => new { f.Indice, f.Rotulo, f.Inicio, f.Fim }),
            Sexos = Enum.GetValues<Sexo>().Select(s => new { Codigo = (int)s, Nome = SexoNome.De(s) }),
            Padrao = new
            {
                Nome = "OMS 2000–2025 (SEER, por milhão)",
                Pesos = padrao.Pesos.OrderBy(p => p.Key.Indice).Select(p => new { Faixa = p.Key.Indice, Peso = p.Value })
            },
            // Marcos metodológicos: o front desenha a linha vertical e o aviso a partir daqui (ufs vazio = vale para todas).
            Marcos = new[]
            {
                new { Ano = RevisaoCidPorAno.PrimeiroAnoCid10, Tipo = "cid", Rotulo = "CID-9 → CID-10", Descricao = "Troca da CID-9 pela CID-10", Ufs = Array.Empty<int>() },
                new { Ano = Uf.AnoCriacaoTocantins, Tipo = "territorio", Rotulo = "TO separado de GO", Descricao = "Tocantins passa a ser contado separado de Goiás",
                      Ufs = new[] { Uf.Tocantins.CodigoIbge, Uf.Goias.CodigoIbge } }
            }
        }, Json);

    private static double? Arredondar(double? valor) => valor is null ? null : Math.Round(valor.Value, 3);
}
