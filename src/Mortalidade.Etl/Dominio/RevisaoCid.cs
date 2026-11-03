namespace Mortalidade.Etl.Dominio;

public enum RevisaoCid
{
    Cid9 = 9,
    Cid10 = 10
}

public static class RevisaoCidPorAno
{
    public const int PrimeiroAnoCid10 = 1996;

    public static RevisaoCid De(Ano ano) => ano.Valor < PrimeiroAnoCid10 ? RevisaoCid.Cid9 : RevisaoCid.Cid10;
}
