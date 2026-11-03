namespace Mortalidade.Etl.Dominio;

public sealed class Result<T>
{
    private readonly T? _valor;

    private Result(T valor)
    {
        _valor = valor;
        IsSuccess = true;
    }

    private Result(Erro erro)
    {
        Erro = erro;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Erro? Erro { get; }

    public T Valor => IsSuccess
        ? _valor!
        : throw new InvalidOperationException($"Result sem valor: {Erro!.Codigo}.");

    public static Result<T> Sucesso(T valor) => new(valor);

    public static Result<T> Falha(string codigo, string mensagem) => new(new Erro(codigo, mensagem));
}
