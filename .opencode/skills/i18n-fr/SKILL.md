---
name: i18n-fr
description: "How to add or change a French text in public/i18n/fr.json — key naming, alphabetical order, wording rules for labels, buttons, errors and plurals. Read before touching fr.json or writing any user-facing text, even for a single key."
---

# French i18n

All user-facing text lives in `GreenGenius.Frontend/public/i18n/fr.json`. You write the French texts yourself, in the same change as the template that uses them.

## Steps

1. Read `public/i18n/fr.json`.
2. List the keys the template uses that the file lacks. A key that already exists keeps its value unless the task asks to reword it.
3. Add each key anywhere in the file, with no comment and no trailing comma, then run `node GreenGenius.Frontend/scripts/sortkeys.mjs` from the repository root. It sorts `fr.json` in place and prints one line, `sortkeys: OK, …`. Never sort by hand and never print the keys to sort them. A `FAILED` line names duplicate keys: remove them and run it again.
4. Every key used with `| translate` must exist in the file, letter for letter: the check tells.

## Keys

- Flat, lowercase, dotted: `<feature>.<screen or object>.<element>.<role>`, as `gardens.form.name.label`, `gardens.form.name.required`, `gardens.list.add`.
- Words inside a segment are joined with `-`: `gardens.form.name.too-long`.
- A text used by several features goes under `common.`: `common.actions.cancel`. Look for an existing `common.` key before adding one that says the same thing.

## Wording

- Natural, short French for a web interface. No technical term, no English word.
- Buttons and actions: a verb in the infinitive, `Enregistrer`, `Annuler`, `Ajouter un jardin`.
- Labels: a noun with a capital letter, no final colon: `Nom`.
- Titles: `Nouveau jardin`, `Modifier le jardin`.
- Errors: a full sentence that says what to do, `Saisissez un nom.`, `Le nom ne peut pas dépasser 250 caractères.`
- Empty states: `Aucun jardin pour le moment`.
- Typography: a real ellipsis `…`, an apostrophe `'`, accents on capitals (`À`, `É`).
- A count never uses `(s)`. Write two keys and choose in the template: `gardens.count.one` → `{{count}} jardin`, `gardens.count.other` → `{{count}} jardins`.
- A value in a text is a parameter: `"gardens.count.other": "{{count}} jardins"`, used as `{{ 'gardens.count.other' | translate: { count: gardenCount() } }}`.

## Enum labels, units, yes and no

A value of an enum (family, life cycle, level), a unit and a yes or no are texts: they live in `fr.json` like any other.

- One `Record<Enum, string>` per enum, in the component file, whose values are the **complete key literals**: `[PlantLifeCycle.Annual]: 'plants.life-cycle.annual'`. Never `'plants.life-cycle.' + value`: the check reads literals to find missing and unused keys.
- The template translates the key: `{{ lifeCycleKey() | translate }}` where `lifeCycleKey` is one `computed` reading the record. Several enums need several records, not several copies of the same `computed` logic: a function `labelKey(record, value)` is enough.
- A unit is part of a parametrised text: `"plants.unit.cm": "{{value}} cm"`, used as `{{ 'plants.unit.cm' | translate: { value: p.needs.heightCm } }}`; days `"plants.unit.days": "{{value}} j"`.
- Yes and no are `common.yes` → `Oui` and `common.no` → `Non`, shared by every feature.
- A value the data may lack shows one `common.not-set` → `Non renseigné.`, never an empty cell.

## Never

- Leave a French sentence, a French word or an accented letter in a template or in a component `.ts`: it goes in `fr.json`.
- Add a key nothing uses, or translate into another language.
