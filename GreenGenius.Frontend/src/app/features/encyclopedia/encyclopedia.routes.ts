import { Routes } from '@angular/router';
import { EncyclopediaComponent } from './pages/encyclopedia.component';
import { EncyclopediaPageComponent } from './pages/encyclopedia-page.component';

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
