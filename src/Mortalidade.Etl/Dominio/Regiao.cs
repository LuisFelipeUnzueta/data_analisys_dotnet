namespace Mortalidade.Etl.Dominio;

public enum Regiao
{
    Norte = 1,
    Nordeste = 2,
    Sudeste = 3,
    Sul = 4,
    CentroOeste = 5
}

public static class RegiaoNome
{
    public static string De(Regiao regiao) => regiao switch
    {
        Regiao.Norte => "Norte",
        Regiao.Nordeste => "Nordeste",
        Regiao.Sudeste => "Sudeste",
        Regiao.Sul => "Sul",
        _ => "Centro-Oeste"
    };
}
