---
name: spartan-toast
description: "How to tell the user that an action succeeded or failed here — a Spartan sonner toast shown from the component after a store call. Read before adding a confirmation message, a notification, a toast or a snackbar after a save, a delete or an error."
---

# Spartan toast

A short message that appears in a corner and leaves by itself, after an action: "Jardin enregistré.", "La suppression a échoué.". Not for a validation message of a form (`signal-form`), not for a question (`spartan-alert-dialog`).

## Once for the app

The toaster is declared once, in the root component. Check `GreenGenius.Frontend/src/app/app.html`: if `<hlm-toaster />` is there, skip this section.

1. `GreenGenius.Frontend/src/app/shared/spartan/helm/sonner/` missing: `npm run ui --prefix GreenGenius.Frontend -- --name=sonner`.
2. `GreenGenius.Frontend/src/app/app.ts`: `import { HlmToasterImports } from '@spartan-ng/helm/sonner';` and `HlmToasterImports` in `imports`.
3. `GreenGenius.Frontend/src/app/app.html`, last line: `<hlm-toaster />`.
4. Run the check. `bundle initial exceeded maximum budget`: the toaster adds about 60 kB to the first load, and the budget of `angular.json` is the user's decision. Do not change it, do not remove the toaster: stop and report that line.

## Showing a toast

`toast` is the one name an app component imports from `@spartan-ng/brain/…`.

Verified in the browser: both toasts appear after the click.

```ts
import { Component, inject } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { toast } from '@spartan-ng/brain/sonner';
import { GardensStore } from '../../../shared/stores/gardens.store';

@Component({ /* … */ })
export class PlannerGardensListComponent {
  private readonly translate = inject(TranslateService);
  readonly gardensStore = inject(GardensStore);

  async confirmDeleteGarden(id: string): Promise<void> {
    await this.gardensStore.deleteGarden(id);
    if (this.gardensStore.error()) {
      toast.error(this.translate.instant('gardens.delete.failed'));
    } else {
      toast.success(this.translate.instant('gardens.delete.done'));
    }
  }
}
```

| Call | When |
|---|---|
| `toast.success(text)` | The action is done. |
| `toast.error(text)` | The action failed. |
| `toast(text)` | Neutral information. |

## Rules

- The component shows the toast, after `await` of the store method. A store never imports `toast`.
- The stores here catch their own errors and keep the message in `error`: read `store.error()` after the `await`, never a `try/catch` around a store method.
- The text is always `this.translate.instant('<key>')`, with the key added to `public/i18n/fr.json` (`i18n-fr`). Never a French text in the code.
- One toast per action. No toast for what the screen already shows (a new line in a list is enough when the task does not ask for a message).
- No `<hlm-toaster />` in a feature component, no second toaster.
- No option (`duration`, `position`, `richColors`) unless the task asks for it.
- Braces `{ }` on every `if`, `else`, `for` and `while`, also around a single line: never `if (x) return;`.
