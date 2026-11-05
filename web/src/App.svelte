<script lang="ts">
  import BarraRecorte from './componentes/BarraRecorte.svelte';
  import ComoLerEstatisticas from './componentes/ComoLerEstatisticas.svelte';
  import ComoLerMetricas from './componentes/ComoLerMetricas.svelte';
  import Destaque from './componentes/Destaque.svelte';
  import Excesso from './componentes/Excesso.svelte';
  import Filtros from './componentes/Filtros.svelte';
  import LinhaTempo from './componentes/LinhaTempo.svelte';
  import Mapa from './componentes/Mapa.svelte';
  import Painel from './componentes/Painel.svelte';
  import Piramide from './componentes/Piramide.svelte';
  import Ranking from './componentes/Ranking.svelte';
  import Recolhivel from './componentes/Recolhivel.svelte';
  import { agregar } from './lib/agregacao';
  import { avisosDoRecorte, marcosDoRecorte } from './lib/avisos';
  import { carregarBase } from './lib/dados';
  import { formatarInteiro, formatarPercentual, formatarTendencia, formatarValor, nomeMetrica, rotuloMetrica, rotulosDoRecorte } from './lib/formato';
  import { pesosDoPadrao } from './lib/taxas';
  import type { Base, Filtro, Local } from './lib/tipos';
  import { DESCRICOES } from './lib/metricas';
  import { filtroDaUrl, filtroParaUrl } from './lib/url';
  import { criarVistas, MINIMO_ANOS_BASE } from './lib/vistas';

  const BASE_COVID = { inicio: 2015, fim: 2019 };
  const BRASIL: Local = { tipo: 'brasil' };

  let base = $state.raw<Base | null>(null);
  let erro = $state<string | null>(null);
  let filtro = $state<Filtro>({ anoInicio: 0, anoFim: 0, local: BRASIL, grupo: null, sexo: null, faixa: null, metrica: 'padronizada' });

  carregarBase()
    .then(b => {
      filtro = filtroDaUrl(location.search, b.dimensoes);
      base = b;
    })
    .catch(e => (erro = `Não foi possível carregar os dados: ${e}`));

  // A URL acompanha o recorte, para o link da página reabrir a mesma visão.
  $effect(() => {
    if (base) history.replaceState(null, '', `${location.pathname}${filtroParaUrl(filtro, base.dimensoes)}${location.hash}`);
  });

  const pesos = $derived(base ? pesosDoPadrao(base.dimensoes) : null);
  const indiceMalDefinidas = $derived(base?.dimensoes.grupos.find(g => g.codigo === 'mal_definidas')?.indice ?? -1);
  const vistas = $derived(base && pesos ? criarVistas(agregar(base, filtro, indiceMalDefinidas), filtro, base.dimensoes, pesos) : null);
  const avisos = $derived(base ? avisosDoRecorte(filtro, base.dimensoes) : []);
  const metrica = $derived(rotuloMetrica(filtro));
  const nome = $derived(nomeMetrica(filtro));
  const unidade = $derived(filtro.metrica === 'obitos' ? '' : 'por 100 mil hab. · ');
  const formatar = (v: number) => formatarValor(v, filtro.metrica);
  const siglas = $derived(new Map(base?.dimensoes.ufs.map(u => [u.codigo, u.sigla]) ?? []));
  const rotulos = $derived(base ? rotulosDoRecorte(filtro, base.dimensoes) : null);
  const marcos = $derived(base ? marcosDoRecorte(filtro, base.dimensoes).map(m => ({ ano: m.ano, texto: m.rotulo })) : []);
  const periodo = $derived(`${filtro.anoInicio}–${filtro.anoFim}`);

  const alternarUf = (uf: number) => {
    filtro.local = filtro.local.tipo === 'uf' && filtro.local.codigo === uf ? BRASIL : { tipo: 'uf', codigo: uf };
  };
  const usarBaseCovid = () => {
    filtro.anoInicio = BASE_COVID.inicio;
    filtro.anoFim = BASE_COVID.fim;
  };
</script>

<main>
  <header>
    <h1>Mortalidade no Brasil</h1>
    <p class="sub">
      Óbitos registrados no SIM por UF de residência, causa harmonizada CID-9/CID-10, sexo e idade, desde 1979.
    </p>
  </header>

  {#if erro}
    <p class="erro">{erro}</p>
  {:else if !base || !vistas || !rotulos}
    <div class="esqueleto" role="status" aria-label="Carregando dados">
      <div></div>
      <div class="alto"></div>
      <div class="alto"></div>
    </div>
  {:else}
    {#if vistas.destaque}<Destaque destaque={vistas.destaque} {metrica} {formatar} />{/if}

    <Painel>
      <Filtros dimensoes={base.dimensoes} bind:filtro />
    </Painel>

    <BarraRecorte dimensoes={base.dimensoes} bind:filtro totalObitos={vistas.totalObitos} />

    {#if avisos.length > 0}
      <ul class="avisos" aria-label="Avisos metodológicos">
        {#each avisos as aviso (aviso)}<li><span aria-hidden="true">ⓘ</span> {aviso}</li>{/each}
      </ul>
    {/if}

    <Painel titulo="{nome} por ano" contexto="{unidade}{rotulos.contexto}">
      <p class="descricao">{DESCRICOES[filtro.metrica].resumo} <a href="#como-ler">Como ler as métricas</a></p>
      {#if vistas.tendencia}
        <p class="tendencia">
          <strong>Tendência {periodo}:</strong> {formatarTendencia(vistas.tendencia)}
          <span class="nota-tendencia">
            regressão log-linear ponderada; varia conforme o período escolhido ·
            <a href="#como-ler-estatisticas">o que é a faixa, a tendência e o excesso</a>
          </span>
        </p>
      {:else}
        <p class="nota-tendencia">Tendência não calculada: a maioria dos anos do recorte tem poucos óbitos para uma estimativa estável.</p>
      {/if}
      <LinhaTempo pontos={vistas.serie} {formatar} {marcos} rotuloColuna={metrica} />
    </Painel>

    {#if vistas.excesso}
      <Painel
        titulo="Excesso de óbitos após {filtro.anoFim}"
        contexto="{rotulos.contexto} · esperado pela tendência de {periodo} (o período escolhido) em cada faixa etária, com a população de cada ano (regressão de Poisson com sobredispersão)"
      >
        <p class="nota-tendencia"><a href="#como-ler-estatisticas">Como ler o excesso</a></p>
        <Excesso dados={vistas.excesso} />
      </Painel>
    {:else}
      <Painel
        titulo="Excesso de óbitos"
        contexto="O período escolhido é a base do esperado, e o excesso é medido nos anos seguintes. Para calculá-lo, escolha um período com pelo menos {MINIMO_ANOS_BASE} anos que termine antes de {base.dimensoes.anos.at(-1)!.ano}."
      >
        <button class="acao" onclick={usarBaseCovid}>Ver o excesso da covid (base {BASE_COVID.inicio}–{BASE_COVID.fim})</button>
      </Painel>
    {/if}

    <div class="grade">
      <Painel titulo="{nome} por UF" contexto="{unidade}{rotulos.idade} · {periodo} · clique numa UF para selecioná-la">
        <Mapa
          geo={base.geo}
          valores={vistas.porUf}
          {siglas}
          selecionada={filtro.local.tipo === 'uf' ? filtro.local.codigo : null}
          {formatar}
          rotuloMetrica={metrica}
          aoSelecionar={alternarUf}
        />
      </Painel>

      <Painel
        titulo="Pirâmide etária em {filtro.anoFim}"
        contexto="{filtro.metrica === 'obitos' ? 'Óbitos' : 'Taxa específica por 100 mil hab.'} por sexo e faixa · {rotulos.causa} · {rotulos.local}"
      >
        <Piramide
          linhas={vistas.piramide}
          formatar={v => formatarValor(v, filtro.metrica === 'obitos' ? 'obitos' : 'bruta')}
          rotuloMetrica={filtro.metrica === 'obitos' ? 'Óbitos' : 'Taxa por 100 mil hab.'}
        />
        {#if vistas.idadeIgnoradaNaPiramide > 0}
          <p class="nota">{formatarInteiro(vistas.idadeIgnoradaNaPiramide)} óbitos com idade ignorada não aparecem na pirâmide.</p>
        {/if}
      </Painel>
    </div>

    <div class="grade">
      <Painel titulo="Principais causas" contexto="{metrica} · {rotulos.local} · {rotulos.idade} · {periodo} · clique para selecionar">
        <Ranking
          itens={vistas.ranking}
          selecionado={filtro.grupo}
          {formatar}
          rotuloMetrica={metrica}
          aoSelecionar={g => (filtro.grupo = filtro.grupo === g ? null : g)}
        />
      </Painel>

      <Painel titulo="Causas mal definidas" contexto="% dos óbitos com causa mal definida · {rotulos.local} · indicador de qualidade do registro">
        <LinhaTempo pontos={vistas.qualidade} formatar={formatarPercentual} formatarEixo={v => `${v}%`} altura={220} rotuloColuna="% mal definidas" />
      </Painel>
    </div>

    <Recolhivel id="como-ler" titulo="Como ler as métricas">
      <ComoLerMetricas dimensoes={base.dimensoes} selecionada={filtro.metrica} />
    </Recolhivel>

    <Recolhivel id="como-ler-estatisticas" titulo="Como ler as estatísticas">
      <ComoLerEstatisticas {vistas} metrica={filtro.metrica} recorte={rotulos.contexto} />
    </Recolhivel>

    <footer>
      <p>
        Fontes: Ministério da Saúde, Sistema de Informações sobre Mortalidade (SIM/OpenDATASUS), 1979–{base.dimensoes.anos.at(-1)!.ano};
        população: IBGE (Censos 1980 e 1991, Contagem 1996) e estimativas RIPSA 2024, com interpolação de 1981–1999.
        Taxa padronizada pelo método direto com a {base.dimensoes.padrao.nome}. Totais anuais conferidos com o Tabnet.
      </p>
    </footer>
  {/if}
</main>

<style>
  main {
    max-width: 1200px;
    margin: 0 auto;
    padding: 24px 16px 48px;
  }
  h1 {
    font-size: 28px;
    margin: 0 0 4px;
    color: var(--text-primary);
  }
  .sub,
  .nota,
  footer {
    color: var(--text-secondary);
  }
  .sub {
    margin: 0 0 16px;
  }
  .nota {
    font-size: 12px;
    margin: 8px 0 0;
  }
  .descricao {
    font-size: 14px;
    color: var(--text-secondary);
    margin: 0 0 12px;
  }
  .tendencia {
    font-size: 14px;
    color: var(--text-primary);
    margin: 0 0 12px;
  }
  .nota-tendencia {
    display: block;
    font-size: 12px;
    color: var(--text-secondary);
  }
  .acao {
    font: inherit;
    font-size: 14px;
    padding: 6px 12px;
    color: var(--text-primary);
    background: var(--selecao);
    border: 1px solid var(--acento);
    border-radius: 6px;
    cursor: pointer;
  }
  .acao:focus-visible {
    outline: 2px solid var(--acento);
    outline-offset: 2px;
  }
  .grade {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 480px), 1fr));
    gap: 0 16px;
  }
  .avisos {
    list-style: none;
    padding: 12px 16px;
    margin: 0 0 16px;
    border-left: 3px solid var(--aviso);
    background: var(--surface);
    color: var(--text-primary);
    font-size: 14px;
  }
  .avisos span {
    color: var(--aviso);
  }
  .avisos li + li {
    margin-top: 6px;
  }
  .erro {
    color: var(--text-secondary);
  }
  .esqueleto div {
    height: 72px;
    margin-bottom: 16px;
    border-radius: 8px;
    background: var(--surface);
    border: 1px solid var(--border);
  }
  .esqueleto .alto {
    height: 320px;
  }
  @media (prefers-reduced-motion: no-preference) {
    .esqueleto div {
      animation: pulsar 1.4s ease-in-out infinite alternate;
    }
  }
  @keyframes pulsar {
    to {
      opacity: 0.5;
    }
  }
  footer {
    font-size: 12px;
    margin-top: 24px;
  }
</style>
