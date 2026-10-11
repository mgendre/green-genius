---
name: loading-state
description: "How a component that loads one item by an id input shows loading, not found and the value, and reloads when the id changes — Angular resource over the generated client. Read before writing a component that fetches one record itself, and whenever a loading or not-found text shows at the wrong time."
---

# Loading state of one item

Only for the one case where a shared component loads its own item from an `id` input, decided by the user (the plant sheet of the encyclopedia). Everything else goes through a store: skill `signal-store`. Paths start at `GreenGenius.Frontend/src/app/`.

## Recipe

1. The id is `readonly id = input.required<string>();`.
2. The item is an Angular `resource` whose `params` is the id and whose `loader` awaits the generated client: it reloads by itself when the id changes and cancels the previous load.
3. The template tests the three states in this order: loading, error, value. Never test `value()` against `null` to mean "loading".
4. The "not found" text belongs to the error branch only. The "loading" text belongs to `isLoading()` only.
5. Values derived from the item are one `computed` each only when they need logic; a plain field is read in the template.

## Reference

```ts
import { Component, inject, input, resource } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { Client } from '../../api/api-client.generated';

@Component({
  imports: [TranslatePipe],
  selector: 'app-plant-identity',
  templateUrl: './plant-identity.html',
})
export class PlantIdentityComponent {
  private readonly client = inject(Client);

  readonly id = input.required<string>();

  readonly plant = resource({
    params: () => this.id(),
    loader: ({ params: id }) => firstValueFrom(this.client.getPlant(id)),
  });
}
```

```html
@if (plant.isLoading()) {
  <p>{{ 'encyclopedia.identity.loading' | translate }}</p>
} @else if (plant.error()) {
  <p>{{ 'encyclopedia.identity.not-found' | translate }}</p>
} @else if (plant.value(); as p) {
  <h2>{{ p.nameFr }}</h2>
}
```

## Mistakes already made here

- A `signal<Plant | null>(null)` set in `afterNextRender`: the not-found text showed while loading, and a new id was never fetched.
- A `computed` that returns its input unchanged (`currentPlant`): delete it.
- `!` in a template to silence a possibly-null error: use the alias of `@else if (… ; as p)`.
- One `computed` per field repeating the same null test: read `p.needs?.heightCm` in the template, or write one small function for the label of an enum (skill `i18n-fr`, section Enum labels).

## Never

- Inject `Client` in a component that is not this shared sheet: a list or a form goes through a store.
- Use `effect` or `subscribe` to fetch.
- Write an `if`, `else`, `for` or `while` without `{ }`, also around a single line.
