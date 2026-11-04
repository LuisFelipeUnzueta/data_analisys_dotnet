"""Consulta ao Tabnet do DATASUS (POST em tabcgi.exe), usada pelos scripts de gabarito e de população.

O Tabnet não tem API: o formulário é enviado em ISO-8859-1 com formato=prn e a resposta é HTML com a tabela
separada por ";" dentro de <PRE>. Só biblioteca padrão.
"""
import html
import re
import time
import urllib.parse
import urllib.request

URL = "http://tabnet.datasus.gov.br/cgi/tabcgi.exe?{definicao}"


def consultar(definicao, linha, coluna, incremento, arquivos, filtros=None):
    """Devolve (cabeçalho, linhas, texto_completo). Linhas mal formadas e o rodapé ficam de fora."""
    campos = [("Linha", linha), ("Coluna", coluna), ("Incremento", incremento)]
    campos += [("Arquivos", a) for a in arquivos]
    campos += list((filtros or {}).items())
    campos += [("formato", "prn"), ("zeradas", "exibirlz"), ("mostre", "Mostra")]
    corpo = urllib.parse.urlencode(campos, encoding="latin-1").encode("ascii")
    pedido = urllib.request.Request(URL.format(definicao=definicao), data=corpo,
                                    headers={"Content-Type": "application/x-www-form-urlencoded"})

    for tentativa in range(3):
        try:
            texto = urllib.request.urlopen(pedido, timeout=300).read().decode("latin-1")
            break
        except OSError:
            if tentativa == 2:
                raise
            time.sleep(10 * (tentativa + 1))

    tabela = re.search(r"<PRE>(.*?)</PRE>", texto, re.S | re.I)
    if not tabela:
        raise SystemExit(f"Tabnet sem tabela para {definicao} {arquivos} {filtros}:\n{texto[:500]}")

    linhas = html.unescape(re.sub(r"<[^>]+>", "", tabela.group(1))).strip().splitlines()
    cabecalho = [c.strip('"') for c in linhas[0].split(";")]
    corpo_tabela = [[c.strip('"') for c in l.split(";")] for l in linhas[1:]]
    return cabecalho, [l for l in corpo_tabela if len(l) == len(cabecalho)], html.unescape(texto)


def numero(valor):
    """Células vazias ou "-" do Tabnet valem zero."""
    valor = valor.strip()
    return int(valor) if valor.isdigit() else 0
