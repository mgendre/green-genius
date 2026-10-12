---
name: scss-styling
description: "How to write styles here — Tailwind first, tokens in @theme, 7-1 Sass classes composed with @apply, mixins for repeated fragments, Spartan UI themed through our tokens. Read before writing or reviewing any SCSS or class list."
---

# SCSS styling

Tokens: `@theme` in `styles/vendors/_tailwind.scss`.

## Structure

```
styles/main.scss              @use every partial
styles/abstracts/_mixins.scss shared fragments (focus-ring, surface, …), no CSS output
styles/vendors/_tailwind.scss @import "tailwindcss", Spartan preset, fonts, @theme tokens
styles/base/                  reset, typography (headings defined once here)
styles/layout/                header, shell, navigation
styles/components/            one partial per component family (.btn, .panel, .form-input…)
styles/pages/                 page-specific rules only
```

- `@use` / `@forward` only. Partial over ~150 lines → split. New partial → register in `main.scss`.
- A plain-CSS package (`.css` file) is imported with its extension (`@import 'pkg/index.css'`); without it Sass tries to parse the file and fails.

## Tokens

- Single source: `@theme` in `_tailwind.scss`. SCSS reads `var(--color-primary)`. No hex, rgb, named colour or px radius anywhere else. `color-mix()` stays allowed.
- One token per role, no shade ladders. Soft backgrounds and borders: `color-mix(in srgb, var(--color-x) 8%, transparent)`, never a new token. A new token only when two components need the same new role, added to `@theme` in the same change.
- Utilities that carry no size are free: `flex`, `flex-col`, `flex-wrap`, `flex-1`, `grid`, `items-center`, `justify-between`, `justify-end`, `grow`, `min-w-0`, `ml-auto`, `w-full`.
- Sized spacing, scale of four: `--spacing-xs` 4px, `--spacing-sm` 8px, `--spacing-md` 16px, `--spacing-lg` 32px. Only `gap-sm`, `p-md`, `mt-lg`… are allowed; never `gap-3`, `p-4` or an arbitrary value, in a template or in SCSS.

## Classes, not utility piles

- Layout and appearance both live in named classes in the right 7-1 partial, composed with `@apply` from Tailwind utilities: `.app-main-menu { @apply flex items-baseline gap-lg bg-primary px-lg py-sm; }`. A template carries class names by role.
- More than two utilities on an element means a named class.
- Tokens are Tailwind theme entries, so `bg-primary`, `text-muted-foreground`, `border-border`, `rounded-base`, `shadow-sm` exist for free.
- A raw CSS property only when Tailwind has no utility for it (`content`, a pseudo-element).
- Classes: base + variant + size (`.btn .btn-primary .btn-sm`), named by role not look, states via Tailwind variants inside `@apply` (`hover:`, `focus-visible:`, `disabled:`). Desktop-first: base rule for desktop, then `max-md:` variants; no SCSS breakpoint mixin, Tailwind owns the breakpoints.
- Repeated fragments (surface + border + radius, focus ring) → a mixin in `abstracts/_mixins.scss` made of `@apply` lines, included by each class. Two classes sharing three utilities is the trigger.
- Component `.scss` files are the exception: only for a rule meaningless outside that one component (a keyframe for its own animation) and under ~15 lines. Host display goes in metadata: `host: { class: 'block' }`.
- **Delete test**: remove the rule; if nothing visible changes, it stays deleted.

## Flex and spacing

Spacing is owned by the parent, with one rule per role. The sized steps are `xs` 4px, `sm` 8px, `md` 16px, `lg` 32px (`p-sm` is Tailwind's `p-2`, `p-md` its `p-4`: write ours).

| Where | Utility | Why |
|---|---|---|
| Space between siblings in a row or column | `gap-sm` (tight: badges, icon + text), `gap-md` (cards, form fields), `gap-lg` (page sections) | `gap` on the flex parent, so no child needs a margin |
| Inside a card, panel, dialog | `p-md` (compact variant: `p-sm`) | One inner padding per surface, the same on every card |
| Inside a chip, badge, button | `px-sm py-xs` | Horizontal wider than vertical |
| Page or shell edge | `p-lg` desktop, `max-md:p-md` mobile | The page breathes more than a card |

- **No margin on a child** to push it away from a sibling: put `gap-*` on the parent. A margin only for one-off separation from the parent edge, or `ml-auto` to push to the end.
- **Same nesting, same step**: a card in a page uses `p-md`, what it holds uses `gap-sm`. Never the same step at two levels of nesting (`p-md` inside `p-md`); go one step down.
- **Row**: `flex gap-md`. **Column**: `flex flex-col gap-md`. **Centered**: `items-center`. **Space out**: `justify-between`.
- **Equal columns**: `flex gap-md` on the parent and `flex-1` on each child (`.parent > * { @apply flex-1 min-w-0; }`). `min-w-0` lets text and tables shrink instead of overflowing.
- **Many items of unknown count** (badges, tags): `flex flex-wrap gap-sm`. A fixed set of cards stacks with `max-md:flex-col`, never `flex-wrap`.
- **Mobile**: desktop first, then `max-md:flex-col` and `max-md:p-md`; the gap stays the same.

```scss
.plant-identity {
  @apply flex gap-md max-md:flex-col;

  > * {
    @apply flex-1 min-w-0;
  }
}

.section-card {
  @apply flex flex-col gap-sm p-md;
}
```

## Before writing any rule, in this order

1. An existing class in `styles/` already does it → use it.
2. An existing class almost does it → add a modifier (`.panel-compact`, `.badge-warning`), not a new class.
3. Two existing classes overlap → merge them into one base with modifiers, update the templates, then add yours.
4. Nothing fits → new class in the right 7-1 partial, named by role.

Never skip to 4. Report every merge done in step 3.

## Spartan UI and native widgets

- Spartan's semantic tokens (`--color-background`, `--color-primary-foreground`, `--color-ring`…) are mapped onto ours in `styles/vendors/_tailwind.scss` only, each value a `var(--…)` or a `color-mix()` of them, never a literal.
- Colour a Spartan component with its semantic classes (`bg-primary text-primary-foreground`), never a raw palette value. `class` on a Spartan element is for placement only; to change its look, change a token or edit the generated Helm file.
- `max-w-xs`, `max-w-sm`, `max-w-md`, `max-w-lg` read `--max-width-xs` … `--max-width-lg` in `@theme`. Without `--max-width-sm`, `max-w-sm` falls back to `--spacing-sm` and is 8px wide: a dialog a few pixels wide means that token is missing. The name is `--max-width-sm`, never `--max-w-sm`.
- Never `::ng-deep`. The Helm code in `shared/spartan/helm/` is created by `@spartan-ng/cli`, never written from scratch; once generated it is ours to adapt.
- Platform first when Spartan adds nothing: style `details`, `summary` and the native inputs directly.
