---
name: ui-quality
description: "What makes a screen correct for a person, beyond the code compiling — hierarchy, grouping, the four states of a data view, forms, actions, responsive, accessibility, consistency. Read before building or reviewing any page, card, list or form, and when writing the criteria of a frontend task."
---

# UI quality

Spacing, flex and tokens are in `scss-styling`; loading of one item in `loading-state`; texts in `i18n-fr`. This skill is the part those do not cover: is the screen understandable and complete.

## Decide first, in this order

1. **One job per screen**: the one thing the person comes to do or read. Its title says it. Everything else is secondary.
2. **One primary action** per screen or card (`hlmBtn` default variant). Others are `outline` or `ghost`. Two primary buttons side by side is a decision not taken.
3. **Reuse before new**: `shared/components/` (`section-card`, `plant-identity`), the classes of `styles/components/`, a Spartan component. A new piece only when none fits.

## Hierarchy and grouping

- Hierarchy comes from weight and colour, not from size alone: the important text `font-medium` or `font-semibold` in `text-foreground`, the secondary one `text-muted-foreground`.
- Group by proximity before drawing a line: related things share a `gap-sm`, unrelated blocks a `gap-md` or `gap-lg`. A border or a divider only when spacing is not enough.
- A label and its value stay side by side or stacked, never far apart. Values the data may lack show `common.not-set`, never an empty cell.
- Numbers with units: value and unit together (`20 cm`, through a parametrised text), `tabular-nums` in a column of numbers.

## The four states of a data view

Every list, sheet or card that loads data shows each of these, with its own French text in `fr.json`:

| State | Shows |
|---|---|
| Loading | a short text or a skeleton, never an empty frame |
| Empty | what is missing and what to do: `Aucun jardin pour le moment`, then the add button |
| Error | what failed in a sentence and a way to retry, never a raw message |
| Content | the data |

A view that handles only content is not finished. Check all four in the template before reporting.

## Forms

- A visible label on every field (`hlm-field` with its label), never a placeholder alone.
- The error under the field it belongs to, in a full sentence that says what to do; submit disabled while the form is invalid or saving.
- The first field has focus when a dialog opens; Enter submits; Escape closes.
- A destructive action asks for confirmation in an alert dialog and names what is deleted: `Supprimer le jardin « Potager » ?`.
- Right type and `autocomplete` on native inputs.

## Actions

- A link goes somewhere (`routerLink`), a button does something (`<button>`). Never a clickable `div`.
- A verb for an action (`Enregistrer`), the object for a title. The same word for the same action everywhere: do not mix `Supprimer` and `Effacer`.
- An icon alone has an `aria-label` from `fr.json`. An icon next to a text is `aria-hidden="true"`.
- A click target is at least 40 px high: Spartan sizes already do it; a bare icon in a custom button needs `p-sm`.

## Responsive

- Desktop first, then `max-md:`. On a phone: one column, no horizontal scroll, the primary action reachable without scrolling a long form.
- A wide table becomes a list of cards under `max-md`, not a scrolling table.
- Long text: `truncate` with a `title`, or wraps; never overflows its card (`min-w-0` on flex children).
- A line of text stays under about 70 characters: `max-w-md` on prose.

## Accessibility

- Colour never carries a meaning alone: a status has an icon or a text too (the badges do).
- Keep the focus ring: never `outline-none` without `focus-visible:` replacement.
- Contrast comes from the tokens; a hand-picked grey is a token to add, not a value.
- Headings in order (one `h1` per page, then `h2`); a landmark per region (`nav`, `main`).
- A dynamic message (error, saved) is in an `aria-live="polite"` region or a Spartan toast.

## Before reporting a UI task

1. The four states exist, each with its text in `fr.json`.
2. One primary action, in its place; every icon-only button labelled.
3. Nothing in the template is a hand-written colour, size or margin: classes by role.
4. The mobile layout is described by a `max-md:` rule, not left to chance.
5. What only the running app shows (overlap, focus order, real widths) goes under `not verified`.

## Never

- A screen that only works with data present.
- A decorative element (shadow, gradient, animation) that carries no information.
- A new colour, spacing step or component style to fix one screen: change the token or the class that already exists.
