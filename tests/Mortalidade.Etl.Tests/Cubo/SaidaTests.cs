using Mortalidade.Etl.Leitura;
using System.Text.Json;
using Mortalidade.Etl.Cubo;
using Mortalidade.Etl.Dominio;
using Parquet;
using Parquet.Schema;

namespace Mortalidade.Etl.Tests.Cubo;

public class SaidaTests
{
    private static readonly MapaCausas Mapa =
        LeitorMapaCausas.Ler(File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Dados", "grupos.csv"))).Valor;

    private static readonly PopulacaoPadrao Oms =
        LeitorPopulacaoPadrao.Ler(File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Dados", "oms_2000_2025.csv"))).Valor;

    private static readonly Uf Sp = Uf.DeCodigoIbge(35).Valor;
    private static readonly Uf Ac = Uf.DeCodigoIbge(12).Valor;

    private static CelulaCubo Celula(int ano, Uf uf, int grupo, Sexo sexo, int faixa, long obitos, double ajustados) =>
        new(Ano.Criar(ano).Valor, uf, Mapa.Grupos[grupo], sexo, FaixaEtaria.Todas[faixa], obitos, ajustados);

    private static async Task<Dictionary<string, Array>[]> LerAsync(MemoryStream arquivo)
    {
        arquivo.Position = 0;
        await using var leitor = await ParquetReader.CreateAsync(arquivo);
        var grupos = new Dictionary<string, Array>[leitor.RowGroupCount];
        for (var i = 0; i < leitor.RowGroupCount; i++)
        {
            using var grupo = leitor.OpenRowGroupReader(i);
            grupos[i] = [];
            foreach (var campo in leitor.Schema.DataFields)
                grupos[i][campo.Name] = await Ler(grupo, campo);
        }

        return grupos;
    }

    private static async Task<Array> Ler(ParquetRowGroupReader grupo, DataField campo)
    {
        async Task<Array> Coluna<T>() where T : struct
        {
            var valores = new T[grupo.RowCount];
            await grupo.ReadAsync<T>(campo, valores);
            return valores;
        }

        return campo.ClrType == typeof(short) ? await Coluna<short>()
            : campo.ClrType == typeof(byte) ? await Coluna<byte>()
            : campo.ClrType == typeof(int) ? await Coluna<int>()
            : await Coluna<float>();
    }

    [Fact]
    public async Task Obitos_UmRowGroupPorAno_OrdenadoECodificado()
    {
        using var arquivo = new MemoryStream();
        await EscritorParquet.GravarObitosAsync(arquivo, [
            [Celula(1979, Sp, 1, Sexo.Feminino, 16, 7, 7.5), Celula(1979, Ac, 0, Sexo.Masculino, 17, 1, 0)],
            [Celula(2023, Sp, 2, Sexo.Ignorado, 0, 3, 3)]]);

        var grupos = await LerAsync(arquivo);

        Assert.Equal(2, grupos.Length);
        Assert.Equal(new short[] { 1979, 1979 }, grupos[0]["ano"]);
        Assert.Equal(new byte[] { 12, 35 }, grupos[0]["uf"]);
        Assert.Equal(new short[] { 0, 1 }, grupos[0]["grupo"]);
        Assert.Equal(new byte[] { 1, 2 }, grupos[0]["sexo"]);
        Assert.Equal(new byte[] { 17, 16 }, grupos[0]["faixa"]);
        Assert.Equal(new[] { 1, 7 }, grupos[0]["obitos"]);
        Assert.Equal(new[] { 0f, 7.5f }, grupos[0]["obitos_ajustados"]);
        Assert.Equal(new short[] { 2023 }, grupos[1]["ano"]);
    }

    [Fact]
    public async Task Populacao_GravaTodasAsLinhasOrdenadas()
    {
        var faixa = FaixaEtaria.Todas[3];
        using var arquivo = new MemoryStream();
        await EscritorParquet.GravarPopulacaoAsync(arquivo, [
            new Populacao(Ano.Criar(2000).Valor, Sp, Sexo.Feminino, faixa, 1_500_000),
            new Populacao(Ano.Criar(1979).Valor, Ac, Sexo.Masculino, faixa, 20_000)]);

        var grupo = Assert.Single(await LerAsync(arquivo));

        Assert.Equal(new short[] { 1979, 2000 }, grupo["ano"]);
        Assert.Equal(new byte[] { 12, 35 }, grupo["uf"]);
        Assert.Equal(new[] { 20_000, 1_500_000 }, grupo["populacao"]);
    }

    [Fact]
    public void Dimensoes_DescrevemCodigosEProveniencia()
    {
        var fontes = new Dictionary<int, FontePopulacao> { [1979] = FontePopulacao.Extrapolada, [2000] = FontePopulacao.Ripsa2024 };
        using var json = JsonDocument.Parse(
            GeradorDimensoes.Gerar(Mapa, Oms, new Dictionary<int, (double?, double?)> { [2000] = (541.9, 666.9), [1979] = (608.7, null) }, fontes, new DateTimeOffset(2026, 11, 3, 0, 0, 0, TimeSpan.Zero)));
        var raiz = json.RootElement;

        var anos = raiz.GetProperty("anos").EnumerateArray().ToList();
        Assert.Equal(1979, anos[0].GetProperty("ano").GetInt32());
        Assert.True(anos[0].GetProperty("populacao_estimada").GetBoolean());
        Assert.Equal(10, anos[1].GetProperty("revisao_cid").GetInt32());
        Assert.Equal(666.9, anos[1].GetProperty("taxa_padronizada_brasil").GetDouble());
        Assert.Equal(JsonValueKind.Null, anos[0].GetProperty("taxa_padronizada_brasil").ValueKind);

        Assert.Equal(Mapa.Grupos.Count, raiz.GetProperty("grupos").GetArrayLength());
        Assert.Equal(18, raiz.GetProperty("faixas").GetArrayLength());
        Assert.Equal(27, raiz.GetProperty("ufs").GetArrayLength());
        Assert.Equal(1_000_000, raiz.GetProperty("padrao").GetProperty("pesos").EnumerateArray().Sum(p => p.GetProperty("peso").GetDouble()));
        Assert.Equal("Centro-Oeste", raiz.GetProperty("regioes").EnumerateArray().Last().GetProperty("nome").GetString());
    }
}
