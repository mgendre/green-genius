---
name: signal-form
description: "How to build or change a form here — Angular Signal Forms (form, FormField, FormRoot) with Spartan hlm-field and hlmInput, validation messages as i18n keys, submission through a SignalStore. Read before writing any form, input field, validation rule or submit handler."
---

# Signal form

Signal Forms only (`@angular/forms/signals`). Never `FormsModule`, `ReactiveFormsModule`, `ngModel`, `FormGroup` or `FormControl`.

## Recipe

1. A `signal` holds the model: one plain object, one property per field, every property initialised (`''`, `0`, `false`, never `undefined`).
2. `form(model, schema, options)` builds the field tree. Validation rules go in the schema; each `message` is an **i18n key**, never a French sentence.
3. Submission goes in `options.submission.action`: it calls a store method. Angular runs it only when the form is valid, after marking every field as touched.
4. The template binds `[formRoot]` on the `<form>` and `[formField]` on each control. No `(submit)`, no `(ngSubmit)`, no `novalidate`: `FormRoot` does it.
5. Add every key to `public/i18n/fr.json` with the `i18n-fr` skill.

## Reference

```ts
import { Component, inject, output, signal } from '@angular/core';
import { form, FormField, FormRoot, maxLength, required } from '@angular/forms/signals';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { GardensStore } from '../../../shared/stores/gardens.store';

const NAME_MAX_LENGTH = 250;

@Component({
  imports: [FormField, FormRoot, TranslatePipe, HlmButtonImports, HlmFieldImports, HlmInputImports],
  selector: 'app-garden-form',
  templateUrl: './garden-form.component.html',
})
export class GardenFormComponent {
  private readonly gardensStore = inject(GardensStore);

  readonly saved = output<void>();

  readonly gardenModel = signal({ name: '' });

  readonly gardenForm = form(
    this.gardenModel,
    (path) => {
      required(path.name, { message: 'gardens.form.name.required' });
      maxLength(path.name, NAME_MAX_LENGTH, { message: 'gardens.form.name.too-long' });
    },
    { submission: { action: () => this.save() } },
  );

  private async save(): Promise<void> {
    await this.gardensStore.createGarden(this.gardenModel().name);
    this.saved.emit();
  }
}
```

```html
<form [formRoot]="gardenForm">
  <hlm-field>
    <label hlmFieldLabel for="garden-name">{{ 'gardens.form.name.label' | translate }}</label>
    <input hlmInput id="garden-name" [formField]="gardenForm.name" />
    @if (gardenForm.name().touched() && gardenForm.name().invalid()) {
      <hlm-field-error>{{ gardenForm.name().errors()[0].message ?? '' | translate }}</hlm-field-error>
    }
  </hlm-field>
  <button hlmBtn type="submit" [disabled]="gardenForm().submitting()">{{ 'gardens.form.save' | translate }}</button>
</form>
```

## Reading and writing values in the component

`gardenForm` is the field tree. A field is reached with a dot, and **called** to get its state.

| Need | Write | Never |
|---|---|---|
| The values, to save them | `this.gardenModel().name` | `this.gardenForm().name()` |
| The state of one field | `this.gardenForm.name()` then `.valid()`, `.invalid()`, `.touched()`, `.errors()` | `this.gardenForm().name` |
| The current value of one field | `this.gardenForm.name().value()` | `this.gardenForm.name.value` |
| Set one field | `this.gardenForm.name().value.set('Verger')` | `.update(…)` on the field, `.setValue(…)` |
| Set the whole form | `this.gardenModel.set({ name: 'Verger' })` | an `effect` that copies the model into the fields |
| The state of the whole form | `this.gardenForm()` then `.valid()`, `.submitting()`, `.reset(…)` | |

- The model signal and the fields are the same data: typing updates `gardenModel()` at once. Never an `effect`, a subscription or a helper between the model and the form.
- Never open `node_modules/@angular/forms`. If something is missing here, call `tools["angular-cli"].search_documentation` through `execute` with `signal forms <topic>`.

## Rules

- Show an error only when `touched() && invalid()`.
- A max length mirrors the backend limit in `EntitiesConstants` (50, 250, 4000), held in a named constant.
- A form that edits an item received through `input()` builds its model with `linkedSignal`, so it is prefilled: `readonly gardenModel = linkedSignal(() => ({ name: this.garden()?.name ?? '' }));`. To clear a form after a save: `this.gardenForm().reset({ name: '' })`.
- A form shown in a modal: `spartan-dialog` skill.
- Validators available from `@angular/forms/signals`: `required`, `email`, `min`, `max`, `minLength`, `maxLength`, `pattern`. Conditional state: `disabled`, `hidden`, `readonly` with `{ when: ({ valueOf }) => … }`.
- A Spartan piece that is missing (`select`, `textarea`, `checkbox`) is generated with the `spartan` skill, never written by hand.

## Mistakes already made here

- `FormSubmissionState` does not exist. The submit action takes no argument: read `this.gardenModel().name`.
- `input`, `output`, `signal`, `linkedSignal`, `computed` come from `@angular/core`, each in the import list.
- `output<void>()` is emitted with `this.saved.emit()`; `output<string>()` needs a value. Declare the type you emit.
- An `id` of a generated DTO is optional (`string | undefined`): test it before passing it to the store, `if (garden?.id) { … }`.
- A template referenced by `templateUrl` must exist before the build: write the `.html` in the same step as the `.ts`.
- The shared folder is `src/app/shared/`, not `src/shared/`.
