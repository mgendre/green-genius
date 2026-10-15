---
name: signal-store
description: "How to add or change state here — NgRx SignalStore over the generated API client: state shape, async methods with patchState, computed values, hooks. Read before creating a store, adding a method that calls the API, or moving state out of a component."
---

# SignalStore

Reference: `GreenGenius.Frontend/src/app/shared/stores/gardens.store.ts`, read it before writing a store. Paths below start at `GreenGenius.Frontend/`.

## Where

- State of one feature: `src/app/features/<feature>/<feature>.store.ts`.
- State several features read: `src/app/shared/stores/<name>.store.ts`.
- Always `signalStore({ providedIn: 'root' }, …)`, never provided in a component.

## Recipe

1. Declare the state as an exported interface, then `withState<TState>({ … })` with every property initialised. A store that loads from the API carries `loading: boolean`, `loaded: boolean` and `error: boolean`; `error` is set by the load method only, a failed save or delete sets no flag.
2. Values derived from the state go in `withComputed`, as `computed`. Never store what can be computed.
3. Methods go in `withMethods((store, client = inject(Client), translate = inject(TranslateService)) => ({ … }))`; `translate` only when a method shows a toast.
4. A method that calls the API is `async`, awaits `firstValueFrom(client.<operation>(…))` and writes the result with `patchState`. No `subscribe`, no `HttpClient`.
5. Data needed as soon as the store exists is loaded in `withHooks({ onInit(store) { … } })`.

## Shape of a method

A load method sets `error`. A create, update or delete method shows its toast (`spartan-toast` skill) and returns whether it worked.

```ts
async createGarden(name: string): Promise<boolean> {
  patchState(store, { loading: true });
  try {
    const garden = await firstValueFrom(client.createGarden({ name }));
    patchState(store, { gardens: [garden, ...store.gardens()] });
    toast.success(translate.instant('gardens.save.done') as string);
    return true;
  } catch {
    toast.error(translate.instant('gardens.save.failed') as string);
    return false;
  } finally {
    patchState(store, { loading: false });
  }
},
```

- Create: add the returned DTO to the list. Update: replace the item with the same `id` using `map`. Delete: drop it with `filter`. Always a new array, never a reload of the whole list.
- Types come from `src/app/api/api-client.generated.ts`, never a hand-written model. If the client lacks the operation, stop and report: the backend comes first.
- A store that depends on another one injects it inside `onInit` or `withMethods`, as `planner.store.ts` does.

## Mistakes to avoid

- The generated client takes a request object, not loose values: `client.createGarden({ name })`, `client.updateGarden(id, { name })`. Read the method in `api-client.generated.ts` before calling it.
- A client call returns an `Observable`: never `await` it directly.
- An `id` of a generated DTO is optional (`string | undefined`).

## Components

A component injects the store (`readonly gardensStore = inject(GardensStore)`), reads its signals and calls its methods. It closes its window or form when a save or delete method answered `true`. It never copies store state into a local signal and never shows the toast of a save or a delete itself.
