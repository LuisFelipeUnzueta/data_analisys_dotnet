# Denominadores populacionais para o Brasil por UF, sexo e faixa etária quinquenal (1979–último ano), situação em nov/2026

> Notas de pesquisa (pt-BR). Cada afirmação tem fonte. Itens não confirmados estão marcados como **[NÃO CONFIRMADO]**.
> Faixas-alvo: 0-4, 5-9, …, 75-79, 80+; 27 UFs; masculino/feminino; anos 1979–2025.

## 1. Quais opções existem e o que cada uma cobre (anos, desagregação, formato, download)

### Takeaway
Não existe uma única série oficial que vá de 1979 até hoje por UF × sexo × idade. Na prática há duas séries do MS/DATASUS (as duas no TabNet), que cobrem tudo de 1980 a 2025 com as faixas quinquenais até 80+: a antiga "popbr", de 1980 a 2012, e a nova série RIPSA/MS 2024, de 2000 a 2025. As projeções do IBGE (revisão 2024) são a base estadual da série nova e começam em 2000. Para 1979 não há série alguma; o ano precisa ser estimado.

### Cited Findings

**A. DATASUS/TabNet "População residente – Censos (1980, 1991, 2000, 2010), Contagem (1996) e estimativas" (popbr.def), 1980–2012**
- TabNet `ibge/cnv/popbr.def`: população residente com período de 1980 a 2012, ano a ano. Pode ser tabulada por município, ano, sexo, situação, "Faixa Etária" e "Faixa Etária detalhada". — [TabNet popbr](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?ibge/cnv/popbr.def)
- Origem de cada ano, segundo a nota técnica: — [Nota técnica popbr (DATASUS)](http://tabnet.datasus.gov.br/cgi/ibge/popdescr.htm)
  - 1980: Censo 1980, com 3.991 municípios.
  - 1981–1990: estimativas do IBGE pelo método do próprio Instituto, já na divisão de 1991 (inclui Tocantins).
  - 1991: Censo.
  - 1992: estimativas do IBGE.
  - 1993–1995: o IBGE forneceu só os totais municipais, sem sexo e idade. O DATASUS aplicou a estrutura etária e de sexo de 1991, com arredondamentos.
  - 1996: Contagem.
  - 1997–1999: estimativas do IBGE por idade. O sexo só existe para 1998; nos outros anos foi aplicada uma razão de sexo.
  - 2000: Censo.
  - 2001–2006: estimativas do IBGE com a estrutura etária e de sexo do Censo 2000.
  - 2007–2009: projeto UNFPA/IBGE (BRA/4/P31A), com o método AiBi para os totais e a relação de coortes para a estrutura.
  - 2010: Censo.
  - 2011–2012: estimativas TCU distribuídas pela estrutura do Censo 2010.
- Faixas etárias, segundo a mesma nota: — [Nota técnica popbr](http://tabnet.datasus.gov.br/cgi/ibge/popdescr.htm)
  - Na "detalhada", o grupo 20–79 vem em faixas de 5 anos, seguido de 80+.
  - 5–19 vem em faixas de 5 anos em 1981–1990 e 1992–1995, e em idade simples nos demais anos.
  - Em 1981–1990 e 1992 só existe 0–4, sem a separação <1 / 1–4.
  - A categoria "idade ignorada" existe a partir de 1996, e em 1993–1995 para 20 municípios do PA (distribuídos 50/50 por sexo).
- Formato: arquivos dBase (.DBF) zipados por ano (POPBR_nn) no FTP do DATASUS, mais o pacote de tabulação popbr.zip. A nota adverte que o Excel trunca os arquivos por causa do limite de linhas. — [Nota técnica popbr](http://tabnet.datasus.gov.br/cgi/ibge/popdescr.htm)
- Cautela da própria nota: não comparar indicadores até 2006 com os de 2007 em diante. O índice de envelhecimento, por exemplo, cai de 2006 para 2007 por causa da mudança de método. — [Nota técnica popbr](http://tabnet.datasus.gov.br/cgi/ibge/popdescr.htm)
- A página "Dados Populacionais" do MS descreve o link "População residente e abrangência geográfica" (datasus.saude.gov.br/populacao-residente), onde ficam as séries antigas, como "População não mais em uso para indicadores". — [MS – Dados Populacionais](https://www.gov.br/saude/pt-br/composicao/seidigi/demas/dados-populacionais)

**B. Série RIPSA/MS "Estudo de Estimativas Populacionais por Município, Idade e Sexo" — versão atual 2000–2025 (popsvs2024br.def)**
- O título no TabNet é "População Residente – Estudo de Estimativas Populacionais por Município, Idade e Sexo 2000-2025 – Brasil". A série vai de 2000 a 2025 e pode ser tabulada por Região, UF (27), município, sexo, Faixa Etária 1, Faixa Etária 2 e idade simples. — [TabNet popsvs2024br](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?ibge/cnv/popsvs2024br.def)
- A "Faixa Etária 2" tem exatamente as faixas quinquenais 0–4, 5–9, …, 75–79 e "80 anos ou mais". A idade simples vai de <1 a 79 e depois 80+. A série não tem 85+ nem 90+. — [TabNet popsvs2024br](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?ibge/cnv/popsvs2024br.def)
- Nota técnica: RIPSA nº 01/2025, "População residente: estudo de estimativas populacionais para os municípios brasileiros, desagregadas por sexo e idade", DOI 10.5281/zenodo.17990183. — [NT RIPSA 01/2025 (PDF no TabNet)](http://tabnet.datasus.gov.br/cgi/IBGE/NT-Ripsa_01_2025_Estimativas_Populacionais.PDF)
- Método, segundo a NT RIPSA 01/2025: — [NT RIPSA 01/2025](http://tabnet.datasus.gov.br/cgi/IBGE/NT-Ripsa_01_2025_Estimativas_Populacionais.PDF)
  - Compatibiliza a malha municipal de 2000 até o último ano.
  - Ajusta os Censos 2000, 2010 e 2022 às estruturas estaduais por sexo e idade das Projeções IBGE revisão 2024 e aos totais municipais publicados no DOU.
  - Aplica a relação de coortes (Duchesne; Freire, Gonzaga e Gomes 2020) em dois níveis, RGInt→UF e município→RGInt.
  - Interpola os anos intermediários.
  - Garante que a soma dos municípios por sexo e idade seja igual aos totais estaduais do IBGE.
- A mesma NT diz que as estimativas "serão revisadas e atualizadas anualmente", seguindo os totais municipais que o IBGE divulga em agosto (DOU). Na atualização de 2025, foram usados os totais municipais de 2025 do DOU e a extensão das projeções estaduais para 2000–2025. — [NT RIPSA 01/2025](http://tabnet.datasus.gov.br/cgi/IBGE/NT-Ripsa_01_2025_Estimativas_Populacionais.PDF)
- Linha do tempo da série segundo a Fiocruz/OPGH: — [Nota técnica OPGH/Fiocruz nº 1/2025](https://observatoriohospitalar.fiocruz.br/sites/default/files/biblioteca/Nota%20t%C3%A9cnica%20n%C2%BA1_2025_%20OPGH%20atualiza%20o%20c%C3%A1lculo%20dos%20seus%20indicadores%20com%20base%20nas%20novas%20estimativas%20populacionais.pdf)
  - O grupo de trabalho MS/RIPSA/IBGE foi criado em 2012.
  - A 1ª versão saiu em 2014 (base: Projeções edição 2013, período 2000–2015).
  - A versão seguinte foi refeita com as Projeções edição 2018 (2000–2021).
  - A versão nova, feita com as Projeções edição 2024, foi divulgada em janeiro de 2025 para 2000–2024 e "passando a ser adotada no âmbito do Ministério da Saúde como padrão".
- O pacote R `brpop` (v0.7.0, publicado em 2026-07-29) separa a fonte "datasus" (antiga, 2000–2021) da "datasus2024" (nova, 2000–2024), ambas por município, sexo e faixa etária. A documentação avisa que as fontes "should not be treated as interchangeable" (não devem ser tratadas como intercambiáveis). — [brpop refman](https://ftp.fau.de/cran/web/packages/brpop/refman/brpop.html); [brpop – comparação de estimativas](https://rfsaldanha.github.io/brpop/articles/Population-estimates-comparison.html)
- Metodologia da série anterior (2000–2021), segundo o PROADESS: — [Nota Técnica PROADESS 2](https://www.proadess.icict.fiocruz.br/Nota%20Tecnica%20PROADESS_2_Estimativas%20populacionais.pdf) (via busca; PDF não aberto)
  - 2014–2015: regressão por município, sexo e idade.
  - 2016–2020: distribuição proporcional sobre os totais municipais do IBGE.

**C. IBGE – Projeções da População do Brasil e UFs, revisão 2024 (2000–2070)**
- O IBGE divulgou a revisão em 22/08/2024. Ela cobre 2000–2070, usa como base o Censo 2022 e traz estimativas e indicadores retrospectivos e prospectivos. — [FAPESPA Informativo 01/2024](https://fapespa.pa.gov.br/wp-content/uploads/2024/08/Nota-Informativa-01_2024-Projecao-Populacional-IBGE-2000-a-2070.pdf); [OPGH/Fiocruz NT 1/2025](https://observatoriohospitalar.fiocruz.br/sites/default/files/biblioteca/Nota%20t%C3%A9cnica%20n%C2%BA1_2025_%20OPGH%20atualiza%20o%20c%C3%A1lculo%20dos%20seus%20indicadores%20com%20base%20nas%20novas%20estimativas%20populacionais.pdf)
- Os anos 2000–2022 são tratados como estimativas e 2023–2070 como projeções, em 1º de julho, por UF, sexo e idade. A revisão faz conciliação demográfica para as 27 UFs. As tabelas saem em xlsx/ods com idade simples, grupos quinquenais e indicadores implícitos. — [IBGE nota metodológica (liv102111)](https://biblioteca.ibge.gov.br/visualizacao/livros/liv102111.pdf); [IBGE – Projeções (página)](https://www.ibge.gov.br/estatisticas/sociais/populacao/9109-projecao-da-populacao.html) (página retornou 403 na coleta; conteúdo obtido via resumo de busca)
- O TabNet espelha essas projeções em "Projeção da População das UFs por sexo, idade simples ou faixa-etária: 2000-2070 (edição 2024)", fonte IBGE: — [TabNet projpop2024uf](http://tabnet.datasus.gov.br/cgi/deftohtm.exe?ibge/cnv/projpop2024uf.def)
  - Idade simples de 0 até 90+.
  - "Faixa Etária 1" quinquenal de 0–4 até 90+.
  - "Faixa Etária 2" até 80+.
  - Nota técnica: `Ibge/Projpop_2000_2070_2024.pdf`.
- O IBGE recomenda usar sempre a revisão mais recente. — [IBGE nota metodológica](https://biblioteca.ibge.gov.br/visualizacao/livros/liv102111.pdf) (via resumo de busca)

**D. Revisões anteriores das projeções IBGE**
- Revisão 2018: tabelas de UF para 2010–2060, divulgadas em 2018. — [IBGE destaque projeção UF por sexo e idade simples](https://www.ibge.gov.br/novo-portal-destaques/27470-projecao-da-populacao-das-unidades-da-federacao-por-sexo-e-idade-simples.html) (via resumo de busca). Há conflito com outro resumo, que fala em "2000–2060" ([IBGE nota 2024](https://agenciadenoticias.ibge.gov.br/media/com_mediaibge/arquivos/9c05a15727b2454a8640369bf9c93c57.pdf)). **[NÃO CONFIRMADO qual dos dois]**
- Revisão 2013: primeira projeção de UF por sexo e idade pelo método das componentes, com horizonte 2030 para as UFs (tabelas 2000–2030). — [IBGE liv86444/liv33078](https://biblioteca.ibge.gov.br/visualizacao/livros/liv86444.pdf) (via resumo de busca)
- Revisão 2008: projeção nacional 1980–2050, com tabelas de UF até ~2031. Não está confirmado que haja tabela de UF por sexo e idade desde 1980. — [IBGE liv5590 / liv33078](https://biblioteca.ibge.gov.br/visualizacao/livros/liv5590.pdf) (via resumo de busca) **[NÃO CONFIRMADO]**

**E. Censos e estimativas IBGE (SIDRA)**
- Tabelas SIDRA usadas pelo brpop para os totais municipais: — [brpop refman](https://ftp.fau.de/cran/web/packages/brpop/refman/brpop.html)
  - 202: Censos 2000 e 2010.
  - 9514: Censo 2022.
  - 793: Contagem 2007.
  - 6579: estimativas anuais 2001–2021, atualizada em 2024-06-24.
  - FTP `Estimativas_de_Populacao/Estimativas_2024/` e `Estimativas_2025/`: 2024 e 2025. Os números de 2025 foram revisados e enviados ao TCU em 2026-01-13.
- As estimativas anuais do IBGE para o TCU/FPM não têm idade nem sexo. — [OPGH/Fiocruz NT 1/2025](https://observatoriohospitalar.fiocruz.br/sites/default/files/biblioteca/Nota%20t%C3%A9cnica%20n%C2%BA1_2025_%20OPGH%20atualiza%20o%20c%C3%A1lculo%20dos%20seus%20indicadores%20com%20base%20nas%20novas%20estimativas%20populacionais.pdf)

**F. UFRN/LEPP (Freire, Gonzaga, Queiroz)**
- Projeções municipais por sexo e idade para 2010–2030, feitas com estimadores bayesianos. Não cobrem anos anteriores a 2010. — [brpop refman](https://ftp.fau.de/cran/web/packages/brpop/refman/brpop.html)
- Os mesmos pesquisadores (Freire, Gonzaga) estão na equipe e na metodologia da NT RIPSA 01/2025. — [NT RIPSA 01/2025](http://tabnet.datasus.gov.br/cgi/IBGE/NT-Ripsa_01_2025_Estimativas_Populacionais.PDF)

### Inferences
- Para a desagregação pedida (UF × sexo × 0-4…75-79, 80+):
  - 2000–2025: popsvs2024br (Faixa Etária 2) serve diretamente.
  - 1980–1999: popbr (Faixa Etária detalhada) serve, com estes caveats:
    - 1981–1990 e 1992 só têm 0–4 (sem <1/1–4), o que não atrapalha o objetivo.
    - O sexo de 1997 e 1999 foi imputado por razão de sexo.
    - 1993–1995 replicam a estrutura de 1991.
- Para UF, o caminho mais simples é tabular no TabNet com Linha = UF, Coluna = Faixa Etária 2, seleção por Sexo e Ano, e exportar em CSV. Outra opção é baixar os DBF do FTP e agregar por UF. Os dois caminhos levam ao mesmo dado.
- As Projeções IBGE 2024 por UF (2000–2070) e a série RIPSA 2024 são consistentes no nível de UF por construção: a NT diz que a soma dos municípios é igual aos totais estaduais do IBGE. Para trabalho só por UF, as duas são equivalentes até 2024/2025, mas a versão TabNet RIPSA é a que o MS adota.

### Gaps
- Não confirmei o caminho exato do FTP dos DBF popbr. A nota cita ftp.datasus.gov.br, provavelmente `/dissemin/publicos/IBGE/POP/` **[NÃO CONFIRMADO]**. Também não confirmei se há download em lote da série RIPSA 2024 (CSV/DBF) além do TabNet.
- Não abri as planilhas do IBGE revisão 2024, então não verifiquei os nomes exatos dos arquivos nem se há tabela SIDRA com a revisão 2024.
- Não foi verificado se a série RIPSA já tem extensão para 2026. Em nov/2026 o TabNet mostra até 2025.

## 2. Qual série o MS/RIPSA recomenda hoje (IDB) e qual cobre 1979–1999

### Takeaway
O padrão oficial do MS desde janeiro de 2025 é a série RIPSA/MS 2024 (2000–2025, no TabNet `popsvs2024br`). As séries antigas são rotuladas como "não mais em uso para indicadores". Para 1980–1999, a única série pronta por UF × sexo × idade é a antiga popbr do DATASUS (de censos, contagem e estimativas IBGE), e nenhuma série oficial cobre 1979.

### Cited Findings
- O MS publica três conjuntos em "Dados Populacionais": a projeção por UF 2000–2070 (ed. 2024), a população TCU 2024–2025 e a "População municipal por sexo e idade 2000-2025" (popsvs2024br). A série popbr/antigas é marcada como "População não mais em uso para indicadores". — [MS – Dados Populacionais](https://www.gov.br/saude/pt-br/composicao/seidigi/demas/dados-populacionais)
- A série nova foi divulgada em jan/2025 e "passando a ser adotada no âmbito do Ministério da Saúde como padrão". — [OPGH/Fiocruz NT 1/2025](https://observatoriohospitalar.fiocruz.br/sites/default/files/biblioteca/Nota%20t%C3%A9cnica%20n%C2%BA1_2025_%20OPGH%20atualiza%20o%20c%C3%A1lculo%20dos%20seus%20indicadores%20com%20base%20nas%20novas%20estimativas%20populacionais.pdf)
- A SES-RS (Nota de Orientação 04/25) orienta adotar a base populacional da RIPSA (2000–2024) "sempre que possível, incluindo a atualização das séries históricas". As exceções são raça/cor, para a qual se usa o Censo IBGE, e diretrizes específicas do MS. O link indicado é `tabnet.datasus.gov.br/cgi/deftohtm.exe?ibge/cnv/popsvs2024br.def`. — [SES-RS Nota de Orientação 04/25](https://saude.rs.gov.br/upload/arquivos/202607/02141815-nota-de-orientacao-ripsa-1-1-1.pdf)
- Para 1980–1999, a popbr (1980–2012) é a série do DATASUS por município/UF, sexo e idade com essa cobertura. — [Nota técnica popbr](http://tabnet.datasus.gov.br/cgi/ibge/popdescr.htm)

### Inferences
- A ficha do indicador A.1 do IDB não foi lida diretamente. Pela cadeia MS→RIPSA→TabNet, é provável que os IDB atuais usem popsvs2024br **[NÃO CONFIRMADO na ficha IDB]**.
- 1979 não tem denominador oficial. Ver a seção 3 para as opções.

### Gaps
- Não localizei a ficha atual do IDB/RIPSA A.1 nem uma orientação oficial do MS sobre que série usar antes de 2000.

## 3. Existe série única 1979–hoje? Se não, como emendar, e o que fazem os estudos brasileiros

### Takeaway
Não existe série única. É preciso emendar popbr (1980–1999) com RIPSA 2024 (2000–2025) e estimar 1979. Os estudos brasileiros de tendência desde 1980 costumam interpolar entre censos e a contagem de 1996 (por exemplo, com Lagrange) ou usar as estimativas DATASUS como estão. Os pontos de quebra conhecidos são 1991, 1996, 2000 e a mudança de método de 2007; dentro da série RIPSA 2024, a quebra é 2022.

### Cited Findings
- Descontinuidades documentadas na popbr: — [Nota técnica popbr](http://tabnet.datasus.gov.br/cgi/ibge/popdescr.htm)
  - Mudança de método 2006→2007 ("não comparar").
  - 1993–1995 com a estrutura fixa de 1991.
  - Sexo de 1997 e 1999 imputado.
  - Anos censitários (1980, 1991, 2000, 2010) com contagens não ajustadas, entre anos estimados.
- A série RIPSA 2024 foi construída para garantir "compatibilidade metodológica" ao longo de 2000 até o último ano. Ela compatibiliza a malha municipal e reconstrói os totais passados com a metodologia mais recente, o que evita a quebra de 2007 da série antiga. — [NT RIPSA 01/2025](http://tabnet.datasus.gov.br/cgi/IBGE/NT-Ripsa_01_2025_Estimativas_Populacionais.PDF)
- O IPECE observa que o método antigo do DATASUS (estrutura etária do censo aplicada nos anos intercensitários) tendia a superestimar a população jovem, porque não capta o envelhecimento entre censos. — [IPECE NT 21](https://www.ipece.ce.gov.br/wp-content/uploads/sites/45/2012/12/NT_21.pdf) (via resumo de busca)
- Exemplo de prática acadêmica: um estudo de mortalidade cardiovascular de 1980–1999 calculou a população de 1º de julho dos anos intercensitários pelo método de Lagrange, sobre os Censos 1970, 1980, 1991 e 2000 e a Contagem 1996. — [Arq Bras Cardiol 2005 (PDF)](https://abccardiol.org/wp-content/uploads/2020/08/05_8505002.x66747.pdf) (via resumo de busca)
- Um artigo em Genus (2020) usou como denominador os censos IBGE de 1980, 1991 e 2000 e o DATASUS para 2019. — [Genus 2020, doi:10.1186/s41118-020-00105-3](https://doi.org/10.1186/s41118-020-00105-3) (via resumo de busca)
- O pacote brpop mostra séries de UF de fontes diferentes com linhas de referência em 2000, 2010 e 2022, o que evidencia os saltos entre fontes nos anos censitários. — [brpop – comparação](https://rfsaldanha.github.io/brpop/articles/Population-estimates-comparison.html)

### Inferences
- Emenda recomendada como hipótese de trabalho, a validar:
  - 1980–1999: popbr.
  - 2000–2025: popsvs2024br.
  - Na emenda de 2000 deve haver salto. O valor de 2000 da popbr é o Censo 2000 enumerado; o da RIPSA 2024 é a população de 2000 reconciliada pelas projeções 2024, que tipicamente incluem ajuste de cobertura. **[NÃO CONFIRMADO; quantificar comparando os dois valores de 2000 por UF/sexo/idade]**
- Para 1979, três opções:
  - (a) Extrapolar para trás a partir de 1980–1981 (geométrica por UF × sexo × idade).
  - (b) Interpolar entre o Censo 1970 e o Censo 1980. Exige obter o Censo 1970 por UF, sexo e idade no SIDRA ou no IBGE.
  - (c) Começar a análise em 1980 e registrar a decisão em ADR.
  - Em todos os casos, é preciso lembrar que Tocantins não existia antes de 1989. A popbr já realoca TO para 1981–1988, mas em 1980 os municípios de TO ficam em GO ([Nota técnica popbr](http://tabnet.datasus.gov.br/cgi/ibge/popdescr.htm): só realoca 1981–1988; para 1980 não está explícito) **[NÃO CONFIRMADO para 1980]**.
- Alternativa mais "limpa" para 1980–1999: interpolar entre os Censos 1980, 1991, 2000 e a Contagem 1996 (Lagrange ou exponencial por coorte), em vez de usar as estimativas IBGE embutidas na popbr. O custo é ter de implementar e documentar o método.

### Gaps
- Não encontrei revisão retroativa oficial do IBGE (pós-Censo 2022) por UF × sexo × idade para 1980–1999.
- Não quantifiquei o tamanho do salto em 2000 entre popbr e RIPSA 2024.

## 4. Como o GBD e outros estudos tratam a população das UFs brasileiras desde 1980

### Takeaway
As análises subnacionais do GBD para o Brasil começam em 1990 (não em 1980) e usam estimativas populacionais próprias do IHME. Não confirmei o detalhe metodológico das populações de UF do GBD.

### Cited Findings
- As análises subnacionais GBD-Brasil cobrem as 26 UFs e o DF a partir de 1990 (por exemplo, 1990–2017 para indicadores dos ODS; GBD 2016 com 1990–2016). — [Popul Health Metr 2020 (PMC7526114)](https://pmc.ncbi.nlm.nih.gov/articles/PMC7526114); [GBD Brazil 1990–2016 (Lancet 2018)](https://bohrium.dp.tech/paper/arxiv/39084048)
- Estudos GBD com UFs publicam taxas padronizadas para 1990 e 2019, sem 1980. — [PMC8959063](https://pmc.ncbi.nlm.nih.gov/articles/PMC8959063/table/t3)

### Inferences
- O GBD não resolve o problema 1979–1989 e usa populações modeladas próprias, não comparáveis às oficiais brasileiras. Não serve como denominador se o numerador for o SIM bruto.

### Gaps
- Não li o apêndice metodológico do GBD (população por UF: fontes, uso dos censos, ajustes). Fica pendente: Lancet 2018 doi:10.1016/S0140-6736(18)31221-2 e a documentação de métodos de população do IHME.
