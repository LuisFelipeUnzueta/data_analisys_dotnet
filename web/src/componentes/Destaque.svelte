<script lang="ts">
  import { formatarVariacao } from '../lib/formato';
  import type { Vistas } from '../lib/vistas';

  // O achado do recorte em três números, acima dos filtros.
  let { destaque, metrica, formatar }: {
    destaque: NonNullable<Vistas['destaque']>;
    metrica: string;
    formatar: (valor: number) => string;
  } = $props();
</script>

<dl aria-label={metrica}>
  <div>
    <dt>{destaque.inicio.ano}</dt>
    <dd>{formatar(destaque.inicio.valor)}</dd>
  </div>
  <div>
    <dt>{destaque.fim.ano}</dt>
    <dd>{formatar(destaque.fim.valor)}</dd>
  </div>
  <div class="variacao">
    <dt>Variação</dt>
    <dd>{formatarVariacao(destaque.variacao)}</dd>
  </div>
  <p>{metrica}</p>
</dl>

<style>
  dl {
    display: flex;
    flex-wrap: wrap;
    align-items: baseline;
    gap: 4px 28px;
    margin: 0 0 16px;
  }
  dt {
    font-size: 12px;
    color: var(--text-secondary);
  }
  dd {
    margin: 0;
    font-size: 28px;
    font-weight: 600;
    line-height: 1.2;
  }
  .variacao dd {
    color: var(--acento);
  }
  p {
    width: 100%;
    margin: 0;
    font-size: 12px;
    color: var(--text-secondary);
  }
</style>
