using Mortalidade.Etl.Dominio;
using nietras.SeparatedValues;

namespace Mortalidade.Etl.Leitura;

/// <summary>
/// Lê um CSV anual do SIM (OpenDATASUS, 1979–atual). Colunas são localizadas pelo nome, sem diferenciar caixa,
/// porque a posição e a caixa mudam entre anos. Só óbitos não fetais (TIPOBITO = 2) são devolvidos.
/// </summary>
public static class LeitorSimCsv
{
    public static IEnumerable<Result<RegistroObito>> Ler(Stream csv, Ano ano)
    {
        using var leitor = Sep.New(';').Reader(o => o with { Unescape = true }).From(csv);
        var colunas = leitor.Header.ColNames;

        int Coluna(params string[] nomes) =>
            colunas.ToList().FindIndex(c => nomes.Any(n => string.Equals(c, n, StringComparison.OrdinalIgnoreCase)));

        var (tipo, sexo, idade, causa) = (Coluna("TIPOBITO"), Coluna("SEXO"), Coluna("IDADE"), Coluna("CAUSABAS"));
        var (residencia, ocorrencia) = (Coluna("CODMUNRES", "MUNIRES"), Coluna("CODMUNOCOR", "MUNIOCOR"));
        if (new[] { tipo, sexo, idade, causa, residencia, ocorrencia }.Any(i => i < 0))
        {
            yield return Result<RegistroObito>.Falha("sim.coluna_ausente",
                $"CSV de {ano} sem coluna obrigatória. Colunas: {string.Join(';', colunas)}.");
            yield break;
        }

        var causas = new CacheCausas(RevisaoCidPorAno.De(ano));
        foreach (var linha in leitor)
        {
            if (linha[tipo].Span.Trim() is not "2")
                continue;

            var uf = UfDoMunicipio(linha[residencia].Span) ?? UfDoMunicipio(linha[ocorrencia].Span);
            if (uf is null)
            {
                yield return Result<RegistroObito>.Falha("sim.uf_invalida",
                    $"Óbito de {ano} sem UF válida: residência '{linha[residencia]}', ocorrência '{linha[ocorrencia]}'.");
                continue;
            }

            // IdadeEmAnos nunca é negativa, então a faixa sempre resolve.
            var faixa = FaixaEtaria.DeIdadeEmAnos(IdadeEmAnos(linha[idade].Span)).Valor;
            yield return Result<RegistroObito>.Sucesso(new RegistroObito(
                ano, uf, causas.Obter(linha[causa].Span), SexoSim.DeCodigo(linha[sexo].Span), faixa));
        }
    }

    /// <summary>Reaproveita a mesma <see cref="CausaBasica"/> por código, sem alocar string por registro.</summary>
    private sealed class CacheCausas(RevisaoCid revisao)
    {
        private readonly Dictionary<string, CausaBasica> _causas = [];

        public CausaBasica Obter(ReadOnlySpan<char> campo)
        {
            var codigo = campo.Trim();
            Span<char> maiusculo = codigo.Length <= 16 ? stackalloc char[codigo.Length] : new char[codigo.Length];
            codigo.ToUpperInvariant(maiusculo);

            var busca = _causas.GetAlternateLookup<ReadOnlySpan<char>>();
            if (busca.TryGetValue(maiusculo, out var causa))
                return causa;

            causa = new CausaBasica(revisao, maiusculo.ToString());
            _causas[causa.Codigo] = causa;
            return causa;
        }
    }

    /// <summary>
    /// IDADE do SIM: 1º dígito é a unidade (0 minutos, 1 horas, 2 dias, 3 meses, 4 anos, 5 = 100 + anos).
    /// Vazio, 000 e 999 são ignorados. Zeros à esquerda perdidos (2022–2023) são recompostos.
    /// </summary>
    public static int? IdadeEmAnos(ReadOnlySpan<char> campo)
    {
        var valor = campo.Trim();
        if (valor.IsEmpty || valor.Length > 3 || !int.TryParse(valor, out var numero))
            return null;

        if (numero is 0 or 999)
            return null;

        var (unidade, quantidade) = (numero / 100, numero % 100);
        return unidade switch
        {
            0 or 1 or 2 or 3 => 0,
            4 => quantidade,
            5 => 100 + quantidade,
            _ => null
        };
    }

    private static Uf? UfDoMunicipio(ReadOnlySpan<char> municipio)
    {
        var valor = municipio.Trim();
        if (valor.Length < 2 || !int.TryParse(valor[..2], out var codigo))
            return null;

        var uf = Uf.DeCodigoHistorico(codigo);
        return uf.IsSuccess ? uf.Valor : null;
    }
}
