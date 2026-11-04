namespace Mortalidade.Etl.Dominio;

public enum FontePopulacao
{
    Censo,
    Contagem,
    Ripsa2024,
    Interpolada,
    Extrapolada
}

/// <summary>Ponto de uma fonte populacional (censo, contagem ou RIPSA), ainda com idade possivelmente ignorada.</summary>
public sealed record PontoPopulacao(int Ano, FontePopulacao Fonte, Uf Uf, Sexo Sexo, FaixaEtaria FaixaEtaria, long Habitantes);

/// <summary>
/// Monta a série 1979–último ano por UF × sexo × faixa a partir dos anos-âncora:
/// idade ignorada redistribuída na proporção das faixas conhecidas; 1981–1999 por interpolação geométrica
/// entre âncoras; 1979 extrapolado com o crescimento 1980–1981; Tocantins somado a Goiás antes de 1989.
/// </summary>
public static class EstimativaPopulacional
{

    public static Result<(IReadOnlyList<Populacao> Populacao, IReadOnlyDictionary<int, FontePopulacao> FontePorAno)> Estimar(
        IEnumerable<PontoPopulacao> pontos, int ultimoAno)
    {
        var porAno = pontos.GroupBy(p => p.Ano).ToDictionary(g => g.Key, g => g.ToList());
        if (porAno.Values.SelectMany(p => p).Any(p => p.Sexo == Sexo.Ignorado))
            return Falha("populacao.sexo_ignorado", "Fonte populacional com sexo ignorado não é suportada.");

        var ancoras = new SortedDictionary<int, Dictionary<Estrato, double>>();
        var fontes = new Dictionary<int, FontePopulacao>();
        foreach (var (ano, linhas) in porAno)
        {
            var distribuida = RedistribuirIdadeIgnorada(linhas);
            if (distribuida.IsFailure)
                return Falha(distribuida.Erro!.Codigo, distribuida.Erro.Mensagem);

            ancoras[ano] = distribuida.Valor;
            fontes[ano] = linhas[0].Fonte;
        }

        var faltantes = new[] { 1980, 1981 }.Concat(Enumerable.Range(2000, ultimoAno - 1999))
            .Where(a => a != 1981 && !ancoras.ContainsKey(a)).ToList();
        if (faltantes.Count > 0)
            return Falha("populacao.ancora_ausente", $"Anos-âncora ausentes: {string.Join(", ", faltantes)}.");

        var serie = new Dictionary<int, Dictionary<Estrato, double>>();
        for (var ano = 1980; ano <= ultimoAno; ano++)
        {
            if (ancoras.TryGetValue(ano, out var ancora))
            {
                serie[ano] = ancora;
                continue;
            }

            var anterior = ancoras.Keys.Last(a => a < ano);
            var posterior = ancoras.Keys.First(a => a > ano);
            var t = (double)(ano - anterior) / (posterior - anterior);
            serie[ano] = TemTocantins(ancoras[anterior])
                ? Interpolar(ancoras[anterior], ancoras[posterior], t)
                : InterpolarComGoiasHistorico(ano, ancoras[anterior], ancoras[posterior], t);
            fontes[ano] = FontePopulacao.Interpolada;
        }

        serie[1979] = Interpolar(serie[1980], serie[1981], -1);
        fontes[1979] = FontePopulacao.Extrapolada;

        var populacao = serie.OrderBy(s => s.Key)
            .SelectMany(s => AplicarTocantins(s.Key, s.Value))
            .ToList();

        return Result<(IReadOnlyList<Populacao>, IReadOnlyDictionary<int, FontePopulacao>)>.Sucesso((populacao, fontes));
    }

    private static Result<Dictionary<Estrato, double>> RedistribuirIdadeIgnorada(IReadOnlyList<PontoPopulacao> linhas)
    {
        var resultado = new Dictionary<Estrato, double>();
        foreach (var grupo in linhas.GroupBy(l => (l.Uf, l.Sexo)))
        {
            var conhecidas = grupo.Where(l => l.FaixaEtaria != FaixaEtaria.Ignorada).ToList();
            var totalConhecido = conhecidas.Sum(l => (double)l.Habitantes);
            var ignorada = grupo.Where(l => l.FaixaEtaria == FaixaEtaria.Ignorada).Sum(l => (double)l.Habitantes);
            if (totalConhecido == 0 && ignorada > 0)
                return Result<Dictionary<Estrato, double>>.Falha("populacao.sem_idade_conhecida",
                    $"{linhas[0].Ano} {grupo.Key.Uf} {grupo.Key.Sexo}: só há idade ignorada.");

            var fator = totalConhecido > 0 ? 1 + ignorada / totalConhecido : 1;
            foreach (var linha in conhecidas)
            {
                var estrato = new Estrato(linha.Uf, linha.Sexo, linha.FaixaEtaria);
                resultado[estrato] = resultado.GetValueOrDefault(estrato) + linha.Habitantes * fator;
            }
        }

        return Result<Dictionary<Estrato, double>>.Sucesso(resultado);
    }

    /// <summary>Geométrica: P0 · (P1/P0)^t. Se um dos extremos é zero, cai para a linear.</summary>
    private static Dictionary<Estrato, double> Interpolar(Dictionary<Estrato, double> inicio, Dictionary<Estrato, double> fim, double t) =>
        inicio.Keys.Union(fim.Keys).ToDictionary(e => e, e =>
        {
            var (p0, p1) = (inicio.GetValueOrDefault(e), fim.GetValueOrDefault(e));
            return p0 > 0 && p1 > 0 ? p0 * Math.Pow(p1 / p0, t) : Math.Max(0, p0 + (p1 - p0) * t);
        });

    private static bool TemTocantins(Dictionary<Estrato, double> estratos) =>
        estratos.Any(e => e.Key.Uf == Uf.Tocantins && e.Value > 0);

    /// <summary>
    /// Entre uma âncora sem Tocantins (Censo 1980, com o território dentro de Goiás) e uma com Tocantins,
    /// interpola GO+TO como um só território; a partir de 1989 separa TO pela proporção TO/(GO+TO) da âncora posterior.
    /// Interpolar GO e TO separadamente contaria a área de TO duas vezes em 1989–1990.
    /// </summary>
    private static Dictionary<Estrato, double> InterpolarComGoiasHistorico(
        int ano, Dictionary<Estrato, double> inicio, Dictionary<Estrato, double> fim, double t)
    {
        Dictionary<Estrato, double> Unir(Dictionary<Estrato, double> estratos) => estratos
            .GroupBy(e => e.Key.Uf == Uf.Tocantins ? e.Key with { Uf = Uf.Goias } : e.Key)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Value));

        var interpolada = Interpolar(Unir(inicio), Unir(fim), t);
        if (ano < Uf.AnoCriacaoTocantins)
            return interpolada;

        foreach (var (estrato, combinado) in interpolada.Where(e => e.Key.Uf == Uf.Goias).ToList())
        {
            var noTocantins = fim.GetValueOrDefault(estrato with { Uf = Uf.Tocantins });
            var total = noTocantins + fim.GetValueOrDefault(estrato);
            var proporcao = total > 0 ? noTocantins / total : 0;
            interpolada[estrato] = combinado * (1 - proporcao);
            interpolada[estrato with { Uf = Uf.Tocantins }] = combinado * proporcao;
        }

        return interpolada;
    }

    private static IEnumerable<Populacao> AplicarTocantins(int anoNumero, Dictionary<Estrato, double> estratos)
    {
        var ano = Ano.Criar(anoNumero).Valor;
        return estratos
            .GroupBy(e => anoNumero < Uf.AnoCriacaoTocantins && e.Key.Uf == Uf.Tocantins
                ? e.Key with { Uf = Uf.Goias }
                : e.Key)
            .Select(g => new Populacao(ano, g.Key.Uf, g.Key.Sexo, g.Key.FaixaEtaria, (long)Math.Round(g.Sum(e => e.Value))));
    }

    private static Result<(IReadOnlyList<Populacao>, IReadOnlyDictionary<int, FontePopulacao>)> Falha(string codigo, string mensagem) =>
        Result<(IReadOnlyList<Populacao>, IReadOnlyDictionary<int, FontePopulacao>)>.Falha(codigo, mensagem);

    private readonly record struct Estrato(Uf Uf, Sexo Sexo, FaixaEtaria FaixaEtaria);
}
