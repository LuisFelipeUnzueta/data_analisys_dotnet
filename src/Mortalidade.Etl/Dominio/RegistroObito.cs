namespace Mortalidade.Etl.Dominio;

public sealed record RegistroObito(Ano Ano, Uf UfResidencia, CausaBasica Causa, Sexo Sexo, FaixaEtaria FaixaEtaria);
