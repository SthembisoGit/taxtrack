# TaxTrack Brand System v1

## Design Goal

Create a calm, trustworthy enterprise SaaS interface that communicates compliance status clearly without visual noise.

## Primary Brand Colors

| Token | Hex | Usage |
|---|---|---|
| `primary` | `#1E3A5F` | logo, primary buttons, navigation, key highlights |
| `secondary` | `#4A5D73` | secondary buttons, card accents, side panels |

## Status Colors

| Token | Hex | Semantic Meaning |
|---|---|---|
| `success` | `#2E7D32` | low risk, compliant, healthy |
| `warning` | `#F59E0B` | medium risk, caution |
| `danger` | `#D32F2F` | high risk, critical compliance concern |

## Neutral Colors

| Token | Hex | Usage |
|---|---|---|
| `bg` | `#F8FAFC` | page background |
| `card` | `#FFFFFF` | panel/card background |
| `border` | `#E5E7EB` | dividers and control outlines |
| `text-primary` | `#111827` | primary text |
| `text-secondary` | `#6B7280` | secondary text and helper text |

## Usage Rules

1. Use one dominant brand color (`primary`) for navigation and primary CTA actions.
2. Do not use status colors decoratively; use only for semantic states.
3. Keep dashboard surfaces neutral with `bg` and `card`.
4. Never hardcode hex values in UI components; always consume token keys.
5. API payloads must return semantic statuses (`Low`, `Medium`, `High`), not color values.
6. Enforce "no rainbow UI" in design and PR reviews.

## Dashboard Mapping

- Navigation bar: `primary`
- Card background: `card`
- Borders and separators: `border`
- Risk badges:
  - Low -> `success`
  - Medium -> `warning`
  - High -> `danger`

## Accessibility Requirements

1. Text/background combinations must meet WCAG AA contrast.
2. Status is communicated by both color and text label.
3. Alerts retain distinguishable states in grayscale and color-deficient views.

## Frontend Token Interfaces

```ts
export type ColorTokenName =
  | "primary"
  | "secondary"
  | "success"
  | "warning"
  | "danger"
  | "bg"
  | "card"
  | "border"
  | "text-primary"
  | "text-secondary";

export interface ThemeTokensV1 {
  tokenVersion: "v1";
  theme: "light";
  colors: Record<ColorTokenName, string>;
  semantic: {
    riskLow: "success";
    riskMedium: "warning";
    riskHigh: "danger";
  };
}
```
