"""Gera data/tabnet/gabarito.csv: óbitos por residência por ano × total, UF e sexo, dos anos FINAIS do Tabnet.

O último ano final vem do rodapé do Tabnet ("Dados finais disponíveis até AAAA"); anos preliminares ficam de fora,
então o ETL nunca publica um ano preliminar. Uso: python scripts/gerar_gabarito.py data/tabnet/gabarito.csv
"""
import datetime
import re
import sys

from tabnet import consultar, numero

PRIMEIRO_ANO = 1979
PRIMEIRO_ANO_CID10 = 1996
INCREMENTO = "Óbitos_p/Residênc"
SEXOS = {"Mas": "masculino", "Fem": "feminino", "Ign": "ignorado"}  # o Tabnet usa "Masc" ou "Masculino"


def ultimo_ano_final():
    _, _, texto = consultar("sim/cnv/obt10uf.def", "Ano_do_Óbito", "--Não-Ativa--", INCREMENTO, ["obtuf96.dbf"])
    achado = re.search(r"Dados finais disponíveis até (\d{4})", texto)
    if not achado:
        raise SystemExit("Rodapé do Tabnet sem 'Dados finais disponíveis até'.")
    return int(achado.group(1))


def tabular(definicao, linha, anos):
    """{(rótulo da linha, ano): óbitos} para a tabulação linha × ano."""
    cabecalho, linhas, _ = consultar(definicao, linha, "Ano_do_Óbito", INCREMENTO, [f"obtuf{a % 100:02d}.dbf" for a in anos])
    return {(l[0], int(ano)): numero(v) for l in linhas for ano, v in zip(cabecalho[1:], l[1:]) if ano.isdigit()}


def main(destino):
    ultimo = ultimo_ano_final()
    linhas = []
    for definicao, anos in (("sim/cnv/obt09uf.def", range(PRIMEIRO_ANO, PRIMEIRO_ANO_CID10)),
                            ("sim/cnv/obt10uf.def", range(PRIMEIRO_ANO_CID10, ultimo + 1))):
        for (rotulo, ano), obitos in tabular(definicao, "Unidade_da_Federação", anos).items():
            codigo = rotulo.split(" ")[0]
            if rotulo == "Total":
                linhas.append((ano, "total", "total", obitos))
            elif codigo.isdigit():
                linhas.append((ano, "uf", codigo, obitos))
            elif obitos:
                raise SystemExit(f"Categoria de UF inesperada: {rotulo} {ano} {obitos}")
        for (rotulo, ano), obitos in tabular(definicao, "Sexo", anos).items():
            if rotulo != "Total":
                linhas.append((ano, "sexo", SEXOS[rotulo[:3]], obitos))

    hoje = datetime.date.today().isoformat()
    with open(destino, "w", encoding="utf-8", newline="\n") as arquivo:
        arquivo.write("ano;dimensao;categoria;obitos;consulta\n")
        for ano, dimensao, categoria, obitos in sorted(linhas):
            arquivo.write(f"{ano};{dimensao};{categoria};{obitos};{hoje}\n")
    print(f"{len(linhas)} linhas, {PRIMEIRO_ANO}–{ultimo} (último ano final do Tabnet)")


if __name__ == "__main__":
    main(sys.argv[1])
