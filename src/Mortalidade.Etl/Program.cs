using System.Collections.Concurrent;
using System.Diagnostics;
using Mortalidade.Etl.Cubo;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;
using Mortalidade.Etl.Validacao;

// Uso: dotnet run --project src/Mortalidade.Etl -- [dir_csv] [primeiro_ano] [ultimo_ano]
var dirCsv = args.ElementAtOrDefault(0) ?? "data/raw";
var primeiro = int.Parse(args.ElementAtOrDefault(1) ?? "1979");
var ultimo = int.Parse(args.ElementAtOrDefault(2) ?? "2023");

var mapa = MapaCausas.Carregar(File.ReadLines("data/cause_map/grupos.csv"));
if (mapa.IsFailure)
{
    Console.Error.WriteLine($"Mapa de causas inválido: {mapa.Erro!.Mensagem}");
    return 1;
}

var gabarito = ConferenciaTabnet.LerGabarito(File.ReadLines("data/tabnet/gabarito_1979_2023.csv"));
var resultados = new ConcurrentDictionary<int, (long Obitos, int Celulas, IReadOnlyList<string> Problemas)>();
var relogio = Stopwatch.StartNew();

Parallel.ForEach(Enumerable.Range(primeiro, ultimo - primeiro + 1), anoNumero =>
{
    var ano = Ano.Criar(anoNumero).Valor;
    var caminho = Path.Combine(dirCsv, $"Mortalidade_Geral_{anoNumero}.csv");
    if (!File.Exists(caminho))
    {
        resultados[anoNumero] = (0, 0, [$"{anoNumero}: arquivo ausente ({caminho})."]);
        return;
    }

    var agregador = new AgregadorCubo(mapa.Valor);
    using (var arquivo = File.OpenRead(caminho))
    {
        foreach (var registro in LeitorSimCsv.Ler(arquivo, ano))
            agregador.Adicionar(registro);
    }

    var celulas = agregador.Celulas();
    var problemas = agregador.Falhas.Select(f => $"{anoNumero}: {f.Value} registro(s) com {f.Key}")
        .Concat(agregador.ExemplosFalha.Take(3))
        .Concat(ConferenciaTabnet.Divergencias(anoNumero, celulas, gabarito))
        .ToList();
    resultados[anoNumero] = (celulas.Sum(c => c.Obitos), celulas.Count, problemas);
});

foreach (var (ano, (obitos, celulas, problemas)) in resultados.OrderBy(r => r.Key))
{
    Console.WriteLine($"{ano} {(problemas.Count == 0 ? "OK " : "DIF")} óbitos={obitos} células={celulas}");
    foreach (var problema in problemas.Take(5))
        Console.WriteLine($"     {problema}");
}

var aprovados = resultados.Count(r => r.Value.Problemas.Count == 0);
Console.WriteLine($"{aprovados}/{resultados.Count} anos conferem com o Tabnet em {relogio.Elapsed:mm\\:ss}.");
return aprovados == resultados.Count ? 0 : 1;
