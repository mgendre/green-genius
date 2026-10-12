---
name: feature-routes
description: "How to add or change the routes of a feature here — lazy feature routes, a layout component with child pages, a route id given to a component input. Read before adding a page, a route, a URL with an id, or when a routed component gets no value for its input."
---

# Feature routes

Models: `GreenGenius.Frontend/src/app/app.routes.ts`, `GreenGenius.Frontend/src/app/features/planner/planner.routes.ts`, `GreenGenius.Frontend/src/app/app.config.ts`. Paths below start at `GreenGenius.Frontend/src/app/`.

## Where

| Piece | File |
|---|---|
| Entry of the feature, lazy | `app.routes.ts`: `{ path: '<feature>', loadChildren: () => import('./features/<feature>/<feature>.routes').then((m) => m.<FEATURE>_ROUTES) }` |
| Routes of the feature | `features/<feature>/<feature>.routes.ts`, one exported `Routes` constant |
| Layout (title, content width, `<router-outlet />`) | `features/<feature>/pages/<feature>.component.ts` + `<feature>.html` |
| A page | `features/<feature>/pages/<feature>-page.component.ts` + `.html` |

## Recipe

1. A feature with a title and the shared content width is one layout component with the pages as `children`. A page never repeats the title or the `.app-content` wrapper.
2. The layout imports `RouterOutlet` and holds the title and `<div class="app-content"><router-outlet /></div>`.
3. A URL with an id (`/encyclopedia/<id>`) is a child `path: ':id'`; the same page also serves `path: ''` when the URL without id is valid.
4. The page reads the id through an input named like the route parameter. Route inputs are bound once, in `app.config.ts`: `provideRouter(routes, withComponentInputBinding())`, imported from `@angular/router`.
5. An input that a URL may lack is `input<string>()` (its value is `undefined`). `input.required` only when every route to that component carries the parameter.
6. A link in a template is `routerLink`; navigating after an event is `inject(Router).navigate(['/encyclopedia', id])`.

## Reference

```ts
export const ENCYCLOPEDIA_ROUTES: Routes = [
  {
    path: '',
    component: EncyclopediaComponent,
    children: [
      { path: '', component: EncyclopediaPageComponent },
      { path: ':id', component: EncyclopediaPageComponent },
    ],
  },
];
```

```ts
export class EncyclopediaPageComponent {
  readonly id = input<string>();
}
```

```html
@if (id(); as plantId) {
  <app-plant-identity [id]="plantId" />
} @else {
  <p>{{ 'encyclopedia.page.empty' | translate }}</p>
}
```

## Mistakes already made here

- A layout component written but not named in any route: the page opens without its title. Every routed component is named after `component:` in a routes file; the check reports a component used nowhere.
- `input.required` on a page that is also routed at `''`: Angular raises NG0950 on `/encyclopedia`.
- `withComponentInputBinding()` forgotten: the input stays empty with no error. The check reports it.
- The id compared with `''`: an absent parameter is `undefined`, test the value itself with `@if`.

## Never

- Read `ActivatedRoute` or `paramMap` in a component to get an id: the input does it.
- Add a route to a feature without opening it in the menu or a link when the task asks for the page to be reachable.
- Write an `if`, `else`, `for` or `while` without `{ }`, also around a single line.
