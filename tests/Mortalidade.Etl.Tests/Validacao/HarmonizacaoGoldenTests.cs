using Mortalidade.Etl.Leitura;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Validacao;

namespace Mortalidade.Etl.Tests.Validacao;

/// <summary>
/// Testes sobre a tabela real (data/cause_map/grupos.csv) e fixtures extraídas dos CSV do SIM 1979–2023.
/// O golden de 1992–1999 trava a continuidade da troca CID-9 → CID-10: mudar uma faixa exige regerá-lo.
/// </summary>
public class HarmonizacaoGoldenTests
{
    private static readonly MapaCausas Mapa =
        LeitorMapaCausas.Ler(File.ReadLines(Caminho("Dados", "grupos.csv"))).Valor;

    private static string Caminho(params string[] partes) =>
        Path.Combine([AppContext.BaseDirectory, .. partes]);

    private static IEnumerable<string[]> Linhas(string fixture) =>
        File.ReadLines(Caminho("Fixtures", fixture)).Skip(1).Select(l => l.Split(';'));

    private static RevisaoCid RevisaoDoAno(int ano) =>
        RevisaoCidPorAno.De(Ano.Criar(ano).Valor);

    private static Dictionary<string, Dictionary<int, long>> ObitosPorGrupoEAno()
    {
        var totais = new Dictionary<string, Dictionary<int, long>>();
        foreach (var campos in Linhas("obitos_por_codigo_1992_1999.csv"))
        {
            var ano = int.Parse(campos[0]);
            var grupo = Mapa.Resolver(new CausaBasica(RevisaoDoAno(ano), campos[1])).Valor.Codigo;
            var porAno = totais.TryGetValue(grupo, out var existente) ? existente : totais[grupo] = [];
            porAno[ano] = porAno.GetValueOrDefault(ano) + long.Parse(campos[2]);
        }

        return totais;
    }

    [Fact]
    public void TodoCodigoObservado_TemExatamenteUmGrupo()
    {
        var falhas = Linhas("codigos_observados.csv")
            .Select(c => Mapa.Resolver(new CausaBasica((RevisaoCid)int.Parse(c[0]), c[1])))
            .Where(r => r.IsFailure)
            .Select(r => r.Erro!.Mensagem)
            .ToList();

        Assert.Empty(falhas);
    }

    [Fact]
    public void TotaisPorGrupo1992a1999_BatemComGolden()
    {
        var calculado = ObitosPorGrupoEAno();
        var divergencias = Linhas("golden_grupos_1992_1999.csv")
            .Select(c => (ano: int.Parse(c[0]), grupo: c[1], esperado: long.Parse(c[2])))
            .Where(g => calculado.GetValueOrDefault(g.grupo)?.GetValueOrDefault(g.ano) != g.esperado)
            .Select(g => $"{g.ano} {g.grupo}: esperado {g.esperado}")
            .ToList();

        Assert.Empty(divergencias);
    }

    [Fact]
    public void ComparabilidadeDaTabela_ConfereComOSaltoEntre1995e1996()
    {
        var obitos = ObitosPorGrupoEAno();
        var incoerentes = Mapa.Grupos
            .Where(g => g.ComparabilidadeReduzida != ContinuidadeCid.ComparabilidadeReduzida(
                obitos.GetValueOrDefault(g.Codigo) ?? []))
            .Select(g => $"{g.Codigo}: desvio {ContinuidadeCid.Desvio(obitos.GetValueOrDefault(g.Codigo) ?? []):P1}")
            .ToList();

        Assert.Empty(incoerentes);
    }

    [Theory]
    [InlineData("isquemicas")]
    [InlineData("cerebrovasculares")]
    [InlineData("mal_definidas")]
    [InlineData("homicidio")]
    [InlineData("neo_snc")]
    public void GruposPrincipais_PassamPelaTrocaDeCidDentroDoLimite(string grupo)
    {
        var desvio = ContinuidadeCid.Desvio(ObitosPorGrupoEAno()[grupo]);

        Assert.NotNull(desvio);
        Assert.InRange(desvio.Value, 0, ContinuidadeCid.LimiteDesvio);
    }
}
