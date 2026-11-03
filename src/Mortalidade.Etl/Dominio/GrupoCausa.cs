namespace Mortalidade.Etl.Dominio;

/// <param name="Indice">Posição do grupo em <see cref="MapaCausas.Grupos"/>; é o código numérico gravado no cubo.</param>
public sealed record GrupoCausa(int Indice, string Codigo, string Nome, string Capitulo, bool ComparabilidadeReduzida);
