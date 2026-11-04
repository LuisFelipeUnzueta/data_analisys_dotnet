using System.Diagnostics;
using System.Text.Json;
using Mortalidade.Etl.Cubo;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;
using Mortalidade.Etl.Validacao;

namespace Mortalidade.Etl.Pipeline;

/// <summary>
/// Mede o pipeline etapa por etapa (modo <c>perfil</c>): primeiro em um núcleo, separando leitura, agregação,
/// redistribuição e conferência; depois o pipeline completo em paralelo e a gravação dos Parquet. O primeiro passo
/// em paralelo só aquece o cache de disco, para as etapas não medirem a leitura fria do SSD.
/// </summary>
public static class PerfilDesempenho
{
    public static async Task ExecutarAsync(string dirCsv, Entradas entradas, string arquivoSaida)
    {
        var anos = Enumerable.Range(entradas.PrimeiroAno, entradas.UltimoAno - entradas.PrimeiroAno + 1)
            .Where(a => File.Exists(Caminho(dirCsv, a))).ToList();
        var processador = new ProcessadorAno(entradas.Mapa, entradas.Gabarito);

        ResultadoAno[] Paralelo() => anos.AsParallel().Select(ano =>
        {
            using var arquivo = File.OpenRead(Caminho(dirCsv, ano));
            return processador.Processar(Ano.Criar(ano).Valor, arquivo);
        }).ToArray();

        Console.WriteLine("Aquecendo o cache de disco...");
        Paralelo();

        // Etapas em um núcleo. A agregação é medida como (ler + agregar) − ler.
        TimeSpan leitura = default, leituraAgregacao = default, redistribuicao = default, conferencia = default;
        long registros = 0, celulas = 0;
        foreach (var ano in anos)
        {
            var a = Ano.Criar(ano).Valor;
            var relogio = Stopwatch.StartNew();
            using (var arquivo = File.OpenRead(Caminho(dirCsv, ano)))
                foreach (var _ in LeitorSimCsv.Ler(arquivo, a))
                    registros++;
            leitura += relogio.Elapsed;

            relogio.Restart();
            var agregador = new AgregadorCubo(entradas.Mapa, a);
            using (var arquivo = File.OpenRead(Caminho(dirCsv, ano)))
                foreach (var registro in LeitorSimCsv.Ler(arquivo, a))
                    agregador.Adicionar(registro);
            var brutas = agregador.Celulas();
            leituraAgregacao += relogio.Elapsed;

            relogio.Restart();
            var ajustadas = RedistribuicaoIdade.Aplicar(brutas).Valor;
            redistribuicao += relogio.Elapsed;

            relogio.Restart();
            ConferenciaTabnet.Divergencias(ano, ajustadas, entradas.Gabarito);
            conferencia += relogio.Elapsed;
            celulas += ajustadas.Count;
        }

        var relogioParalelo = Stopwatch.StartNew();
        var resultados = Paralelo();
        var paralelo = relogioParalelo.Elapsed;

        var dirTemporario = Directory.CreateTempSubdirectory("mortalidade-perfil").FullName;
        var relogioGravacao = Stopwatch.StartNew();
        var taxas = resultados.ToDictionary(r => r.Ano, _ => ((double?)null, (double?)null));
        await PublicacaoSaida.GravarAsync(dirTemporario, entradas, resultados.Where(r => r.Aprovado).ToList(), taxas);
        var gravacao = relogioGravacao.Elapsed;
        var bytesSaida = new DirectoryInfo(dirTemporario).GetFiles().ToDictionary(f => f.Name, f => f.Length);
        Directory.Delete(dirTemporario, recursive: true);

        var umNucleo = leituraAgregacao + redistribuicao + conferencia;
        var perfil = new
        {
            Maquina = new { Environment.ProcessorCount, Sistema = Environment.OSVersion.ToString(), Runtime = Environment.Version.ToString() },
            Anos = anos.Count,
            Registros = registros,
            Celulas = celulas,
            BytesCsv = anos.Sum(a => new FileInfo(Caminho(dirCsv, a)).Length),
            BytesSaida = bytesSaida,
            UmNucleoSegundos = new
            {
                Leitura = leitura.TotalSeconds,
                Agregacao = (leituraAgregacao - leitura).TotalSeconds,
                Redistribuicao = redistribuicao.TotalSeconds,
                Conferencia = conferencia.TotalSeconds,
                Total = umNucleo.TotalSeconds
            },
            ParaleloSegundos = paralelo.TotalSeconds,
            GravacaoSegundos = gravacao.TotalSeconds,
            RegistrosPorSegundoUmNucleo = registros / leituraAgregacao.TotalSeconds,
            RegistrosPorSegundoParalelo = registros / paralelo.TotalSeconds,
            AceleracaoParalela = umNucleo.TotalSeconds / paralelo.TotalSeconds,
            PicoMemoriaMb = Process.GetCurrentProcess().PeakWorkingSet64 / 1024.0 / 1024.0
        };

        var json = JsonSerializer.Serialize(perfil, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(arquivoSaida, json);
        Console.WriteLine(json);
    }

    private static string Caminho(string dirCsv, int ano) => Path.Combine(dirCsv, $"Mortalidade_Geral_{ano}.csv");
}
