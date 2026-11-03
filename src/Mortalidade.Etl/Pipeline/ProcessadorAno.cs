using Mortalidade.Etl.Cubo;
using Mortalidade.Etl.Dominio;
using Mortalidade.Etl.Leitura;
using Mortalidade.Etl.Validacao;

namespace Mortalidade.Etl.Pipeline;

public sealed record ResultadoAno(int Ano, IReadOnlyList<CelulaCubo> Celulas, IReadOnlyList<string> Problemas)
{
    public bool Aprovado => Problemas.Count == 0;
}

/// <summary>Um ano do pipeline: ler o CSV → agregar → redistribuir idade ignorada → conferir com o Tabnet.</summary>
public sealed class ProcessadorAno(MapaCausas mapa, IReadOnlyList<LinhaGabarito> gabarito)
{
    public ResultadoAno Processar(Ano ano, Stream csv)
    {
        var agregador = new AgregadorCubo(mapa, ano);
        foreach (var registro in LeitorSimCsv.Ler(csv, ano))
            agregador.Adicionar(registro);

        var problemas = agregador.Falhas.Select(f => $"{ano}: {f.Value} registro(s) com {f.Key}")
            .Concat(agregador.ExemplosFalha.Take(3))
            .ToList();

        var celulas = RedistribuicaoIdade.Aplicar(agregador.Celulas());
        if (celulas.IsFailure)
            return new ResultadoAno(ano.Valor, [], [.. problemas, celulas.Erro!.Mensagem]);

        problemas.AddRange(ConferenciaTabnet.Divergencias(ano.Valor, celulas.Valor, gabarito));
        return new ResultadoAno(ano.Valor, celulas.Valor, problemas);
    }
}
