<script lang="ts">
  // Monta uma figura do Plot e a redesenha quando os dados lidos por `desenhar` ou a largura mudam.
  let { desenhar, altura = 280 }: {
    desenhar: (largura: number, altura: number) => SVGSVGElement | HTMLElement;
    altura?: number;
  } = $props();

  let alvo: HTMLDivElement;
  let largura = $state(0);

  $effect(() => {
    if (largura === 0) return;
    const figura = desenhar(largura, altura);
    alvo.replaceChildren(figura);
  });
</script>

<div class="grafico" bind:this={alvo} bind:clientWidth={largura} style:min-height="{altura}px"></div>

<style>
  .grafico {
    width: 100%;
  }
</style>
