using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Validacao;

namespace Mortalidade.Etl.Leitura;

/// <summary>
/// Lê <c>data/tabnet/gabarito.csv</c> (ano;dimensao;categoria;obitos;consulta), gerado por <c>scripts/gerar_gabarito.py</c>.
/// Cada ano precisa do total; as linhas de UF e de sexo ficam em dicionários tipados.
/// </summary>
public static class LeitorGabaritoTabnet
{
    public static Result<IReadOnlyDictionary<int, TotaisTabnet>> Ler(IEnumerable<string> linhasCsv)
    {
        var totais = new Dictionary<int, long>();
        var porUf = new Dictionary<int, Dictionary<int, long>>();
        var porSexo = new Dictionary<int, Dictionary<Sexo, long>>();
        var numero = 1;
        foreach (var linha in linhasCsv.Skip(1))
        {
            numero++;
            if (string.IsNullOrWhiteSpace(linha))
                continue;

            var c = linha.Split(';');
            if (c.Length < 4 || !int.TryParse(c[0], out var ano) || !long.TryParse(c[3], out var obitos))
                return Falha("gabarito.linha_invalida", $"Linha {numero} inválida: {linha}");

            switch (c[1])
            {
                case "total":
                    totais[ano] = obitos;
                    break;
                case "uf" when int.TryParse(c[2], out var uf):
                    Valores(porUf, ano)[uf] = obitos;
                    break;
                case "sexo" when SexoNome.Ler(c[2]) is { } sexo:
                    Valores(porSexo, ano)[sexo] = obitos;
                    break;
                default:
                    return Falha("gabarito.linha_invalida", $"Linha {numero} com dimensão ou categoria desconhecida: {linha}");
            }
        }

        if (totais.Count == 0)
            return Falha("gabarito.vazio", "Gabarito sem nenhum ano.");

        return Result<IReadOnlyDictionary<int, TotaisTabnet>>.Sucesso(totais.ToDictionary(t => t.Key, t => new TotaisTabnet(
            t.Value, porUf.GetValueOrDefault(t.Key) ?? [], porSexo.GetValueOrDefault(t.Key) ?? [])));
    }

    private static Dictionary<TChave, long> Valores<TChave>(Dictionary<int, Dictionary<TChave, long>> porAno, int ano)
        where TChave : notnull =>
        porAno.TryGetValue(ano, out var valores) ? valores : porAno[ano] = [];

    private static Result<IReadOnlyDictionary<int, TotaisTabnet>> Falha(string codigo, string mensagem) =>
        Result<IReadOnlyDictionary<int, TotaisTabnet>>.Falha(codigo, mensagem);
}
