using System.Globalization;
using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Leitura;

/// <summary>Lê os pesos da população-padrão (faixa;peso), por exemplo <c>data/padrao/oms_2000_2025.csv</c>.</summary>
public static class LeitorPopulacaoPadrao
{
    public static Result<PopulacaoPadrao> Ler(IEnumerable<string> linhasCsv)
    {
        var pesos = new Dictionary<FaixaEtaria, double>();
        foreach (var campos in linhasCsv.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Split(';')))
        {
            if (campos.Length != 2 || FaixaEtaria.DeRotulo(campos[0]) is not { } faixa
                || !double.TryParse(campos[1], CultureInfo.InvariantCulture, out var peso))
                return Result<PopulacaoPadrao>.Falha("padrao.linha_invalida", $"Linha inválida: {string.Join(';', campos)}.");
            pesos[faixa] = peso;
        }

        return PopulacaoPadrao.Criar(pesos);
    }
}
