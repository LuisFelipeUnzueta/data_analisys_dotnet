<script lang="ts">
  import Filtros from './componentes/Filtros.svelte';
  import LinhaTempo from './componentes/LinhaTempo.svelte';
  import Mapa from './componentes/Mapa.svelte';
  import Piramide from './componentes/Piramide.svelte';
  import Ranking from './componentes/Ranking.svelte';
  import { agregar } from './lib/agregacao';
  import { avisosDoRecorte } from './lib/avisos';
  import { carregarBase } from './lib/dados';
  import { formatarCompacto, formatarInteiro, formatarPercentual, formatarValor, rotuloFaixas, rotuloMetrica } from './lib/formato';
  import { pesosDoPadrao } from './lib/taxas';
  import type { Base, Filtro } from './lib/tipos';
  import { criarVistas } from './lib/vistas';

  let base = $state.raw<Base | null>(null);
  let erro = $state<string | null>(null);
  let filtro = $state<Filtro>({
    anoInicio: 1979,
    anoFim: 1979,
    local: { tipo: 'brasil' },
    grupo: null,
    sexo: null,
    faixa: null,
    metrica: 'padronizada'
  });

  carregarBase()
    .then(b => {
      filtro.anoInicio = b.dimensoes.anos[0]!.ano;
      filtro.anoFim = b.dimensoes.anos.at(-1)!.ano;
      base = b;
    })
    .catch(e => (erro = `Não foi possível carregar os dados: ${e}`));

  const pesos = $derived(base ? pesosDoPadrao(base.dimensoes) : null);
  const indiceMalDefinidas = $derived(base?.dimensoes.grupos.find(g => g.codigo === 'mal_definidas')?.indice ?? -1);
  const vistas = $derived(base && pesos ? criarVistas(agregar(base, filtro, indiceMalDefinidas), filtro, base.dimensoes, pesos) : null);
  const avisos = $derived(base ? avisosDoRecorte(filtro, base.dimensoes) : []);
  const metrica = $derived(rotuloMetrica(filtro));
  const formatar = (v: number) => formatarValor(v, filtro.metrica);
  const siglas = $derived(new Map(base?.dimensoes.ufs.map(u => [u.codigo, u.sigla]) ?? []));

  const nomeLocal = $derived.by(() => {
    const local = filtro.local;
    if (!base || local.tipo === 'brasil') return 'Brasil';
    return local.tipo === 'uf' ? siglas.get(local.codigo)! : base.dimensoes.regioes.find(r => r.codigo === local.codigo)!.nome;
  });
  const nomeIdade = $derived(base ? rotuloFaixas(base.dimensoes.faixas, filtro.faixa) : '');
  const nomeCausa = $derived(filtro.grupo === null || !base ? 'todas as causas' : base.dimensoes.grupos[filtro.grupo]!.nome);
  const marcos = $derived(
    (base?.dimensoes.marcos ?? [])
      .filter(m => m.ano > filtro.anoInicio && m.ano <= filtro.anoFim && (m.ano === 1996 || avisos.some(a => a.includes('Tocantins'))))
      .map(m => ({ ano: m.ano, texto: m.ano === 1996 ? 'CID-9 → CID-10' : 'TO separado de GO' }))
  );
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
  {:else if !base || !vistas}
    <p class="carregando">Carregando dados…</p>
  {:else}
    <section class="painel filtros">
      <Filtros dimensoes={base.dimensoes} bind:filtro />
    </section>

    <p class="resumo">
      <strong>{formatarCompacto(vistas.totalObitos)}</strong> óbitos por {nomeCausa} em {nomeLocal}, {nomeIdade},
      {filtro.anoInicio}–{filtro.anoFim}.
    </p>

    {#if avisos.length > 0}
      <ul class="avisos" aria-label="Avisos metodológicos">
        {#each avisos as aviso (aviso)}<li><span aria-hidden="true">ⓘ</span> {aviso}</li>{/each}
      </ul>
    {/if}

    <section class="painel">
      <h2>{metrica} por ano</h2>
      <p class="contexto">{nomeCausa} · {nomeLocal} · {nomeIdade}</p>
      <LinhaTempo pontos={vistas.serie} {formatar} {marcos} rotuloColuna={metrica} />
    </section>

    <div class="grade">
      <section class="painel">
        <h2>{metrica} por UF</h2>
        <p class="contexto">{nomeIdade} · {filtro.anoInicio}–{filtro.anoFim} · clique numa UF para selecioná-la</p>
        <Mapa
          geo={base.geo}
          valores={vistas.porUf}
          {siglas}
          selecionada={filtro.local.tipo === 'uf' ? filtro.local.codigo : null}
          {formatar}
          rotuloMetrica={metrica}
          aoSelecionar={uf => (filtro.local = { tipo: 'uf', codigo: uf })}
        />
      </section>

      <section class="painel">
        <h2>Pirâmide etária em {filtro.anoFim}</h2>
        <p class="contexto">
          {filtro.metrica === 'obitos' ? 'Óbitos' : 'Taxa específica por 100 mil hab.'} por sexo e faixa · {nomeCausa} · {nomeLocal}
        </p>
        <Piramide
          linhas={vistas.piramide}
          formatar={v => formatarValor(v, filtro.metrica === 'obitos' ? 'obitos' : 'bruta')}
          rotuloMetrica={filtro.metrica === 'obitos' ? 'Óbitos' : 'Taxa por 100 mil hab.'}
        />
        {#if vistas.idadeIgnoradaNaPiramide > 0}
          <p class="nota">{formatarInteiro(vistas.idadeIgnoradaNaPiramide)} óbitos com idade ignorada não aparecem na pirâmide.</p>
        {/if}
      </section>
    </div>

    <div class="grade">
      <section class="painel">
        <h2>Principais causas</h2>
        <p class="contexto">{metrica} · {nomeLocal} · {nomeIdade} · {filtro.anoInicio}–{filtro.anoFim} · clique para selecionar</p>
        <Ranking
          itens={vistas.ranking}
          selecionado={filtro.grupo}
          {formatar}
          rotuloMetrica={metrica}
          aoSelecionar={g => (filtro.grupo = filtro.grupo === g ? null : g)}
        />
      </section>

      <section class="painel">
        <h2>Causas mal definidas</h2>
        <p class="contexto">% dos óbitos de {nomeLocal} com causa mal definida · indicador de qualidade do registro</p>
        <LinhaTempo pontos={vistas.qualidade} formatar={formatarPercentual} formatarEixo={v => `${v}%`} altura={220} rotuloColuna="% mal definidas" />
      </section>
    </div>

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
  h2 {
    font-size: 16px;
    margin: 0;
    color: var(--text-primary);
  }
  .sub,
  .contexto,
  .nota,
  footer {
    color: var(--text-secondary);
  }
  .sub {
    margin: 0 0 16px;
  }
  .contexto {
    font-size: 13px;
    margin: 2px 0 12px;
  }
  .nota {
    font-size: 12px;
    margin: 8px 0 0;
  }
  .painel {
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 8px;
    padding: 16px;
    margin-bottom: 16px;
  }
  .grade {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 480px), 1fr));
    gap: 0 16px;
  }
  .resumo {
    margin: 0 0 12px;
    color: var(--text-secondary);
  }
  .resumo strong {
    color: var(--text-primary);
    font-size: 20px;
  }
  .avisos {
    list-style: none;
    padding: 12px 16px;
    margin: 0 0 16px;
    border-left: 3px solid var(--muted);
    background: var(--surface);
    color: var(--text-primary);
    font-size: 14px;
  }
  .avisos li + li {
    margin-top: 6px;
  }
  .erro,
  .carregando {
    color: var(--text-secondary);
  }
  footer {
    font-size: 12px;
    margin-top: 24px;
  }
</style>
