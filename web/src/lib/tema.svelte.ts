/**
 * Cores dos gráficos, lidas dos tokens CSS de `app.css`. O Plot desenha SVG com cores literais,
 * então os gráficos leem estes valores e são redesenhados quando o tema do sistema muda.
 */
const TOKENS = ['surface', 'text-primary', 'text-secondary', 'muted', 'grid', 'series-1', 'series-2', 'seq-min', 'seq-max'] as const;

type Token = (typeof TOKENS)[number];

function ler(): Record<Token, string> {
  const estilo = getComputedStyle(document.documentElement);
  return Object.fromEntries(TOKENS.map(t => [t, estilo.getPropertyValue(`--${t}`).trim()])) as Record<Token, string>;
}

export const tema = $state({ cores: ler() });

matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
  tema.cores = ler();
});
