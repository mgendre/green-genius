# 0014. NgRx SignalStore for state

Date: 2026-10-16

## Status

Accepted

## Context

The front end is built on Angular signals. Mixing services with subjects, `rxMethod` and component state would give
several ways to hold the same data.

## Decision

- Shared or asynchronous state lives in an NgRx SignalStore: `src/app/features/<feature>/<feature>.store.ts`, or
  `src/app/shared/stores/<name>.store.ts` when several features share it, declared with
  `signalStore({ providedIn: 'root' }, withState, withComputed, withMethods)`.
- Store methods are `async` functions awaiting the generated client; derived values are `computed`. No `rxMethod`, no
  state in a service, no `BehaviorSubject`.
- Components read the store and never call the API. The one exception is a shared component that loads its own item from
  an `id` input (`shared/components/plant/plant-identity.component.ts`), which injects `Client` in an Angular
  `resource`.

## Consequences

- Only stores, and that one exception, inject `Client`.
- Success and failure feedback of a save or a delete is raised by the store method.
