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
    public static Sexo DeCodigo(ReadOnlySpan<char> codigo) => codigo.Trim() switch
    {
        "1" or "M" or "m" => Sexo.Masculino,
        "2" or "F" or "f" => Sexo.Feminino,
        _ => Sexo.Ignorado
    };
}

/// <summary>Nome do sexo nos arquivos de dados do projeto (gabarito, população e saída).</summary>
public static class SexoNome
{
    public static string De(Sexo sexo) => sexo switch
    {
        Sexo.Masculino => "masculino",
        Sexo.Feminino => "feminino",
        _ => "ignorado"
    };

    public static Sexo? Ler(string nome) => nome switch
    {
        "masculino" => Sexo.Masculino,
        "feminino" => Sexo.Feminino,
        "ignorado" => Sexo.Ignorado,
        _ => null
    };
}
