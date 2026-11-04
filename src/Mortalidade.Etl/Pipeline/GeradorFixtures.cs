using System.Collections.Concurrent;
using System.Text;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;

namespace Mortalidade.Etl.Pipeline;

/// <summary>
/// Regera as fixtures dos testes de harmonização a partir dos CSV do SIM: códigos observados em todos os anos,
/// óbitos por código em 1992–1999 e o golden de totais por grupo nesses anos. Rode depois de mudar <c>grupos.csv</c>
/// e revise o diff do golden antes de commitar.
/// </summary>
public static class GeradorFixtures
{
    private static readonly (int Inicio, int Fim) JanelaContinuidade = (1992, 1999);

    public static async Task GerarAsync(string dirCsv, Entradas entradas, string destino)
    {
        var porAno = new ConcurrentDictionary<int, Dictionary<CausaBasica, long>>();
        Parallel.ForEach(Enumerable.Range(entradas.PrimeiroAno, entradas.UltimoAno - entradas.PrimeiroAno + 1), ano =>
        {
            var caminho = Path.Combine(dirCsv, $"Mortalidade_Geral_{ano}.csv");
            if (!File.Exists(caminho))
                return;

            var contagem = new Dictionary<CausaBasica, long>();
            using var arquivo = File.OpenRead(caminho);
            foreach (var registro in LeitorSimCsv.Ler(arquivo, Ano.Criar(ano).Valor).Where(r => r.IsSuccess))
                contagem[registro.Valor.Causa] = contagem.GetValueOrDefault(registro.Valor.Causa) + 1;

            // Só anos publicáveis: um CSV que não bate com o total do Tabnet (ex.: extração preliminar) fica de fora.
            if (contagem.Values.Sum() == entradas.Gabarito[ano].Total)
                porAno[ano] = contagem;
            else
                Console.WriteLine($"{ano}: total diferente do Tabnet, fora das fixtures.");
        });

        var janela = porAno.Where(a => a.Key >= JanelaContinuidade.Inicio && a.Key <= JanelaContinuidade.Fim)
            .OrderBy(a => a.Key).ToList();

        await Gravar(destino, "codigos_observados.csv", "revisao;codigo",
            porAno.Values.SelectMany(c => c.Keys).Distinct()
                .OrderBy(c => (int)c.Revisao).ThenBy(c => c.Codigo, StringComparer.Ordinal)
                .Select(c => $"{(int)c.Revisao};{c.Codigo}"));

        await Gravar(destino, $"obitos_por_codigo_{JanelaContinuidade.Inicio}_{JanelaContinuidade.Fim}.csv", "ano;codigo;obitos",
            janela.SelectMany(a => a.Value.OrderBy(c => c.Key.Codigo, StringComparer.Ordinal)
                .Select(c => $"{a.Key};{c.Key.Codigo};{c.Value}")));

        await Gravar(destino, $"golden_grupos_{JanelaContinuidade.Inicio}_{JanelaContinuidade.Fim}.csv", "ano;grupo;obitos",
            janela.SelectMany(a => a.Value
                .GroupBy(c => entradas.Mapa.Resolver(c.Key).Valor.Codigo)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => $"{a.Key};{g.Key};{g.Sum(c => c.Value)}")));

        Console.WriteLine($"Fixtures regeradas em {destino} a partir de {porAno.Count} anos.");
    }

    private static Task Gravar(string destino, string arquivo, string cabecalho, IEnumerable<string> linhas) =>
        File.WriteAllTextAsync(Path.Combine(destino, arquivo), string.Join('\n', linhas.Prepend(cabecalho)) + "\n", new UTF8Encoding(false));
}
