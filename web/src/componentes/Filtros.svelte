<script lang="ts">
  import { MediaQuery } from 'svelte/reactivity';
  import { FAIXA_IGNORADA, type Dimensoes, type Filtro, type Local, type Metrica } from '../lib/tipos';

  let { dimensoes, filtro = $bindable() }: { dimensoes: Dimensoes; filtro: Filtro } = $props();

  const anos = $derived(dimensoes.anos.map(a => a.ano));
  const capitulos = $derived([...new Set(dimensoes.grupos.map(g => g.capitulo))]);
  const faixas = $derived(dimensoes.faixas.filter(f => f.indice !== FAIXA_IGNORADA));
  const metricas: { valor: Metrica; rotulo: string }[] = [
    { valor: 'obitos', rotulo: 'Óbitos' },
    { valor: 'bruta', rotulo: 'Taxa bruta' },
    { valor: 'padronizada', rotulo: 'Taxa padronizada' }
  ];
  const sexos: { valor: Filtro['sexo']; rotulo: string }[] = [
    { valor: null, rotulo: 'Ambos' },
    { valor: 1, rotulo: 'Masculino' },
    { valor: 2, rotulo: 'Feminino' }
  ];

  // O <select> de local guarda o recorte como texto ("brasil", "regiao:3", "uf:35").
  const codificar = (local: Local) => (local.tipo === 'brasil' ? 'brasil' : `${local.tipo}:${local.codigo}`);
  function decodificar(valor: string): Local {
    const [tipo, codigo] = valor.split(':');
    return tipo === 'regiao' || tipo === 'uf' ? { tipo, codigo: Number(codigo) } : { tipo: 'brasil' };
  }
  const opcional = (valor: string) => (valor === '' ? null : Number(valor));

  // Intervalo de faixas; o intervalo completo vira null, que também inclui os óbitos de idade ignorada.
  const ultimaFaixa = $derived(faixas.at(-1)!.indice);
  const faixaInicio = $derived(filtro.faixa?.inicio ?? 0);
  const faixaFim = $derived(filtro.faixa?.fim ?? ultimaFaixa);
  function definirFaixa(inicio: number, fim: number) {
    filtro.faixa = inicio === 0 && fim === ultimaFaixa ? null : { inicio, fim };
  }

  // No celular os filtros começam recolhidos, para o gráfico aparecer na primeira tela.
  const telaLarga = new MediaQuery('min-width: 768px');
  let aberto = $state(false);
  $effect(() => {
    aberto = telaLarga.current;
  });
</script>

<details bind:open={aberto}>
  <summary>Filtros</summary>
  <form class="filtros" onsubmit={e => e.preventDefault()}>
    <label>
      De
      <select bind:value={filtro.anoInicio}>
        {#each anos.filter(a => a <= filtro.anoFim) as ano (ano)}<option value={ano}>{ano}</option>{/each}
      </select>
    </label>
    <label>
      Até
      <select bind:value={filtro.anoFim}>
        {#each anos.filter(a => a >= filtro.anoInicio) as ano (ano)}<option value={ano}>{ano}</option>{/each}
      </select>
    </label>

    <label>
      Local
      <select value={codificar(filtro.local)} onchange={e => (filtro.local = decodificar(e.currentTarget.value))}>
        <option value="brasil">Brasil</option>
        <optgroup label="Regiões">
          {#each dimensoes.regioes as regiao (regiao.codigo)}<option value="regiao:{regiao.codigo}">{regiao.nome}</option>{/each}
        </optgroup>
        <optgroup label="Unidades da Federação">
          {#each [...dimensoes.ufs].sort((a, b) => a.sigla.localeCompare(b.sigla)) as uf (uf.codigo)}
            <option value="uf:{uf.codigo}">{uf.sigla}</option>
          {/each}
        </optgroup>
      </select>
    </label>

    <label class="largo">
      Causa
      <select value={filtro.grupo ?? ''} onchange={e => (filtro.grupo = opcional(e.currentTarget.value))}>
        <option value="">Todas as causas</option>
        {#each capitulos as capitulo (capitulo)}
          <optgroup label={capitulo}>
            {#each dimensoes.grupos.filter(g => g.capitulo === capitulo) as grupo (grupo.indice)}
              <option value={grupo.indice}>{grupo.nome}</option>
            {/each}
          </optgroup>
        {/each}
      </select>
    </label>

    <label>
      Idade de
      <select value={faixaInicio} onchange={e => definirFaixa(Number(e.currentTarget.value), faixaFim)}>
        {#each faixas.filter(f => f.indice <= faixaFim) as faixa (faixa.indice)}<option value={faixa.indice}>{faixa.rotulo}</option>{/each}
      </select>
    </label>
    <label>
      Até
      <select value={faixaFim} onchange={e => definirFaixa(faixaInicio, Number(e.currentTarget.value))}>
        {#each faixas.filter(f => f.indice >= faixaInicio) as faixa (faixa.indice)}<option value={faixa.indice}>{faixa.rotulo}</option>{/each}
      </select>
    </label>

    <fieldset>
      <legend>Sexo</legend>
      {#each sexos as sexo (sexo.rotulo)}
        <label class="opcao"><input type="radio" name="sexo" value={sexo.valor} checked={filtro.sexo === sexo.valor}
          onchange={() => (filtro.sexo = sexo.valor)} />{sexo.rotulo}</label>
      {/each}
    </fieldset>

    <fieldset>
      <legend>Métrica</legend>
      {#each metricas as metrica (metrica.valor)}
        <label class="opcao"><input type="radio" name="metrica" bind:group={filtro.metrica} value={metrica.valor} />{metrica.rotulo}</label>
      {/each}
    </fieldset>
  </form>
</details>

<style>
  summary {
    cursor: pointer;
    font-weight: 600;
  }
  details[open] summary {
    margin-bottom: 12px;
  }
  @media (min-width: 768px) {
    summary {
      display: none;
    }
  }
  .filtros {
    display: flex;
    flex-wrap: wrap;
    gap: 12px 16px;
    align-items: flex-end;
  }
  label,
  fieldset {
    display: flex;
    flex-direction: column;
    gap: 4px;
    font-size: 12px;
    color: var(--text-secondary);
  }
  .largo {
    max-width: 100%;
  }
  .largo select {
    width: 320px;
    max-width: 100%;
  }
  fieldset {
    flex-direction: row;
    border: 0;
    padding: 0;
    margin: 0;
    flex-wrap: wrap;
  }
  legend {
    width: 100%;
    margin-bottom: 4px;
  }
  .opcao {
    flex-direction: row;
    align-items: center;
    gap: 4px;
    padding: 6px 10px;
    border: 1px solid var(--border);
    border-radius: 6px;
    color: var(--text-primary);
    font-size: 13px;
    cursor: pointer;
  }
  .opcao:has(input:checked) {
    border-color: var(--acento);
    background: var(--selecao);
  }
  .opcao input {
    position: absolute;
    opacity: 0;
    pointer-events: none;
  }
  .opcao:has(input:focus-visible) {
    outline: 2px solid var(--acento);
    outline-offset: 2px;
  }
  select {
    font: inherit;
    font-size: 14px;
    color: var(--text-primary);
    background: var(--surface);
    border: 1px solid var(--border);
    border-radius: 6px;
    padding: 6px 8px;
  }
</style>
