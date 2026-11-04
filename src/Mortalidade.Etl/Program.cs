using System.Collections.Concurrent;
using System.Diagnostics;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Pipeline;

// Uso:
//   dotnet run --project src/Mortalidade.Etl -- [dir_csv] [dir_saida]   ETL completo dos anos do gabarito do Tabnet
//   dotnet run --project src/Mortalidade.Etl -- fixtures [dir_csv]      regera as fixtures dos testes de harmonização
//   dotnet run --project src/Mortalidade.Etl -- perfil [dir_csv] [json] mede o tempo de cada etapa (README, Desempenho)
// Saída: 0 = todos os anos publicados; 2 = parte reprovada na conferência (não publicada); 1 = erro de entrada.
var entradas = Entradas.Carregar("data");
if (entradas.IsFailure)
{
    Console.Error.WriteLine($"Entrada inválida: {entradas.Erro!.Codigo} {entradas.Erro.Mensagem}");
    return 1;
}

if (args.ElementAtOrDefault(0) == "fixtures")
{
    await GeradorFixtures.GerarAsync(args.ElementAtOrDefault(1) ?? "data/raw", entradas.Valor, "tests/Mortalidade.Etl.Tests/Fixtures");
    return 0;
}

if (args.ElementAtOrDefault(0) == "perfil")
{
    await PerfilDesempenho.ExecutarAsync(args.ElementAtOrDefault(1) ?? "data/raw", entradas.Valor, args.ElementAtOrDefault(2) ?? "out/perfil.json");
    return 0;
}

var dirCsv = args.ElementAtOrDefault(0) ?? "data/raw";
var dirSaida = args.ElementAtOrDefault(1) ?? "out";
var e = entradas.Valor;
var relogio = Stopwatch.StartNew();

var processador = new ProcessadorAno(e.Mapa, e.Gabarito);
var resultados = new ConcurrentBag<ResultadoAno>();
Parallel.ForEach(Enumerable.Range(e.PrimeiroAno, e.UltimoAno - e.PrimeiroAno + 1), anoNumero =>
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

var populacaoPorAno = e.Populacao.ToLookup(p => p.Ano.Valor);
var taxasBrasil = new Dictionary<int, (double? Bruta, double? Padronizada)>();
foreach (var resultado in resultados.OrderBy(r => r.Ano))
{
    var taxas = taxasBrasil[resultado.Ano] = Taxas.DoRecorte(resultado.Celulas, populacaoPorAno[resultado.Ano], e.Padrao);
    Console.WriteLine($"{resultado.Ano} {(resultado.Aprovado ? "OK " : "DIF")} óbitos={resultado.Celulas.Sum(c => c.Obitos)} " +
        $"células={resultado.Celulas.Count} taxa_bruta={taxas.Bruta:F1} taxa_padronizada={taxas.Padronizada:F1}");
    foreach (var problema in resultado.Problemas.Take(5))
        Console.WriteLine($"     {problema}");
}

// Só anos que conferem com o Tabnet são publicados; um ano reprovado não derruba os demais.
var aprovados = resultados.Where(r => r.Aprovado).ToList();
if (aprovados.Count == 0)
{
    Console.Error.WriteLine("Nenhum ano conferiu com o Tabnet; nada foi gravado.");
    return 1;
}

var relogioGravacao = Stopwatch.StartNew();
await PublicacaoSaida.GravarAsync(dirSaida, e, aprovados, taxasBrasil);
Console.WriteLine($"Gravação: {relogioGravacao.Elapsed.TotalSeconds:F1} s.");
foreach (var arquivo in new DirectoryInfo(dirSaida).GetFiles())
    Console.WriteLine($"{arquivo.Name}: {arquivo.Length / 1024.0 / 1024.0:F2} MB");
Console.WriteLine($"{aprovados.Count}/{resultados.Count} anos conferem com o Tabnet e foram publicados em {relogio.Elapsed:mm\\:ss}.");
return aprovados.Count == resultados.Count ? 0 : 2;
