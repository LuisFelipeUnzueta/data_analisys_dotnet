namespace Mortalidade.Etl.Dominio;

public enum Sexo
{
    Ignorado = 0,
    Masculino = 1,
    Feminino = 2
}

public static class SexoSim
{
    // Valores fora do dicionário viram Ignorado: o registro não é descartado.
    public static Sexo DeCodigo(string? codigo) => codigo?.Trim().ToUpperInvariant() switch
    {
        "1" or "M" => Sexo.Masculino,
        "2" or "F" => Sexo.Feminino,
        _ => Sexo.Ignorado
    };
}
