/**
 * Funções estatísticas da página: quantis, intervalos de confiança de taxas, tendência (APC) e excesso de mortalidade.
 * Todas trabalham sobre os acumuladores por faixa etária de `taxas.ts`, então valem para qualquer recorte filtrado.
 */
import { BASE_TAXA, type Acumulador } from './taxas';
import { FAIXA_IGNORADA, NUM_FAIXAS, type Metrica } from './tipos';

/** Quantil 97,5% da normal padrão (IC de 95%). */
const Z = 1.959963984540054;

/** Abaixo disso, ou com erro-padrão relativo acima de 30%, a estimativa é marcada como instável (critério NCHS). */
export const MINIMO_OBITOS_ESTAVEL = 20;
export const ERRO_RELATIVO_MAXIMO = 0.3;

/**
 * Quantil do qui-quadrado com k graus de liberdade (k pode ser fracionário) pela aproximação de Wilson–Hilferty.
 * `z` é o quantil normal correspondente (−Z para 2,5%, +Z para 97,5%).
 */
export function quantilQuiQuadrado(k: number, z: number): number {
  if (k <= 0) return 0;
  const c = 2 / (9 * k);
  return Math.max(0, k * (1 - c + z * Math.sqrt(c)) ** 3);
}

/** Quantil 97,5% da t de Student por expansão de Cornish–Fisher (boa a partir de ~5 graus de liberdade). */
export function quantilT(graus: number): number {
  const z3 = Z ** 3;
  const z5 = Z ** 5;
  return Z + (z3 + Z) / (4 * graus) + (5 * z5 + 16 * z3 + 3 * Z) / (96 * graus ** 2);
}

export interface Estimativa {
  valor: number;
  inferior: number;
  superior: number;
  /** Menos de 20 óbitos ou erro-padrão relativo acima de 30%. */
  instavel: boolean;
  /** Variância de ln(valor), usada como peso na regressão de tendência. */
  varianciaLog: number;
}

/** Taxa bruta com IC95% exato de Poisson: [χ²(2d; 2,5%)/2, χ²(2d+2; 97,5%)/2] / P. */
export function estimativaBruta(acc: Acumulador): Estimativa | null {
  let populacao = 0;
  for (const p of acc.populacao) populacao += p;
  if (populacao <= 0) return null;

  const d = acc.obitos;
  const escala = BASE_TAXA / populacao;
  return {
    valor: d * escala,
    inferior: d === 0 ? 0 : (quantilQuiQuadrado(2 * d, -Z) / 2) * escala,
    superior: (quantilQuiQuadrado(2 * d + 2, Z) / 2) * escala,
    instavel: d < MINIMO_OBITOS_ESTAVEL,
    varianciaLog: d > 0 ? 1 / d : Infinity
  };
}

/**
 * Taxa padronizada (método direto) com IC95% pelo método gama de Fay e Feuer (1997), o mesmo do SEER:
 * y = Σ wᵢ·dᵢ/Pᵢ, v = Σ wᵢ²·dᵢ/Pᵢ², com wᵢ os pesos normalizados e w_M = máx(wᵢ/Pᵢ).
 */
export function estimativaPadronizada(acc: Acumulador, pesos: Float64Array): Estimativa | null {
  let somaPesos = 0;
  for (let f = 0; f < NUM_FAIXAS; f++) if (f !== FAIXA_IGNORADA) somaPesos += pesos[f]!;
  if (somaPesos === 0) return null;

  let y = 0;
  let v = 0;
  let wM = 0;
  let obitos = 0;
  for (let f = 0; f < NUM_FAIXAS; f++) {
    const w = pesos[f]! / somaPesos;
    if (f === FAIXA_IGNORADA || w === 0) continue;
    const populacao = acc.populacao[f]!;
    if (populacao <= 0) return null;
    const d = acc.ajustados[f]!;
    y += (w * d) / populacao;
    v += (w * w * d) / (populacao * populacao);
    wM = Math.max(wM, w / populacao);
    obitos += d;
  }

  const inferior = y === 0 ? 0 : (v / (2 * y)) * quantilQuiQuadrado((2 * y * y) / v, -Z);
  const superior = ((v + wM * wM) / (2 * (y + wM))) * quantilQuiQuadrado((2 * (y + wM) ** 2) / (v + wM * wM), Z);
  const erroRelativo = y > 0 ? Math.sqrt(v) / y : Infinity;
  return {
    valor: y * BASE_TAXA,
    inferior: inferior * BASE_TAXA,
    superior: superior * BASE_TAXA,
    instavel: obitos < MINIMO_OBITOS_ESTAVEL || erroRelativo > ERRO_RELATIVO_MAXIMO,
    varianciaLog: y > 0 ? v / (y * y) : Infinity
  };
}

/** Estimativa da métrica do filtro. Óbitos são contagem do registro, sem intervalo (inferior = superior = valor). */
export function estimativaMetrica(acc: Acumulador, metrica: Metrica, pesos: Float64Array): Estimativa | null {
  if (metrica === 'bruta') return estimativaBruta(acc);
  if (metrica === 'padronizada') return estimativaPadronizada(acc, pesos);
  const d = acc.obitos;
  return { valor: d, inferior: d, superior: d, instavel: false, varianciaLog: d > 0 ? 1 / d : Infinity };
}

export interface Tendencia {
  /** Variação percentual anual (APC) e seu IC95%. */
  apc: number;
  inferior: number;
  superior: number;
  anos: number;
  /** Reta ajustada em escala log: ln(taxa) = mediaLog + inclinacao·(ano − mediaAno). */
  inclinacao: number;
  mediaAno: number;
  mediaLog: number;
}

/** Valor da reta de tendência num ano (escala original). */
export const valorAjustado = (t: Tendencia, ano: number) => Math.exp(t.mediaLog + t.inclinacao * (ano - t.mediaAno));

/**
 * APC por regressão log-linear ponderada: ln(taxa) = a + b·ano, pesos 1/Var(ln taxa), APC = 100·(eᵇ − 1).
 * O erro-padrão usa a variância residual (absorve a sobredispersão entre anos) e o quantil t com n − 2 graus.
 */
export function tendencia(pontos: { ano: number; valor: number; varianciaLog: number }[]): Tendencia | null {
  const validos = pontos.filter(p => p.valor > 0 && Number.isFinite(p.varianciaLog) && p.varianciaLog > 0);
  if (validos.length < 3) return null;

  let sw = 0, sx = 0, sy = 0;
  for (const p of validos) {
    const w = 1 / p.varianciaLog;
    sw += w;
    sx += w * p.ano;
    sy += w * Math.log(p.valor);
  }
  const mx = sx / sw;
  const my = sy / sw;

  let sxx = 0, sxy = 0;
  for (const p of validos) {
    const w = 1 / p.varianciaLog;
    sxx += w * (p.ano - mx) ** 2;
    sxy += w * (p.ano - mx) * (Math.log(p.valor) - my);
  }
  const b = sxy / sxx;

  let residuo = 0;
  for (const p of validos) residuo += (Math.log(p.valor) - (my + b * (p.ano - mx))) ** 2 / p.varianciaLog;
  const graus = validos.length - 2;
  const erro = Math.sqrt(residuo / graus / sxx);
  const t = quantilT(graus);
  const percentual = (x: number) => 100 * (Math.exp(x) - 1);

  return {
    apc: percentual(b), inferior: percentual(b - t * erro), superior: percentual(b + t * erro), anos: validos.length,
    inclinacao: b, mediaAno: mx, mediaLog: my
  };
}

export interface ExcessoAno {
  ano: number;
  observado: number;
  esperado: number;
  /** IC95% do excesso (observado − esperado). */
  inferior: number;
  superior: number;
}

/**
 * Excesso de mortalidade: em cada faixa etária, uma regressão quasi-Poisson com tendência linear sobre os anos-base
 * (log μ = log P + a + b·t) projeta os óbitos esperados nos anos-alvo. Excesso = observado − Σ esperado; a variância
 * soma a do esperado (φ·E) e a da projeção (método delta com a covariância de a e b).
 */
export function excesso(
  serie: (ano: number) => Acumulador | undefined, anosBase: number[], anosAlvo: number[]
): ExcessoAno[] | null {
  const base = anosBase.map(serie);
  if (base.some(a => a === undefined)) return null;
  const centro = anosBase.reduce((s, a) => s + a, 0) / anosBase.length;

  const ajustes: ReturnType<typeof ajustarPoisson>[] = [];
  for (let f = 0; f < NUM_FAIXAS; f++) {
    if (f === FAIXA_IGNORADA) { ajustes.push(null); continue; }
    const pontos = anosBase.map((ano, i) => ({ t: ano - centro, d: base[i]!.ajustados[f]!, p: base[i]!.populacao[f]! }));
    ajustes.push(ajustarPoisson(pontos));
  }

  return anosAlvo.flatMap(ano => {
    const alvo = serie(ano);
    if (!alvo) return [];
    const t = ano - centro;
    let esperado = 0;
    let variancia = 0;
    ajustes.forEach((ajuste, f) => {
      if (!ajuste) return;
      const e = alvo.populacao[f]! * Math.exp(ajuste.a + ajuste.b * t);
      const [vaa, vab, vbb] = ajuste.cov;
      esperado += e;
      variancia += ajuste.dispersao * e + e * e * (vaa + 2 * t * vab + t * t * vbb);
    });
    const diferenca = alvo.obitos - esperado;
    const margem = Z * Math.sqrt(variancia);
    return [{ ano, observado: alvo.obitos, esperado, inferior: diferenca - margem, superior: diferenca + margem }];
  });
}

/** Poisson com offset por IRLS (Newton): devolve a, b e a covariância (var a, cov ab, var b); null sem óbitos na base. */
function ajustarPoisson(pontos: { t: number; d: number; p: number }[]) {
  const obitos = pontos.reduce((s, x) => s + x.d, 0);
  const populacao = pontos.reduce((s, x) => s + x.p, 0);
  if (obitos <= 0 || populacao <= 0 || pontos.some(x => x.p <= 0)) return null;

  let a = Math.log(obitos / populacao);
  let b = 0;
  let info: [number, number, number] = [0, 0, 0];
  for (let iteracao = 0; iteracao < 50; iteracao++) {
    let g0 = 0, g1 = 0, i00 = 0, i01 = 0, i11 = 0;
    for (const { t, d, p } of pontos) {
      const mu = p * Math.exp(a + b * t);
      g0 += d - mu;
      g1 += (d - mu) * t;
      i00 += mu;
      i01 += mu * t;
      i11 += mu * t * t;
    }
    const det = i00 * i11 - i01 * i01;
    const da = (i11 * g0 - i01 * g1) / det;
    const db = (i00 * g1 - i01 * g0) / det;
    a += da;
    b += db;
    info = [i00, i01, i11];
    if (Math.abs(da) + Math.abs(db) < 1e-10) break;
  }

  // Quasi-Poisson: a dispersão φ = Σ(d − μ)²/μ / (n − 2), nunca abaixo de 1, alarga a covariância e a variância do
  // esperado. Sem isso o IC supõe variação puramente de Poisson, estreita demais para séries anuais de mortalidade.
  let pearson = 0;
  for (const { t, d, p } of pontos) {
    const mu = p * Math.exp(a + b * t);
    pearson += (d - mu) ** 2 / mu;
  }
  const dispersao = pontos.length > 2 ? Math.max(1, pearson / (pontos.length - 2)) : 1;

  const [i00, i01, i11] = info;
  const det = i00 * i11 - i01 * i01;
  return { a, b, dispersao, cov: [dispersao * i11 / det, -dispersao * i01 / det, dispersao * i00 / det] as [number, number, number] };
}
