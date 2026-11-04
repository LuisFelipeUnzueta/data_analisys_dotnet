"""Gera data/ibge/populacao_fontes.csv: anos-âncora da população por UF × sexo × faixa quinquenal.

- 1980: Censo pelo SIDRA, tabela 200 (o Tabnet por município perde 739 mil pessoas nesse ano).
- 1991 e 1996: Censo e Contagem pelo Tabnet popbr, por município, somados por UF.
- 2000 em diante: estimativas RIPSA (Tabnet popsvs2024br) até o último ano do gabarito.

Uso: python scripts/baixar_populacao.py data/ibge/populacao_fontes.csv ULTIMO_ANO
"""
import datetime
import json
import re
import sys
import urllib.request
from collections import Counter

from tabnet import consultar, numero

RIPSA = "ibge/cnv/popsvs2024br.def"
INCREMENTO = "População_residente"
SIGLAS = {"RO": "11", "AC": "12", "AM": "13", "RR": "14", "PA": "15", "AP": "16", "TO": "17", "MA": "21", "PI": "22",
          "CE": "23", "RN": "24", "PB": "25", "PE": "26", "AL": "27", "SE": "28", "BA": "29", "MG": "31", "ES": "32",
          "RJ": "33", "SP": "35", "PR": "41", "SC": "42", "RS": "43", "MS": "50", "MT": "51", "GO": "52", "DF": "53"}

# SIDRA tabela 200: grupos de idade (classificação 58) e sexo (classificação 2).
SIDRA_1980 = ("https://apisidra.ibge.gov.br/values/t/200/n3/all/v/93/p/1980/c2/4,5/c1/0/c58/"
              "1140,1141,1142,1143,1144,1145,1146,1147,1148,1149,1150,1151,1152,1153,1154,1155,2503,3245/f/c")
SIDRA_FAIXAS = {"1140": "0-4", "1141": "5-9", "1142": "10-14", "1143": "15-19", "1144": "20-24", "1145": "25-29",
                "1146": "30-34", "1147": "35-39", "1148": "40-44", "1149": "45-49", "1150": "50-54", "1151": "55-59",
                "1152": "60-64", "1153": "65-69", "1154": "70-74", "1155": "75-79", "2503": "80+", "3245": "Ignorada"}
SIDRA_SEXOS = {"4": "masculino", "5": "feminino"}


def faixa_quinquenal(rotulo):
    """Rótulo de idade do Tabnet ("Menor 1 ano", "1 ano", "20 a 24 anos", "De 20 a 24 anos", "80 anos e mais") → faixa."""
    r = rotulo.lower()
    if "ignorada" in r:
        return "Ignorada"
    if "não detalhado" in r:
        return None
    if "menor 1" in r:
        return "0-4"
    inicio = int(re.search(r"(\d+)", r).group(1))
    return "80+" if inicio >= 80 else f"{inicio // 5 * 5}-{inicio // 5 * 5 + 4}"


def censo_1980(saida):
    dados = json.load(urllib.request.urlopen(SIDRA_1980, timeout=300))
    for linha in dados[1:]:
        valor = int(linha["V"]) if linha["V"].isdigit() else 0
        saida[(1980, "censo_sidra200", linha["D1C"], SIDRA_SEXOS[linha["D4C"]], SIDRA_FAIXAS[linha["D6C"]])] += valor


def popbr(saida, ano, fonte):
    for codigo_sexo, sexo in (("1", "masculino"), ("2", "feminino"), ("3", "ignorado")):
        cabecalho, linhas, _ = consultar("ibge/cnv/popbr.def", "Município", "Faixa_Etária_detalhada", INCREMENTO,
                                         [f"popbr{ano % 100:02d}.dbf"], {"SSexo": codigo_sexo})
        for municipio, *valores in linhas:
            if municipio == "Total":
                continue
            uf = municipio.strip()[:2]
            if not uf.isdigit():
                sigla = re.search(r"- ([A-Z]{2})$", municipio.strip())
                if not sigla:
                    raise SystemExit(f"Município sem UF: {municipio}")
                uf = SIGLAS[sigla.group(1)]
            for rotulo, valor in zip(cabecalho[1:], valores):
                faixa = faixa_quinquenal(rotulo) if rotulo != "Total" else None
                if faixa and (numero(valor) or sexo != "ignorado"):
                    saida[(ano, fonte, uf, sexo, faixa)] += numero(valor)


def ripsa(saida, ano):
    for codigo_sexo, sexo in (("1", "masculino"), ("2", "feminino")):
        cabecalho, linhas, _ = consultar(RIPSA, "Unidade_da_Federação", "Faixa_Etária_2", INCREMENTO,
                                         [f"pop{ano % 100:02d}.dbf"], {"SSexo": codigo_sexo})
        for uf, *valores in linhas:
            for rotulo, valor in zip(cabecalho[1:], valores):
                if rotulo != "Total" and uf != "Total":
                    saida[(ano, "ripsa2024", uf[:2], sexo, faixa_quinquenal(rotulo))] += numero(valor)


def main(destino, ultimo_ano):
    saida = Counter()
    censo_1980(saida)
    popbr(saida, 1991, "censo")
    popbr(saida, 1996, "contagem")
    for ano in range(2000, ultimo_ano + 1):
        ripsa(saida, ano)

    hoje = datetime.date.today().isoformat()
    with open(destino, "w", encoding="utf-8", newline="\n") as arquivo:
        arquivo.write("ano;fonte;uf;sexo;faixa;populacao;consulta\n")
        for (ano, fonte, uf, sexo, faixa), populacao in sorted(saida.items()):
            arquivo.write(f"{ano};{fonte};{uf};{sexo};{faixa};{populacao};{hoje}\n")
    print(f"{len(saida)} linhas, âncoras 1980, 1991, 1996 e 2000–{ultimo_ano}")


if __name__ == "__main__":
    main(sys.argv[1], int(sys.argv[2]))
