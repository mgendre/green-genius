---
name: spartan-combobox
description: "How to build a search box with suggestions here — a Spartan hlm-combobox that filters a list by typed text and reports the chosen item. Read before adding an autocomplete, a searchable select or a field that suggests items, and whenever a BrnComboboxConfig or combobox provider error appears."
---

# Spartan combobox

A field where the user types and picks one item of a list. The combobox filters by itself: case, accents and punctuation are ignored, an item is kept when its text **contains** what was typed. You give it the list and the text of an item; you write no filter, no query signal, no provider.

Every import comes from `@spartan-ng/helm/combobox`, as the one array `HlmComboboxImports`. Never import from `@spartan-ng/brain/combobox`: no `BrnComboboxConfig`, no `provideBrnComboboxConfig`, no `comboboxContainsFilter`.

## Recipe

1. The items are the **whole objects** of the store (`[value]="plant"`), never an id or a string.
2. One arrow function property turns an item into the text that is searched. To search in two fields, put both in that text.
3. `(valueChange)` gives the chosen object, or `null` when the field is cleared: ignore the empty case, emit the object through an `output()`.
4. The component that hosts the search decides what the choice does (navigate, open a dialog).

## Reference

Verified in the browser: "tomate", "SOLANUM" and "echal" find their plant, "xyz" shows the empty text, a click reports the plant.

```ts
import { Component, inject, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmComboboxImports } from '@spartan-ng/helm/combobox';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { PlantSummaryDto } from '../../api/api-client.generated';
import { EncyclopediaStore } from './encyclopedia.store';

@Component({
  imports: [TranslatePipe, HlmComboboxImports, HlmLabelImports],
  selector: 'app-encyclopedia-search',
  templateUrl: './encyclopedia-search.html',
})
export class EncyclopediaSearchComponent {
  private readonly store = inject(EncyclopediaStore);

  readonly plants = this.store.plants;
  readonly selected = output<PlantSummaryDto>();

  readonly searchedText = (plant: PlantSummaryDto): string =>
    `${plant.nameFr} ${plant.binomialName ?? ''}`;

  select(plant: PlantSummaryDto | null | undefined): void {
    if (plant) {
      this.selected.emit(plant);
    }
  }
}
```

```html
<label hlmLabel for="encyclopedia-search">{{ 'encyclopedia.search.label' | translate }}</label>
<hlm-combobox [itemToString]="searchedText" (valueChange)="select($event)">
  <hlm-combobox-input inputId="encyclopedia-search" />
  <hlm-combobox-content *hlmComboboxPortal>
    <hlm-combobox-empty>{{ 'encyclopedia.search.empty' | translate }}</hlm-combobox-empty>
    <div hlmComboboxList>
      @for (plant of plants(); track plant.id) {
        <hlm-combobox-item [value]="plant">{{ plant.nameFr }}</hlm-combobox-item>
      }
    </div>
  </hlm-combobox-content>
</hlm-combobox>
```

The host listens to the output:

```html
<app-encyclopedia-search (selected)="openPlant($event)" />
```

## The pieces, and only these

| Piece | Role |
|---|---|
| `<hlm-combobox>` | Holds the state. Inputs: `[itemToString]`, `[value]`. Output: `(valueChange)`. |
| `<hlm-combobox-input inputId="…" />` | The text field. `inputId` is the `for` of the label; `[placeholder]` when asked. |
| `<hlm-combobox-content *hlmComboboxPortal>` | The list that opens under the field. Both the element and the `*hlmComboboxPortal` are needed. |
| `<hlm-combobox-empty>` | The text shown when nothing matches. |
| `<div hlmComboboxList>` | Wraps the `@for`. |
| `<hlm-combobox-item [value]="item">` | One suggestion. What is between the tags is what the user reads. |

After a choice the field shows the text of `itemToString`.

## Mistakes already made here

- `providers: [{ provide: BrnComboboxConfig, … }]`: an interface is not a value, and the defaults already ignore case and accents. No `providers` at all.
- `(searchChange)="onSearch($event)"` with a `query` in the store and a filtered list: the combobox filters, give it the whole list.
- `@for (plant of plants; …)`: a store value is a signal, call it: `plants()`.
- `[value]="plant.id"`: the search then runs on the id. The value is the object.
- `(itemSelected)` on the item: it does not exist. The choice comes from `(valueChange)` on `<hlm-combobox>`.
- A text of the template without its key in `public/i18n/fr.json`: add the label and the empty text, see `i18n-fr`.

## Never

- Read or copy from `shared/spartan/helm/combobox/src/lib/` or from `node_modules`: this file is the reference.
- Use the pieces not listed above (chips, multiple, groups, value template) unless the task asks for several choices: then report that this skill does not cover it.
