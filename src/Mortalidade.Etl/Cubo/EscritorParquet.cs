using Mortalidade.Etl.Dominio;
using Parquet;
using Parquet.Schema;

namespace Mortalidade.Etl.Cubo;

/// <summary>
/// Grava o cubo e a população em Parquet com códigos inteiros (UF IBGE, índice do grupo, sexo e índice da faixa),
/// descritos em <c>dimensoes.json</c>. O cubo tem um row group por ano, para o navegador ler só os anos do filtro.
/// </summary>
public static class EscritorParquet
{
    // Zstd no nível máximo: grava em ~22 s contra ~1 s no "Optimal", mas o cubo fica 23% menor (4,3 × 5,6 MB).
    // O ETL roda uma vez por mês; o arquivo é baixado por cada visitante da página. Medido em `dotnet run -- perfil`.
    private static readonly ParquetOptions Opcoes = new()
    {
        CompressionMethod = CompressionMethod.Zstd,
        CompressionLevel = System.IO.Compression.CompressionLevel.SmallestSize
    };

    private static readonly DataField<short> Ano = new("ano");
    private static readonly DataField<byte> Uf = new("uf");
    private static readonly DataField<short> Grupo = new("grupo");
    private static readonly DataField<byte> Sexo = new("sexo");
    private static readonly DataField<byte> Faixa = new("faixa");
    private static readonly DataField<int> Obitos = new("obitos");
    private static readonly DataField<float> ObitosAjustados = new("obitos_ajustados");
    private static readonly DataField<int> Habitantes = new("populacao");

    public static ParquetSchema EsquemaObitos { get; } = new(Ano, Uf, Grupo, Sexo, Faixa, Obitos, ObitosAjustados);

    public static ParquetSchema EsquemaPopulacao { get; } = new(Ano, Uf, Sexo, Faixa, Habitantes);

    public static async Task GravarObitosAsync(Stream destino, IEnumerable<IReadOnlyList<CelulaCubo>> celulasPorAno)
    {
        await using var escritor = await ParquetWriter.CreateAsync(EsquemaObitos, destino, Opcoes);
        foreach (var celulasDoAno in celulasPorAno)
        {
            // Ordenar agrupa valores repetidos e melhora a compressão.
            var celulas = celulasDoAno
                .OrderBy(c => c.Uf.CodigoIbge).ThenBy(c => c.GrupoCausa.Indice).ThenBy(c => c.Sexo).ThenBy(c => c.FaixaEtaria.Indice)
                .ToArray();

            var (anos, ufs, grupos, sexos, faixas) = (new short[celulas.Length], new byte[celulas.Length], new short[celulas.Length],
                new byte[celulas.Length], new byte[celulas.Length]);
            var (obitos, ajustados) = (new int[celulas.Length], new float[celulas.Length]);
            for (var i = 0; i < celulas.Length; i++)
            {
                var c = celulas[i];
                (anos[i], ufs[i], grupos[i]) = ((short)c.Ano.Valor, (byte)c.Uf.CodigoIbge, (short)c.GrupoCausa.Indice);
                (sexos[i], faixas[i]) = ((byte)c.Sexo, (byte)c.FaixaEtaria.Indice);
                (obitos[i], ajustados[i]) = ((int)c.Obitos, (float)c.ObitosAjustados);
            }

            using var grupo = escritor.CreateRowGroup();
            await grupo.WriteAsync<short>(Ano, anos);
            await grupo.WriteAsync<byte>(Uf, ufs);
            await grupo.WriteAsync<short>(Grupo, grupos);
            await grupo.WriteAsync<byte>(Sexo, sexos);
            await grupo.WriteAsync<byte>(Faixa, faixas);
            await grupo.WriteAsync<int>(Obitos, obitos);
            await grupo.WriteAsync<float>(ObitosAjustados, ajustados);
        }
    }

    public static async Task GravarPopulacaoAsync(Stream destino, IReadOnlyList<Populacao> populacao)
    {
        var linhas = populacao
            .OrderBy(p => p.Ano.Valor).ThenBy(p => p.Uf.CodigoIbge).ThenBy(p => p.Sexo).ThenBy(p => p.FaixaEtaria.Indice)
            .ToArray();

        var (anos, ufs, sexos, faixas, habitantes) = (new short[linhas.Length], new byte[linhas.Length], new byte[linhas.Length],
            new byte[linhas.Length], new int[linhas.Length]);
        for (var i = 0; i < linhas.Length; i++)
        {
            var p = linhas[i];
            (anos[i], ufs[i], sexos[i]) = ((short)p.Ano.Valor, (byte)p.Uf.CodigoIbge, (byte)p.Sexo);
            (faixas[i], habitantes[i]) = ((byte)p.FaixaEtaria.Indice, (int)p.Habitantes);
        }

        await using var escritor = await ParquetWriter.CreateAsync(EsquemaPopulacao, destino, Opcoes);
        using var grupo = escritor.CreateRowGroup();
        await grupo.WriteAsync<short>(Ano, anos);
        await grupo.WriteAsync<byte>(Uf, ufs);
        await grupo.WriteAsync<byte>(Sexo, sexos);
        await grupo.WriteAsync<byte>(Faixa, faixas);
        await grupo.WriteAsync<int>(Habitantes, habitantes);
    }
}
