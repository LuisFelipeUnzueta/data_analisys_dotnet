<script lang="ts">
  // Versão em tabela de cada gráfico: todo valor fica acessível sem depender de hover ou de cor.
  let { colunas, linhas }: { colunas: string[]; linhas: (string | number)[][] } = $props();
</script>

<details>
  <summary>Ver dados em tabela</summary>
  <div class="rolagem">
    <table>
      <thead>
        <tr>{#each colunas as coluna (coluna)}<th scope="col">{coluna}</th>{/each}</tr>
      </thead>
      <tbody>
        {#each linhas as linha, i (i)}
          <tr>{#each linha as celula, j (j)}<td>{celula}</td>{/each}</tr>
        {/each}
      </tbody>
    </table>
  </div>
</details>

<style>
  details {
    margin-top: 8px;
    font-size: 13px;
  }
  summary {
    cursor: pointer;
    color: var(--text-secondary);
  }
  .rolagem {
    max-height: 280px;
    overflow: auto;
    margin-top: 8px;
  }
  table {
    border-collapse: collapse;
    width: 100%;
    font-variant-numeric: tabular-nums;
  }
  th,
  td {
    padding: 4px 8px;
    border-bottom: 1px solid var(--grid);
    text-align: right;
  }
  th:first-child,
  td:first-child {
    text-align: left;
  }
  th {
    position: sticky;
    top: 0;
    background: var(--surface);
    color: var(--text-secondary);
    font-weight: 600;
  }
</style>
