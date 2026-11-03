using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Cubo;

/// <summary>Soma óbitos por célula do cubo. Não é thread-safe: use uma instância por ano.</summary>
public sealed class AgregadorCubo(MapaCausas mapa)
{
    private readonly Dictionary<Chave, long> _celulas = [];
    private readonly Dictionary<CausaBasica, Result<GrupoCausa>> _grupos = [];
    private readonly Dictionary<string, long> _falhas = [];
    private readonly List<string> _exemplosFalha = [];

    public IReadOnlyDictionary<string, long> Falhas => _falhas;

    public IReadOnlyList<string> ExemplosFalha => _exemplosFalha;

    public void Adicionar(Result<RegistroObito> registro)
    {
        if (registro.IsFailure)
        {
            RegistrarFalha(registro.Erro!);
            return;
        }

        var obito = registro.Valor;
        if (!_grupos.TryGetValue(obito.Causa, out var grupo))
            _grupos[obito.Causa] = grupo = mapa.Resolver(obito.Causa);

        if (grupo.IsFailure)
        {
            RegistrarFalha(grupo.Erro!);
            return;
        }

        var chave = new Chave(obito.Ano, obito.UfResidencia, grupo.Valor, obito.Sexo, obito.FaixaEtaria);
        _celulas[chave] = _celulas.GetValueOrDefault(chave) + 1;
    }

    public IReadOnlyList<CelulaCubo> Celulas() =>
        _celulas.Select(c => new CelulaCubo(c.Key.Ano, c.Key.Uf, c.Key.Grupo, c.Key.Sexo, c.Key.FaixaEtaria, c.Value)).ToList();

    private void RegistrarFalha(Erro erro)
    {
        _falhas[erro.Codigo] = _falhas.GetValueOrDefault(erro.Codigo) + 1;
        if (_exemplosFalha.Count < 10)
            _exemplosFalha.Add(erro.Mensagem);
    }

    private readonly record struct Chave(Ano Ano, Uf Uf, GrupoCausa Grupo, Sexo Sexo, FaixaEtaria FaixaEtaria);
}
