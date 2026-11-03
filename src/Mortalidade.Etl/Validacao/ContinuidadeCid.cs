namespace Mortalidade.Etl.Validacao;

/// <summary>
/// Mede o salto de um grupo na troca CID-9 → CID-10: a razão 1996/1995 comparada com a média
/// das razões vizinhas (1992/93 a 1994/95 e 1996/97 a 1998/99).
/// </summary>
public static class ContinuidadeCid
{
    public const double LimiteDesvio = 0.10;

    private static readonly int[] AnosBaseVizinhos = [1992, 1993, 1994, 1996, 1997, 1998];

    public static double? Desvio(IReadOnlyDictionary<int, long> obitosPorAno)
    {
        long Obitos(int ano) => obitosPorAno.TryGetValue(ano, out var n) ? n : 0;

        if (Obitos(1995) == 0)
            return null;

        var razao = (double)Obitos(1996) / Obitos(1995);
        var vizinhas = AnosBaseVizinhos
            .Where(ano => Obitos(ano) > 0)
            .Select(ano => (double)Obitos(ano + 1) / Obitos(ano))
            .ToList();
        var referencia = vizinhas.Count > 0 ? vizinhas.Average() : 1;

        return Math.Abs(razao / referencia - 1);
    }

    public static bool ComparabilidadeReduzida(IReadOnlyDictionary<int, long> obitosPorAno) =>
        Desvio(obitosPorAno) > LimiteDesvio;
}
