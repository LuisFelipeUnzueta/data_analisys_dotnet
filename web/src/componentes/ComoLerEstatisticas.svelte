<script lang="ts">
  import { estimativaBruta, MINIMO_OBITOS_ESTAVEL, valorAjustado } from '../lib/estatistica';
  import { formatarCompacto, formatarInteiro, formatarPercentual, formatarValor } from '../lib/formato';
  import { base, eixoBase, linhaDeGrade, Plot } from '../lib/plot';
  import { novoAcumulador } from '../lib/taxas';
  import { tema } from '../lib/tema.svelte';
  import type { Metrica } from '../lib/tipos';
  import type { Vistas } from '../lib/vistas';
  import Grafico from './Grafico.svelte';

  // Explicações das estatísticas da página, com exemplos tirados do recorte filtrado.
  let { vistas, metrica, recorte }: { vistas: Vistas; metrica: Metrica; recorte: string } = $props();

  const taxa = (v: number) => formatarValor(v, 'bruta');
  const sinal = (v: number) => `${v > 0 ? '+' : ''}${formatarValor(v, 'bruta')}`;

  // 1. Intervalo de confiança: o leitor muda o número de óbitos e vê o intervalo de uma taxa de 100 por 100 mil.
  const TAXA_EXEMPLO = 100;
  const ESCALA_MAXIMA = 250;
  let expoente = $state(2);
  const obitosExemplo = $derived(Math.max(1, Math.round(10 ** expoente)));
  const exemplo = $derived.by(() => {
    const acc = novoAcumulador();
    acc.obitos = obitosExemplo;
    acc.populacao[0] = (obitosExemplo * 100_000) / TAXA_EXEMPLO;
    return estimativaBruta(acc)!;
  });
  const posicao = (v: number) => `${Math.min(100, (100 * v) / ESCALA_MAXIMA)}%`;
  const ultimoComIntervalo = $derived(
    [...vistas.serie].reverse().find(p => p.valor !== null && p.inferior !== undefined && p.inferior !== p.superior)
  );

  // 2. Tendência: os pontos do recorte e a reta ajustada, em escala normal ou logarítmica.
  let escalaLog = $state(false);
  const pontosTendencia = $derived(
    vistas.serie.filter((p): p is typeof p & { valor: number } => p.valor !== null && p.valor > 0)
  );
  const dezAnos = $derived(vistas.tendencia ? 100 * ((1 + vistas.tendencia.apc / 100) ** 10 - 1) : 0);
  const cruzaZero = $derived(vistas.tendencia !== null && vistas.tendencia.inferior < 0 && vistas.tendencia.superior > 0);

  function desenharTendencia(largura: number, altura: number) {
    const c = tema.cores;
    const t = vistas.tendencia!;
    const anos = pontosTendencia.map(p => p.ano);
    const reta = anos.map(ano => ({ ano, valor: valorAjustado(t, ano) }));
    // Na escala log o Plot omite rótulos entre potências de 10; ticks explícitos, com 2 algarismos, mantêm a leitura.
    const valores = pontosTendencia.map(p => p.valor);
    const [minimo, maximo] = [Math.min(...valores), Math.max(...valores)];
    const ticksLog = Array.from({ length: 5 }, (_, i) => Number((minimo * (maximo / minimo) ** (i / 4)).toPrecision(2)));
    return Plot.plot({
      ...base(largura, altura),
      x: { label: null, tickFormat: (a: number) => String(a), ticks: Math.floor(largura / 80) },
      y: {
        label: null, type: escalaLog ? 'log' : 'linear', zero: !escalaLog, tickFormat: formatarCompacto, grid: true,
        ...(escalaLog ? { ticks: ticksLog } : {})
      },
      marks: [
        Plot.dot(pontosTendencia, { x: 'ano', y: 'valor', r: 3, fill: c['series-1'], fillOpacity: 0.6 }),
        Plot.lineY(reta, { x: 'ano', y: 'valor', stroke: c['series-2'], strokeWidth: 2 })
      ]
    });
  }

  // 3. Excesso: anos-base destacados, reta ajustada neles e projetada para frente.
  const maiorExcesso = $derived(
    vistas.excesso ? vistas.excesso.porAno.reduce((m, a) => (a.observado - a.esperado > m.observado - m.esperado ? a : m)) : null
  );

  function desenharExcesso(largura: number, altura: number) {
    const c = tema.cores;
    const dados = vistas.excesso!;
    const esperado = [...dados.ajusteBase, ...dados.porAno].map(a => ({ ano: a.ano, esperado: a.esperado }));
    const ultimoBase = dados.ajusteBase.at(-1)?.ano ?? 2019;
    const primeiroBase = dados.ajusteBase[0]?.ano ?? 2015;
    return Plot.plot({
      ...base(largura, altura),
      x: { label: null, tickFormat: (a: number) => String(a), ticks: Math.min(esperado.length, Math.floor(largura / 60)) },
      y: { label: null, zero: true, tickFormat: formatarCompacto },
      marks: [
        Plot.rectX([{ x1: primeiroBase - 0.5, x2: ultimoBase + 0.5 }], { x1: 'x1', x2: 'x2', fill: c.grid, fillOpacity: 0.6 }),
        Plot.text([{ x: (primeiroBase + ultimoBase) / 2 }], { x: 'x', frameAnchor: 'top', text: () => 'anos-base', dy: 6, fill: c['text-secondary'] }),
        linhaDeGrade(),
        eixoBase(),
        Plot.lineY(esperado, { x: 'ano', y: 'esperado', stroke: c['series-2'], strokeWidth: 2 }),
        Plot.lineY(dados.observados, { x: 'ano', y: 'obitos', stroke: c['series-1'], strokeWidth: 2 }),
        Plot.dot(dados.observados, { x: 'ano', y: 'obitos', r: 3, fill: c['series-1'], stroke: c.surface, strokeWidth: 2 })
      ]
    });
  }
</script>

<h2>Como ler as estatísticas</h2>
<p class="intro">As explicações usam o recorte filtrado acima ({recorte}).</p>

<div class="secoes">
  <details open>
    <summary>Intervalo de confiança de 95%</summary>
    <p>
      Uma taxa calculada com poucos óbitos oscila muito por acaso: um ano com 3 mortes em vez de 6 dobra ou corta a
      taxa pela metade sem que o risco real tenha mudado. A faixa sombreada mostra essa margem: com 95% de confiança,
      a taxa verdadeira do recorte está dentro dela. Quanto mais óbitos, mais estreita a faixa.
    </p>

    <div class="simulador">
      <label>
        Óbitos no recorte: <strong>{formatarInteiro(obitosExemplo)}</strong>
        <input type="range" min="0" max="4" step="0.01" bind:value={expoente} aria-label="Número de óbitos (escala logarítmica)" />
      </label>
      <div class="regua" aria-hidden="true">
        <span class="intervalo" style:left={posicao(exemplo.inferior)} style:width="calc({posicao(exemplo.superior)} - {posicao(exemplo.inferior)})"></span>
        <span class="ponto" style:left={posicao(exemplo.valor)}></span>
      </div>
      <p class="leitura">
        Taxa de {taxa(exemplo.valor)} por 100 mil, com IC 95% de <strong>{taxa(exemplo.inferior)} a {taxa(exemplo.superior)}</strong>
        (±{formatarPercentual((100 * (exemplo.superior - exemplo.inferior)) / 2 / exemplo.valor)}).
        {#if exemplo.instavel}<strong>Estimativa instável</strong>: menos de {MINIMO_OBITOS_ESTAVEL} óbitos.{/if}
      </p>
    </div>

    {#if metrica === 'obitos'}
      <p class="nota">Na métrica "Óbitos" não há intervalo: é a contagem do registro. Escolha uma taxa para ver a faixa no gráfico.</p>
    {:else if ultimoComIntervalo}
      <p class="exemplo">
        <strong>No seu recorte:</strong> em {ultimoComIntervalo.ano} a taxa foi {taxa(ultimoComIntervalo.valor!)}; com 95% de confiança,
        o valor verdadeiro está entre {taxa(ultimoComIntervalo.inferior!)} e {taxa(ultimoComIntervalo.superior!)}.
        {#if ultimoComIntervalo.instavel}Esse ano é marcado como instável, então compare-o com cautela.{/if}
      </p>
    {/if}
    <p class="nota">
      Marcamos como instável a estimativa com menos de {MINIMO_OBITOS_ESTAVEL} óbitos ou cujo erro-padrão passa de 30% do
      valor (critério do NCHS, nos EUA). Taxa bruta: intervalo exato de Poisson. Taxa padronizada: método gama de Fay e Feuer.
    </p>
  </details>

  <details open>
    <summary>Tendência (variação percentual anual)</summary>
    <p>
      A tendência resume a série numa só taxa de mudança: quanto, em média, o valor sobe ou desce a cada ano no período
      escolhido. Ela vem de uma reta ajustada ao logaritmo dos valores, por isso é uma porcentagem constante. Na escala
      logarítmica, essa reta aparece reta.
    </p>
    {#if vistas.tendencia}
      <div class="alternar" role="group" aria-label="Escala do gráfico">
        <button type="button" aria-pressed={!escalaLog} onclick={() => (escalaLog = false)}>Escala normal</button>
        <button type="button" aria-pressed={escalaLog} onclick={() => (escalaLog = true)}>Escala logarítmica</button>
      </div>
      <Grafico desenhar={desenharTendencia} altura={200} />
      <p class="legenda-mini"><span class="ponto-azul"></span> valor de cada ano <span class="linha-laranja"></span> tendência ajustada</p>
      <p class="exemplo">
        <strong>No seu recorte:</strong> {sinal(vistas.tendencia.apc)}% ao ano quer dizer que o valor muda cerca de
        {sinal(vistas.tendencia.apc)}% a cada ano; em 10 anos, {sinal(dezAnos)}%.
        {#if cruzaZero}
          O intervalo ({sinal(vistas.tendencia.inferior)}% a {sinal(vistas.tendencia.superior)}%) cruza o zero: não dá para afirmar
          que houve alta ou queda.
        {:else}
          O intervalo ({sinal(vistas.tendencia.inferior)}% a {sinal(vistas.tendencia.superior)}%) não cruza o zero: a
          {vistas.tendencia.apc < 0 ? 'queda' : 'alta'} é consistente, não efeito do acaso.
        {/if}
      </p>
    {:else}
      <p class="nota">Neste recorte a maioria dos anos tem poucos óbitos e a tendência não é calculada.</p>
    {/if}
    <p class="nota">
      Uma tendência média esconde mudanças de direção dentro do período (por exemplo, a covid em 2020–2021); escolha
      períodos mais curtos para comparar fases.
    </p>
  </details>

  <details open>
    <summary>Excesso de óbitos</summary>
    <p>
      O excesso compara as mortes que aconteceram com as que seriam esperadas se a tendência de 2015 a 2019 tivesse
      continuado. O esperado é calculado por faixa etária, com a população de cada ano, então já desconta o
      crescimento e o envelhecimento da população. A diferença é o excesso.
    </p>
    {#if vistas.excesso && maiorExcesso}
      <Grafico desenhar={desenharExcesso} altura={200} />
      <p class="legenda-mini"><span class="linha-azul"></span> óbitos observados <span class="linha-laranja"></span> esperados: ajustados nos anos-base e projetados depois</p>
      <p class="exemplo">
        <strong>No seu recorte:</strong> o maior excesso foi em {maiorExcesso.ano}, com
        {formatarInteiro(Math.round(maiorExcesso.observado - maiorExcesso.esperado))} óbitos a mais que os
        {formatarInteiro(Math.round(maiorExcesso.esperado))} esperados
        ({formatarPercentual((100 * (maiorExcesso.observado - maiorExcesso.esperado)) / maiorExcesso.esperado)}).
      </p>
    {:else}
      <p class="nota">O recorte não tem óbitos suficientes nos anos-base para estimar o esperado.</p>
    {/if}
    <p class="nota">
      O intervalo do excesso inclui a incerteza da projeção e a variação natural entre anos (modelo quasi-Poisson).
      Um excesso pequeno, cujo intervalo inclui o zero, pode ser só oscilação.
    </p>
  </details>
</div>

<style>
  h2 {
    font-size: 16px;
    margin: 0;
    color: var(--text-primary);
  }
  .intro,
  .nota,
  .legenda-mini {
    font-size: 13px;
    color: var(--text-secondary);
  }
  .intro {
    margin: 2px 0 12px;
  }
  .secoes {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 340px), 1fr));
    gap: 12px;
  }
  details {
    border: 1px solid var(--border);
    border-radius: 6px;
    padding: 12px;
  }
  summary {
    cursor: pointer;
    font-weight: 600;
    color: var(--text-primary);
  }
  p {
    font-size: 14px;
    color: var(--text-secondary);
    margin: 8px 0;
  }
  .exemplo,
  .leitura {
    color: var(--text-primary);
  }
  .simulador label {
    display: flex;
    flex-direction: column;
    gap: 4px;
    font-size: 13px;
    color: var(--text-secondary);
  }
  input[type='range'] {
    width: 100%;
    accent-color: var(--series-1);
  }
  .regua {
    position: relative;
    height: 16px;
    margin: 8px 0;
    border-bottom: 1px solid var(--muted);
  }
  .intervalo {
    position: absolute;
    top: 4px;
    height: 8px;
    background: var(--series-1);
    opacity: 0.3;
    border-radius: 4px;
  }
  .ponto {
    position: absolute;
    top: 2px;
    width: 12px;
    height: 12px;
    margin-left: -6px;
    border-radius: 50%;
    background: var(--series-1);
  }
  .alternar {
    display: flex;
    gap: 6px;
    margin: 8px 0;
  }
  .alternar button {
    font: inherit;
    font-size: 13px;
    padding: 4px 10px;
    border: 1px solid var(--border);
    border-radius: 6px;
    background: var(--surface);
    color: var(--text-primary);
    cursor: pointer;
  }
  .alternar button[aria-pressed='true'] {
    border-color: var(--series-1);
    background: var(--selecao);
  }
  .legenda-mini {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 4px 8px;
    margin: 4px 0;
    font-size: 12px;
  }
  .ponto-azul,
  .linha-azul,
  .linha-laranja {
    display: inline-block;
  }
  .ponto-azul {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: var(--series-1);
  }
  .linha-azul,
  .linha-laranja {
    width: 16px;
    height: 2px;
    background: var(--series-1);
  }
  .linha-laranja {
    background: var(--series-2);
  }
</style>
