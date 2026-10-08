import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/planner', pathMatch: 'full' },
  {
    path: 'planner',
    loadChildren: () =>
      import('./features/planner/planner.routes').then((m) => m.PLANNER_ROUTES),
  },
];
