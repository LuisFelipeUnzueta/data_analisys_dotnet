using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Leitura;

/// <summary>Lê <c>data/cause_map/grupos.csv</c> (codigo;nome;capitulo;resto;cid9;cid10;comparabilidade).</summary>
public static class LeitorMapaCausas
{
    private static readonly string[] Colunas = ["codigo", "nome", "capitulo", "resto", "cid9", "cid10", "comparabilidade"];

    public static Result<MapaCausas> Ler(IEnumerable<string> linhasCsv)
    {
        using var linhas = linhasCsv.GetEnumerator();
        if (!linhas.MoveNext() || !linhas.Current.Trim().Split(';').SequenceEqual(Colunas))
            return Result<MapaCausas>.Falha("mapa.cabecalho_invalido", $"Cabeçalho esperado: {string.Join(';', Colunas)}.");

        var definicoes = new List<DefinicaoGrupo>();
        var numero = 1;
        while (linhas.MoveNext())
        {
            numero++;
            if (string.IsNullOrWhiteSpace(linhas.Current))
                continue;

            var c = linhas.Current.Split(';');
            if (c.Length != Colunas.Length || c[3] is not ("0" or "1") || c[6] is not ("alta" or "reduzida"))
                return Result<MapaCausas>.Falha("mapa.linha_invalida", $"Linha {numero} inválida: {linhas.Current}");

            definicoes.Add(new DefinicaoGrupo(c[0], c[1], c[2], c[3] == "1", c[6] == "reduzida", Faixas(c[4]), Faixas(c[5])));
        }

        return MapaCausas.Criar(definicoes);
    }

    private static string[] Faixas(string lista) =>
        lista.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
