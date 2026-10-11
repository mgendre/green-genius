# GreenGenius.Frontend

Angular 22, NgRx SignalStore, Spartan UI, Tailwind 4 with SCSS 7-1 in `src/styles/`, ngx-translate, ng-icons Lucide.

Generic Angular rules come from the `angular-cli` MCP: `list_projects`, then `get_best_practices` before writing code. `spartan` skill before touching a Spartan component, `scss-styling` skill before writing SCSS.

- **API**: only the generated client `src/app/api/api-client.generated.ts`, never edited, never a handwritten model or HTTP call. The Debug build of `GreenGenius.App` regenerates it. Base URL `/api`, proxied by `proxy.conf.json`.
- **Spartan**: Helm components are created with `npm run ui --prefix GreenGenius.Frontend -- --name=<component>` into `src/app/shared/spartan/helm/`, never written from scratch.
- **Tokens**: `@theme` in `src/styles/vendors/_tailwind.scss`. Colour through semantic classes (`bg-primary`, `text-muted-foreground`), never a raw value.
- **i18n**: `public/i18n/fr.json`, French only, flat dotted lowercase keys sorted alphabetically (`menu.planner`). Texts go through the pipe only (`{{ 'menu.planner' | translate }}`, `[attr.aria-label]="'menu.label' | translate"`), never the `translate` directive. Every standalone component that displays user text must import `TranslatePipe` from `@ngx-translate/core`.
- **Forms**: Signal Forms only.
- **Files**: a file exports one component or one store, together with the types only it uses (its state interface, its form model). The C# rule of one public type per file does not apply to TypeScript.
- **State**: NgRx SignalStore (`@ngrx/signals`). Shared or asynchronous state lives in `src/app/features/<feature>/<feature>.store.ts`, or in `src/app/shared/stores/<name>.store.ts` when several features share it: `signalStore({ providedIn: 'root' }, withState, withComputed, withMethods)`. Store methods are `async` functions awaiting the generated client; derived values are `computed`. No `rxMethod`, no state in a service, no `BehaviorSubject`. Components read the store and never call the API.
- **Dependencies**: never install a package on your own initiative; report the need.

## Where things come from

Every name used in a template needs the TypeScript import below and an entry in the `imports` array of the component. Copy the path from this table: never search `node_modules`, never guess.

| Need | Import | Notes |
|---|---|---|
| Component basics | `import { Component, computed, inject, input, output, signal } from '@angular/core';` | |
| Styled Spartan component | `import { HlmDialogImports } from '@spartan-ng/helm/dialog';` then `imports: [HlmDialogImports]` | One array per component, named `Hlm<Name>Imports`: `HlmAlertDialogImports`, `HlmButtonImports`, `HlmCardImports`, `HlmDialogImports`, `HlmFieldImports`, `HlmInputImports`, `HlmLabelImports`, `HlmSeparatorImports`. Never the pieces one by one (`HlmDialogContent, HlmDialogHeader, …`). `@spartan-ng/helm/<name>` is the code in `src/app/shared/spartan/helm/<name>/`, not an npm package. A missing component is generated, see **Spartan** above. |
| Spartan behaviour | `import { BrnDialogContent } from '@spartan-ng/brain/dialog';` | The only Spartan package in `node_modules`, mostly used by the Helm code itself. |
| Form | `import { form, FormField, FormRoot, maxLength, required } from '@angular/forms/signals';` | Never `@angular/forms` alone. |
| Form field layout | `import { HlmFieldImports } from '@spartan-ng/helm/field';` and `import { HlmInputImports } from '@spartan-ng/helm/input';` | |
| Icon | `import { NgIcon } from '@ng-icons/core';` then `<ng-icon name="lucidePencil" aria-hidden="true" />` | There is no icons folder. A new icon is added in `src/app/app.ts`: import it from `@ng-icons/lucide` and add it to `provideIcons({ ... })`. Names are `lucide` + PascalCase: `lucideTrash`. |
| Translation | `import { TranslatePipe } from '@ngx-translate/core';` | Texts in `public/i18n/fr.json`. |
| Store | `import { patchState, signalStore, withHooks, withMethods, withState } from '@ngrx/signals';` | |
| API types and client | `import { Client, GardenDto } from '../../api/api-client.generated';` | Relative path to `src/app/api/api-client.generated.ts`. Only a store injects `Client`. |

**Check**: `npm run check --prefix GreenGenius.Frontend` must print `check: OK`. It builds without warnings, then looks for the project's known mistakes (i18n keys, `translate` misuse, `hlm` names, Helm imports, dialog services). Each line it prints is `file:line  what to do`. No test runner is set up yet.
