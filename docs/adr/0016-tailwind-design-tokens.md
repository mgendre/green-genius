# 0016. Tailwind design tokens

Date: 2026-10-16

## Status

Accepted

## Context

Colours and spacing must stay consistent across screens and change in one place.

## Decision

- Styling is Tailwind 4 with SCSS 7-1 in `src/styles/`.
- Design tokens are declared in `@theme` in `src/styles/vendors/_tailwind.scss`.
- Colour is applied through semantic classes (`bg-primary`, `text-muted-foreground`), never a raw value.

## Consequences

- A theme change is a token change.
- A raw colour in a template or a stylesheet is a defect.
