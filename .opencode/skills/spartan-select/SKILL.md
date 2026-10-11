---
name: spartan-select
description: "How to let the user pick one value of a closed list here — a Spartan hlm-select, alone or bound to a Signal Form field. Read before adding a select, a dropdown or a field whose values come from an enum. Not for a search box that filters as the user types: spartan-combobox."
---

# Spartan select

A button that opens a short closed list (an enum, a few options) and keeps the chosen value. For a long list the user searches by typing, use `spartan-combobox`.

Every import comes from `@spartan-ng/helm/select`, as the one array `HlmSelectImports`. If `GreenGenius.Frontend/src/app/shared/spartan/helm/select/` does not exist, generate it first: `npm run ui --prefix GreenGenius.Frontend -- --name=select`.

## Recipe

1. The values are the real values (the members of the enum), never their French text.
2. A `Record` gives the French text of each value, as i18n keys; one arrow function property returns it and is given to `[itemToString]`: the button shows that text once a value is chosen.
3. In a form: `[formField]` on `<hlm-select>`, nothing else. Outside a form: `[value]` and `(valueChange)`.

## Reference: in a Signal Form

Verified in the browser: the button shows the text of the starting value, a choice changes the model of the form.

```ts
import { Component, inject, signal } from '@angular/core';
import { form, FormField } from '@angular/forms/signals';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { SunlightLevel } from '../../api/api-client.generated';

interface NeedsModel {
  sunlight: SunlightLevel | null;
}

@Component({
  imports: [FormField, TranslatePipe, HlmLabelImports, HlmSelectImports],
  selector: 'app-plant-needs-form',
  templateUrl: './plant-needs-form.component.html',
})
export class PlantNeedsFormComponent {
  private readonly translate = inject(TranslateService);

  readonly sunlightLevels = Object.values(SunlightLevel);
  readonly sunlightKeys: Record<SunlightLevel, string> = {
    [SunlightLevel.Low]: 'plants.sunlight.low',
    [SunlightLevel.Medium]: 'plants.sunlight.medium',
    [SunlightLevel.High]: 'plants.sunlight.high',
  };
  readonly sunlightText = (level: SunlightLevel): string =>
    this.translate.instant(this.sunlightKeys[level]);

  readonly needs = signal<NeedsModel>({ sunlight: null });
  readonly needsForm = form(this.needs);
}
```

```html
<label hlmLabel for="plant-sunlight">{{ 'plants.sunlight.label' | translate }}</label>
<hlm-select [formField]="needsForm.sunlight" [itemToString]="sunlightText">
  <hlm-select-trigger buttonId="plant-sunlight">
    <hlm-select-value [placeholder]="'plants.sunlight.placeholder' | translate" />
  </hlm-select-trigger>
  <hlm-select-content *hlmSelectPortal>
    @for (level of sunlightLevels; track level) {
      <hlm-select-item [value]="level">{{ sunlightKeys[level] | translate }}</hlm-select-item>
    }
  </hlm-select-content>
</hlm-select>
```

The rest of the form (validation, messages, `[formRoot]`, the save button) is in `signal-form`.

## Reference: outside a form

```ts
readonly sunlight = signal<SunlightLevel | null>(null);

chooseSunlight(level: SunlightLevel | null | undefined): void {
  this.sunlight.set(level ?? null);
}
```

```html
<hlm-select [value]="sunlight()" [itemToString]="sunlightText" (valueChange)="chooseSunlight($event)">
```

The inside of `<hlm-select>` is the same.

## The pieces, and only these

| Piece | Role |
|---|---|
| `<hlm-select>` | Holds the value. `[formField]`, or `[value]` with `(valueChange)`. Always `[itemToString]`. |
| `<hlm-select-trigger buttonId="…">` | The button. `buttonId` is the `for` of the label. |
| `<hlm-select-value placeholder="…" />` | Inside the trigger: the chosen text, or the placeholder. |
| `<hlm-select-content *hlmSelectPortal>` | The list that opens. Both the element and the `*hlmSelectPortal` are needed. |
| `<hlm-select-item [value]="…">` | One option. Between the tags, what the user reads. |

## Mistakes to avoid

- No `[itemToString]`: the button shows `Medium` instead of the French text.
- `[value]="'Plein soleil'"`: the value is the member of the enum, the text goes between the tags.
- `[(ngModel)]`, `formControlName`, a native `<select>`: `[formField]`.
- An option text typed in French in the template: a key in `public/i18n/fr.json`, see `i18n-fr`.
- Reading `shared/spartan/helm/select/src/lib/` or `node_modules`: this file is the reference.
- Several choices, groups, a custom value template: this skill does not cover them, report it.
- `if (x) return;` without braces: `{ }` on every `if`, `else`, `for` and `while`, also around a single line.
