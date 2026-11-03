namespace Mortalidade.Etl.Dominio;

public readonly record struct Ano
{
    public const int Primeiro = 1979;

    private Ano(int valor) => Valor = valor;

    public int Valor { get; }

    public static Result<Ano> Criar(int valor) =>
        valor < Primeiro
            ? Result<Ano>.Falha("ano.anterior_serie", $"Ano {valor} é anterior a {Primeiro}.")
            : Result<Ano>.Sucesso(new Ano(valor));

    public override string ToString() => Valor.ToString();
}
