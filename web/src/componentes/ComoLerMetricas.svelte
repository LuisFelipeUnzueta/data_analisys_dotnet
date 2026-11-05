<script lang="ts">
  import { formatarValor } from '../lib/formato';
  import { DESCRICOES } from '../lib/metricas';
  import type { Dimensoes, Metrica } from '../lib/tipos';

  let { dimensoes, selecionada }: { dimensoes: Dimensoes; selecionada: Metrica } = $props();

  // O exemplo usa as taxas nacionais calculadas pelo ETL, então acompanha cada atualização dos dados.
  const primeiro = $derived(dimensoes.anos[0]!);
  const ultimo = $derived(dimensoes.anos.at(-1)!);
  const taxa = (valor: number | null) => formatarValor(valor, 'bruta');
  const metricas = Object.keys(DESCRICOES) as Metrica[];
</script>

<dl>
  {#each metricas as metrica (metrica)}
    <div class:ativa={metrica === selecionada}>
      <dt>{DESCRICOES[metrica].titulo}</dt>
      <dd>
        <strong>{DESCRICOES[metrica].resumo}</strong>
        {DESCRICOES[metrica].detalhe}
      </dd>
    </div>
  {/each}
</dl>
<p class="exemplo">
  <strong>Por que as duas taxas contam histórias diferentes:</strong> no Brasil, a taxa bruta foi de
  {taxa(primeiro.taxa_bruta_brasil)} em {primeiro.ano} e de {taxa(ultimo.taxa_bruta_brasil)} em {ultimo.ano} por 100 mil
  habitantes, enquanto a padronizada caiu de {taxa(primeiro.taxa_padronizada_brasil)} para
  {taxa(ultimo.taxa_padronizada_brasil)}. O risco de morrer em cada idade diminuiu bastante, mas a população envelheceu,
  e mais pessoas em idades de maior risco seguram a taxa bruta em alta.
</p>

<style>
  dl {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 300px), 1fr));
    gap: 12px;
    margin: 0;
  }
  dl > div {
    padding: 12px;
    border: 1px solid var(--border);
    border-radius: 6px;
  }
  dl > div.ativa {
    border-color: var(--acento);
    background: var(--selecao);
  }
  dt {
    font-weight: 600;
    color: var(--text-primary);
    margin-bottom: 4px;
  }
  dd {
    margin: 0;
    font-size: 14px;
    color: var(--text-secondary);
  }
  dd strong {
    display: block;
    color: var(--text-primary);
    font-weight: 500;
    margin-bottom: 4px;
  }
  .exemplo {
    margin: 12px 0 0;
    font-size: 14px;
    color: var(--text-secondary);
  }
  .exemplo strong {
    color: var(--text-primary);
  }
</style>
