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
var gabarito = ConferenciaTabnet.LerGabarito(File.ReadLines("data/tabnet/gabarito_1979_2023.csv"));
var resultados = new ConcurrentDictionary<int, (long Obitos, int Celulas, double? Bruta, double? Padronizada, IReadOnlyList<string> Problemas)>();
var relogio = Stopwatch.StartNew();

Parallel.ForEach(Enumerable.Range(primeiro, ultimo - primeiro + 1), anoNumero =>
{
    var ano = Ano.Criar(anoNumero).Valor;
    var caminho = Path.Combine(dirCsv, $"Mortalidade_Geral_{anoNumero}.csv");
    if (!File.Exists(caminho))
    {
        resultados[anoNumero] = (0, 0, null, null, [$"{anoNumero}: arquivo ausente ({caminho})."]);
        return;
    }

    var agregador = new AgregadorCubo(mapa.Valor);
    using (var arquivo = File.OpenRead(caminho))
    {
        foreach (var registro in LeitorSimCsv.Ler(arquivo, ano))
            agregador.Adicionar(registro);
    }

    var celulas = RedistribuicaoIdade.Aplicar(agregador.Celulas());
    var problemas = agregador.Falhas.Select(f => $"{anoNumero}: {f.Value} registro(s) com {f.Key}")
        .Concat(agregador.ExemplosFalha.Take(3))
        .Concat(celulas.IsFailure ? [celulas.Erro!.Mensagem] : ConferenciaTabnet.Divergencias(anoNumero, celulas.Valor, gabarito))
        .ToList();
    if (celulas.IsFailure)
    {
        resultados[anoNumero] = (0, 0, null, null, problemas);
        return;
    }

    // Taxas nacionais do ano, como verificação de ordem de grandeza.
    var populacao = populacaoPorAno[anoNumero].ToList();
    var porFaixa = populacao.GroupBy(p => p.FaixaEtaria).ToDictionary(g => g.Key, g =>
        (celulas.Valor.Where(c => c.FaixaEtaria == g.Key).Sum(c => c.ObitosAjustados), g.Sum(p => p.Habitantes)));
    var obitos = celulas.Valor.Sum(c => c.Obitos);
    resultados[anoNumero] = (obitos, celulas.Valor.Count, Taxas.Bruta(obitos, populacao.Sum(p => p.Habitantes)),
        Taxas.Padronizada(porFaixa, padrao.Valor), problemas);
});

foreach (var (ano, (obitos, celulas, bruta, padronizada, problemas)) in resultados.OrderBy(r => r.Key))
{
    Console.WriteLine($"{ano} {(problemas.Count == 0 ? "OK " : "DIF")} óbitos={obitos} células={celulas} " +
        $"taxa_bruta={bruta:F1} taxa_padronizada={padronizada:F1}");
    foreach (var problema in problemas.Take(5))
        Console.WriteLine($"     {problema}");
}

var aprovados = resultados.Count(r => r.Value.Problemas.Count == 0);
Console.WriteLine($"{aprovados}/{resultados.Count} anos conferem com o Tabnet em {relogio.Elapsed:mm\\:ss}.");
return aprovados == resultados.Count ? 0 : 1;
