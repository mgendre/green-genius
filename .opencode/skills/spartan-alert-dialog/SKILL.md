---
name: spartan-alert-dialog
description: "How to ask for a confirmation here — a Spartan hlm-alert-dialog declared in the template and driven by one signal, with a cancel and an action button. Read before adding a confirmation before a delete or any action that cannot be undone."
---

# Spartan alert dialog

A confirmation the user must answer: cancel or do it. Same idea as `spartan-dialog`: declared in the template of the component that asks, driven by one signal, no service.

Every import comes from `@spartan-ng/helm/alert-dialog`, as the one array `HlmAlertDialogImports`. Reference in the code: `GreenGenius.Frontend/src/app/features/planner/components/planner-gardens-list.component.ts` and its `.html`.

## Recipe

1. One `signal<boolean>` for open or closed, one signal for the item concerned, one `computed` that turns the boolean into `'open'` or `'closed'`.
2. The button of the list only opens the dialog. The action runs in the confirm method.
3. The dialog closes in one place, `closeDeleteDialog()`, called by `(closed)` (Escape), by the cancel button and at the end of the confirm method.
4. The template declares `<hlm-alert-dialog>` once, at the end, outside the card or list.

## Reference

Verified in the browser: the button opens it, "Annuler" closes it without acting, the action button acts and closes it.

```ts
import { Component, computed, inject, signal } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmAlertDialogImports } from '@spartan-ng/helm/alert-dialog';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { GardenDto } from '../../../api/api-client.generated';
import { GardensStore } from '../../../shared/stores/gardens.store';

@Component({
  imports: [TranslatePipe, HlmAlertDialogImports, HlmButtonImports],
  selector: 'app-planner-gardens-list',
  templateUrl: './planner-gardens-list.component.html',
})
export class PlannerGardensListComponent {
  readonly gardensStore = inject(GardensStore);

  readonly deleteDialogOpen = signal(false);
  readonly deletingGarden = signal<GardenDto | undefined>(undefined);
  readonly deleteDialogState = computed(() => (this.deleteDialogOpen() ? 'open' : 'closed'));

  askDeleteGarden(garden: GardenDto): void {
    this.deletingGarden.set(garden);
    this.deleteDialogOpen.set(true);
  }

  closeDeleteDialog(): void {
    this.deleteDialogOpen.set(false);
  }

  async confirmDeleteGarden(): Promise<void> {
    const garden = this.deletingGarden();
    if (garden?.id) {
      await this.gardensStore.deleteGarden(garden.id);
    }
    this.closeDeleteDialog();
  }
}
```

```html
<button hlmBtn variant="destructive" type="button" (click)="askDeleteGarden(garden)">
  {{ 'gardens.delete.action' | translate }}
</button>

<hlm-alert-dialog [state]="deleteDialogState()" (closed)="closeDeleteDialog()">
  <hlm-alert-dialog-content *hlmAlertDialogPortal>
    <hlm-alert-dialog-header>
      <h3 hlmAlertDialogTitle>{{ 'gardens.delete.title' | translate }}</h3>
      <p hlmAlertDialogDescription>{{ 'gardens.delete.body' | translate: deletingGarden() }}</p>
    </hlm-alert-dialog-header>
    <hlm-alert-dialog-footer>
      <button hlmAlertDialogCancel type="button" (click)="closeDeleteDialog()">
        {{ 'common.cancel' | translate }}
      </button>
      <button hlmAlertDialogAction variant="destructive" type="button" (click)="confirmDeleteGarden()" [disabled]="gardensStore.loading()">
        {{ 'gardens.delete.action' | translate }}
      </button>
    </hlm-alert-dialog-footer>
  </hlm-alert-dialog-content>
</hlm-alert-dialog>
```

The name of the store method (`deleteGarden`) is the one of your store: read it there.

## Mistakes to avoid

- A service or a ref to open it (`HlmDialogService`, `BrnAlertDialog`, a template reference `#dialog`): the signal does it.
- `<hlm-alert-dialog-content>` without `*hlmAlertDialogPortal`: nothing shows.
- A clickable `<div>` to open it: a `<button hlmBtn type="button">`.
- A plain `hlm-dialog` for a confirmation: the alert dialog cannot be closed by a click outside, which is the point.
