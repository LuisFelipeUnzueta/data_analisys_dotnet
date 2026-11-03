using System.Collections.Concurrent;
using System.Diagnostics;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;
using Mortalidade.Etl.Pipeline;
using Mortalidade.Etl.Validacao;

// Uso: dotnet run --project src/Mortalidade.Etl -- [dir_csv] [primeiro_ano] [ultimo_ano]
var dirCsv = args.ElementAtOrDefault(0) ?? "data/raw";
var primeiro = int.Parse(args.ElementAtOrDefault(1) ?? "1979");
var ultimo = int.Parse(args.ElementAtOrDefault(2) ?? "2023");

var mapa = MapaCausas.Carregar(File.ReadLines("data/cause_map/grupos.csv"));
var padrao = PopulacaoPadrao.Carregar(File.ReadLines("data/padrao/oms_2000_2025.csv"));
var pontos = LeitorPopulacaoFontes.Ler(File.ReadLines("data/ibge/populacao_fontes.csv"));
var estimativa = pontos.IsSuccess ? EstimativaPopulacional.Estimar(pontos.Valor, ultimo) : null;
var falhaEntrada = new[] { mapa.Erro, padrao.Erro, pontos.Erro, estimativa?.Erro }.FirstOrDefault(e => e is not null);
if (falhaEntrada is not null)
{
    Console.Error.WriteLine($"Entrada inválida: {falhaEntrada.Codigo} {falhaEntrada.Mensagem}");
    return 1;
}

var populacaoPorAno = estimativa!.Valor.Populacao.ToLookup(p => p.Ano.Valor);
var processador = new ProcessadorAno(mapa.Valor, ConferenciaTabnet.LerGabarito(File.ReadLines("data/tabnet/gabarito_1979_2023.csv")));
var resultados = new ConcurrentBag<ResultadoAno>();
var relogio = Stopwatch.StartNew();

Parallel.ForEach(Enumerable.Range(primeiro, ultimo - primeiro + 1), anoNumero =>
{
    var caminho = Path.Combine(dirCsv, $"Mortalidade_Geral_{anoNumero}.csv");
    if (!File.Exists(caminho))
    {
        resultados.Add(new ResultadoAno(anoNumero, [], [$"{anoNumero}: arquivo ausente ({caminho})."]));
        return;
    }

    using var arquivo = File.OpenRead(caminho);
    resultados.Add(processador.Processar(Ano.Criar(anoNumero).Valor, arquivo));
});

foreach (var resultado in resultados.OrderBy(r => r.Ano))
{
    var (bruta, padronizada) = Taxas.DoRecorte(resultado.Celulas, populacaoPorAno[resultado.Ano], padrao.Valor);
    Console.WriteLine($"{resultado.Ano} {(resultado.Aprovado ? "OK " : "DIF")} óbitos={resultado.Celulas.Sum(c => c.Obitos)} " +
        $"células={resultado.Celulas.Count} taxa_bruta={bruta:F1} taxa_padronizada={padronizada:F1}");
    foreach (var problema in resultado.Problemas.Take(5))
        Console.WriteLine($"     {problema}");
}

var aprovados = resultados.Count(r => r.Aprovado);
Console.WriteLine($"{aprovados}/{resultados.Count} anos conferem com o Tabnet em {relogio.Elapsed:mm\\:ss}.");
return aprovados == resultados.Count ? 0 : 1;
