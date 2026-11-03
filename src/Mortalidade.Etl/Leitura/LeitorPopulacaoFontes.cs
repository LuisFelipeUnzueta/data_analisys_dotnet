using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Leitura;

/// <summary>
/// Lê <c>data/ibge/populacao_fontes.csv</c> (ano;fonte;uf;sexo;faixa;populacao;consulta), gerado do Tabnet
/// (censo 1991, contagem 1996, RIPSA 2000+) e do SIDRA tabela 200 (censo 1980). UF 00 é "ignorado" e deve ser zero.
/// </summary>
public static class LeitorPopulacaoFontes
{
    private static readonly Dictionary<string, FaixaEtaria> Faixas =
        FaixaEtaria.Todas.ToDictionary(f => f == FaixaEtaria.Ignorada ? "ign" : f.Rotulo);

    public static Result<IReadOnlyList<PontoPopulacao>> Ler(IEnumerable<string> linhasCsv)
    {
        var pontos = new List<PontoPopulacao>();
        var numero = 1;
        foreach (var linha in linhasCsv.Skip(1))
        {
            numero++;
            if (string.IsNullOrWhiteSpace(linha))
                continue;

            var c = linha.Split(';');
            if (c.Length < 6 || !int.TryParse(c[0], out var ano) || !int.TryParse(c[2], out var codigoUf)
                || !long.TryParse(c[5], out var habitantes) || !Faixas.TryGetValue(c[4], out var faixa)
                || Fonte(c[1]) is not { } fonte || Sexo(c[3]) is not { } sexo)
                return Falha("populacao.linha_invalida", $"Linha {numero} inválida: {linha}");

            if (codigoUf == 0)
            {
                if (habitantes != 0)
                    return Falha("populacao.uf_ignorada", $"Linha {numero}: população em UF ignorada.");
                continue;
            }

            var uf = Uf.DeCodigoIbge(codigoUf);
            if (uf.IsFailure)
                return Falha(uf.Erro!.Codigo, $"Linha {numero}: {uf.Erro.Mensagem}");

            pontos.Add(new PontoPopulacao(ano, fonte, uf.Valor, sexo, faixa, habitantes));
        }

        return Result<IReadOnlyList<PontoPopulacao>>.Sucesso(pontos);
    }

    private static FontePopulacao? Fonte(string valor) => valor switch
    {
        "censo" or "censo_sidra200" => FontePopulacao.Censo,
        "contagem" => FontePopulacao.Contagem,
        "ripsa2024" => FontePopulacao.Ripsa2024,
        _ => null
    };

    private static Sexo? Sexo(string valor) => valor switch
    {
        "masculino" => Dominio.Sexo.Masculino,
        "feminino" => Dominio.Sexo.Feminino,
        "ignorado" => Dominio.Sexo.Ignorado,
        _ => null
    };

    private static Result<IReadOnlyList<PontoPopulacao>> Falha(string codigo, string mensagem) =>
        Result<IReadOnlyList<PontoPopulacao>>.Falha(codigo, mensagem);
}
