import tokenSet from '../../../docs/design/tokens-v1.json';

type ColorTokenName =
  | 'primary'
  | 'secondary'
  | 'success'
  | 'warning'
  | 'danger'
  | 'bg'
  | 'card'
  | 'border'
  | 'text-primary'
  | 'text-secondary';

interface ThemeTokensV1 {
  tokenVersion: 'v1';
  theme: 'light';
  colors: Record<ColorTokenName, string>;
  semantic: {
    riskLow: 'success';
    riskMedium: 'warning';
    riskHigh: 'danger';
  };
}

export const themeTokens = tokenSet as ThemeTokensV1;

export function applyThemeTokens() {
  const root = document.documentElement;

  for (const [name, value] of Object.entries(themeTokens.colors)) {
    root.style.setProperty(`--color-${name}`, value);
  }
}
