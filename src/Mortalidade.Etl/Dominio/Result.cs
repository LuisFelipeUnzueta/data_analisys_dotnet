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

    /// <summary>Transforma o valor; uma falha passa adiante sem executar <paramref name="transformar"/>.</summary>
    public Result<TNovo> Map<TNovo>(Func<T, TNovo> transformar) =>
        IsSuccess ? Result<TNovo>.Sucesso(transformar(_valor!)) : Result<TNovo>.Falha(Erro!);

    /// <summary>Encadeia um passo que também pode falhar; a primeira falha interrompe a cadeia.</summary>
    public Result<TNovo> Bind<TNovo>(Func<T, Result<TNovo>> proximo) =>
        IsSuccess ? proximo(_valor!) : Result<TNovo>.Falha(Erro!);

    private static Result<T> Falha(Erro erro) => new(erro);
}
