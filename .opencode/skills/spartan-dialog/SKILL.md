---
name: spartan-dialog
description: "How to open a modal here — a Spartan hlm-dialog declared in the template and driven by one signal, with its content in a child component. Read before adding a dialog, a modal, a popup or a create/edit window, and whenever a BrnDialogRef or dialog provider error appears."
---

# Spartan dialog

The dialog is declared in the template of the component that opens it and driven by one signal. No `HlmDialogService`, no `BrnDialogRef`, no `BrnDialog`: they cause `NullInjectorError: No provider for BrnDialogRef`.

Every import comes from `@spartan-ng/helm/…`, as one `Hlm…Imports` array.

## Recipe

1. The **content** is its own component. It knows nothing about the dialog: data comes through `input()`, the end is reported through `output()` (`saved`, `cancelled`).
2. The **opener** holds the state: one `signal<boolean>` for open or closed, one signal for the item being edited, and a `computed` that turns the boolean into `'open'` or `'closed'`.
3. The template declares `<hlm-dialog>` once, at the end, outside the card or list. One dialog serves both create and edit: the edited item is `undefined` for a creation.
4. The dialog closes in one place, `closeDialog()`, called by `(closed)` (Escape, click outside, close button) and by the outputs of the content.

## Reference: opener

```ts
import { Component, computed, signal } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialogImports } from '@spartan-ng/helm/dialog';
import { GardenDto } from '../../../api/api-client.generated';
import { GardenFormComponent } from './garden-form.component';

@Component({
  imports: [TranslatePipe, HlmButtonImports, HlmDialogImports, GardenFormComponent],
  selector: 'app-planner-gardens-list',
  templateUrl: './planner-gardens-list.component.html',
})
export class PlannerGardensListComponent {
  readonly dialogOpen = signal(false);
  readonly editedGarden = signal<GardenDto | undefined>(undefined);
  readonly dialogState = computed(() => (this.dialogOpen() ? 'open' : 'closed'));

  addGarden(): void {
    this.openDialog(undefined);
  }

  editGarden(garden: GardenDto): void {
    this.openDialog(garden);
  }

  closeDialog(): void {
    this.dialogOpen.set(false);
  }

  private openDialog(garden: GardenDto | undefined): void {
    this.editedGarden.set(garden);
    this.dialogOpen.set(true);
  }
}
```

```html
<button hlmBtn type="button" (click)="addGarden()">{{ 'gardens.list.add' | translate }}</button>

<hlm-dialog [state]="dialogState()" (closed)="closeDialog()">
  <hlm-dialog-content *hlmDialogPortal="let ctx">
    <hlm-dialog-header>
      <h3 hlmDialogTitle>{{ (editedGarden() ? 'gardens.form.title.edit' : 'gardens.form.title.create') | translate }}</h3>
    </hlm-dialog-header>
    <app-garden-form [garden]="editedGarden()" (saved)="closeDialog()" (cancelled)="closeDialog()" />
  </hlm-dialog-content>
</hlm-dialog>
```

## Reference: content

The content of a create/edit dialog is a form: write it with the `signal-form` skill. What the dialog adds:

```ts
readonly garden = input<GardenDto>();
readonly saved = output<void>();
readonly cancelled = output<void>();

readonly gardenModel = linkedSignal(() => ({ name: this.garden()?.name ?? '' }));
```

```html
<form [formRoot]="gardenForm">
  <!-- fields, see signal-form -->
  <hlm-dialog-footer>
    <button hlmBtn type="button" variant="outline" (click)="cancelled.emit()">{{ 'gardens.form.cancel' | translate }}</button>
    <button hlmBtn type="submit" [disabled]="gardenForm().invalid() || gardenForm().submitting()">{{ 'gardens.form.save' | translate }}</button>
  </hlm-dialog-footer>
</form>
```

- The content uses `hlm-dialog-footer`, so it imports `HlmDialogImports` too.
- `*hlmDialogPortal` creates the content when the dialog opens and destroys it when it closes: the form starts fresh each time, prefilled from its input.
- The cancel button is `type="button"`, or it submits the form.
- The save handler calls the store, then emits `saved`. The opener closes the dialog; the content never does.

## Pieces of `@spartan-ng/helm/dialog`

| Selector | Use |
|---|---|
| `hlm-dialog` | the dialog; `[state]` is `'open'` or `'closed'`, `(closed)` fires on every close |
| `*hlmDialogPortal="let ctx"` | on `hlm-dialog-content`, renders it lazily |
| `hlm-dialog-content` | the panel, with its close button |
| `hlm-dialog-header` | wraps the title and description |
| `[hlmDialogTitle]` | the title, required for accessibility |
| `[hlmDialogDescription]` | optional text under the title |
| `hlm-dialog-footer` | the action buttons |

## Confirmation

A confirmation before a delete is an alert dialog: `.opencode/skills/spartan-alert-dialog/SKILL.md`.

## Mistakes already made here

- `inject(BrnDialogRef)`, or `BrnDialogRef` / `BrnDialog` in `imports`: there is no dialog reference outside `hlm-dialog-content`. Use the signal and the outputs.
- `dialogRef.open()` does not exist. Opening is `this.dialogOpen.set(true)`.
- Two dialogs for create and edit: one dialog, `editedGarden()` tells which.
- `@spartan-ts/…`, `@spartan-brain/…`: these packages do not exist.
- An `hlm-alert-dialog` inside the content of an `hlm-dialog`: the inner one never opens. One dialog per window, side by side in the opener.
- `if (x) return;` without braces: `{ }` on every `if`, `else`, `for` and `while`, also around a single line.

The check does not catch a provider error: report the dialog as not opened in a browser.
