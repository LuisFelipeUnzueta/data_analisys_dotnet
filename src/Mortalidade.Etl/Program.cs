using System.Collections.Concurrent;
using System.Diagnostics;
using Mortalidade.Etl.Cubo;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;
using Mortalidade.Etl.Pipeline;
using Mortalidade.Etl.Validacao;

// Uso: dotnet run --project src/Mortalidade.Etl -- [dir_csv] [dir_saida]
// Os anos processados são os do gabarito do Tabnet (só anos finais; ver scripts/gerar_gabarito.py).
// Saída: 0 = todos os anos publicados; 2 = parte reprovada na conferência (não publicada); 1 = erro de entrada.
var dirCsv = args.ElementAtOrDefault(0) ?? "data/raw";
var dirSaida = args.ElementAtOrDefault(1) ?? "out";

var gabarito = ConferenciaTabnet.LerGabarito(File.ReadLines("data/tabnet/gabarito.csv"));
var primeiro = gabarito.Min(g => g.Ano);
var ultimo = gabarito.Max(g => g.Ano);

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
var processador = new ProcessadorAno(mapa.Valor, gabarito);
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

var taxasBrasil = new Dictionary<int, (double? Bruta, double? Padronizada)>();
foreach (var resultado in resultados.OrderBy(r => r.Ano))
{
    var (bruta, padronizada) = Taxas.DoRecorte(resultado.Celulas, populacaoPorAno[resultado.Ano], padrao.Valor);
    if (resultado.Aprovado)
        taxasBrasil[resultado.Ano] = (bruta, padronizada);
    Console.WriteLine($"{resultado.Ano} {(resultado.Aprovado ? "OK " : "DIF")} óbitos={resultado.Celulas.Sum(c => c.Obitos)} " +
        $"células={resultado.Celulas.Count} taxa_bruta={bruta:F1} taxa_padronizada={padronizada:F1}");
    foreach (var problema in resultado.Problemas.Take(5))
        Console.WriteLine($"     {problema}");
}

// Só anos que conferem com o Tabnet são publicados; um ano reprovado não derruba os demais.
var publicados = resultados.Where(r => r.Aprovado).OrderBy(r => r.Ano).ToList();
if (publicados.Count == 0)
{
    Console.Error.WriteLine("Nenhum ano conferiu com o Tabnet; nada foi gravado.");
    return 1;
}

var anosPublicados = publicados.Select(r => r.Ano).ToHashSet();
Directory.CreateDirectory(dirSaida);
await using (var arquivo = File.Create(Path.Combine(dirSaida, "obitos.parquet")))
    await EscritorParquet.GravarObitosAsync(arquivo, publicados.Select(r => r.Celulas));
await using (var arquivo = File.Create(Path.Combine(dirSaida, "populacao.parquet")))
    await EscritorParquet.GravarPopulacaoAsync(arquivo, estimativa.Valor.Populacao.Where(p => anosPublicados.Contains(p.Ano.Valor)).ToList());
await File.WriteAllTextAsync(Path.Combine(dirSaida, "dimensoes.json"),
    GeradorDimensoes.Gerar(mapa.Valor, padrao.Valor, taxasBrasil, estimativa.Valor.FontePorAno, DateTimeOffset.UtcNow));

foreach (var arquivo in new DirectoryInfo(dirSaida).GetFiles())
    Console.WriteLine($"{arquivo.Name}: {arquivo.Length / 1024.0 / 1024.0:F2} MB");
Console.WriteLine($"{publicados.Count}/{resultados.Count} anos conferem com o Tabnet e foram publicados em {relogio.Elapsed:mm\\:ss}.");
return publicados.Count == resultados.Count ? 0 : 2;
