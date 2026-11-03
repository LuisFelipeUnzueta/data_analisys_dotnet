using System.Runtime.InteropServices;
using Mortalidade.Etl.Dominio;

namespace Mortalidade.Etl.Cubo;

/// <summary>Soma os óbitos de um ano por célula do cubo. Não é thread-safe: use uma instância por ano.</summary>
public sealed class AgregadorCubo(MapaCausas mapa, Ano ano)
{
    private readonly Dictionary<(int Uf, int Grupo, Sexo Sexo, int Faixa), long> _celulas = [];
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
        var grupo = mapa.Resolver(obito.Causa);
        if (grupo.IsFailure)
        {
            RegistrarFalha(grupo.Erro!);
            return;
        }

        var chave = (obito.UfResidencia.CodigoIbge, grupo.Valor.Indice, obito.Sexo, obito.FaixaEtaria.Indice);
        CollectionsMarshal.GetValueRefOrAddDefault(_celulas, chave, out _)++;
    }

    public IReadOnlyList<CelulaCubo> Celulas() =>
        _celulas.Select(c => new CelulaCubo(ano, Uf.DeCodigoIbge(c.Key.Uf).Valor, mapa.Grupos[c.Key.Grupo], c.Key.Sexo,
            FaixaEtaria.Todas[c.Key.Faixa], c.Value)).ToList();

    private void RegistrarFalha(Erro erro)
    {
        _falhas[erro.Codigo] = _falhas.GetValueOrDefault(erro.Codigo) + 1;
        if (_exemplosFalha.Count < 10)
            _exemplosFalha.Add(erro.Mensagem);
    }
}
