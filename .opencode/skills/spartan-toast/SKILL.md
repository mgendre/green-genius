---
name: spartan-toast
description: "How to tell the user that an action succeeded or failed here — a Spartan sonner toast shown by the store method when a save or a delete succeeds or fails. Read before adding a confirmation message, a notification, a toast or a snackbar after a save, a delete or an error."
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

`toast` is the one name an app file imports from `@spartan-ng/brain/…`. The store method that saves or deletes shows the toast of its outcome, and returns `true` when it worked.

```ts
export const GardensStore = signalStore(
  { providedIn: 'root' },
  withState<GardensState>({ gardens: [], loading: false, loaded: false, error: false }),
  withMethods((store, client = inject(Client), translate = inject(TranslateService)) => ({
    async deleteGarden(id: string): Promise<boolean> {
      patchState(store, { loading: true });
      try {
        await firstValueFrom(client.deleteGarden(id));
        patchState(store, { gardens: store.gardens().filter((existing) => existing.id !== id) });
        toast.success(translate.instant('gardens.delete.done') as string);
        return true;
      } catch {
        toast.error(translate.instant('gardens.delete.failed') as string);
        return false;
      } finally {
        patchState(store, { loading: false });
      }
    },
  })),
);
```

The component only reacts to the answer:

```ts
async confirmDeleteGarden(): Promise<void> {
  const garden = this.deletingGarden();
  if (garden?.id && (await this.gardensStore.deleteGarden(garden.id))) {
    this.closeDeleteDialog();
  }
}
```

| Call | When |
|---|---|
| `toast.success(text)` | The save or the delete is done. |
| `toast.error(text)` | The save or the delete failed. |
| `toast(text)` | Neutral information. |

## Rules

- The store method shows the toast, in the `try` for the success and in the `catch` for the failure, then returns `true` or `false`. A component never imports `toast` for a save or a delete.
- The stores catch their own errors: a component reads the boolean answer, never a `try/catch` around a store method and never a failure flag of the store.
- The load of a list shows no toast: its failure is the `error` text of the screen.
- The text is always `translate.instant('<key>')`, with the key added to `public/i18n/fr.json` (`i18n-fr`): `<feature>.save.done`, `<feature>.save.failed`, `<feature>.delete.done`, `<feature>.delete.failed`. Never a French text in the code.
- One toast per action. A create and a rename share `<feature>.save.done`.
- No `<hlm-toaster />` in a feature component, no second toaster.
- No option (`duration`, `position`, `richColors`) unless the task asks for it.
