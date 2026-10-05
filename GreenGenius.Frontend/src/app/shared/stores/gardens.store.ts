import { inject } from '@angular/core';
import { patchState, signalStore, withHooks, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { Client, GardenDto } from '../../api/api-client.generated';

export interface GardensState {
  gardens: GardenDto[];
  loading: boolean;
  loaded: boolean;
  error: string | null;
}

export const GardensStore = signalStore(
  { providedIn: 'root' },
  withState<GardensState>({
    gardens: [],
    loading: false,
    loaded: false,
    error: null,
  }),
  withMethods((store, client = inject(Client)) => ({
    async loadGardens(): Promise<void> {
      patchState(store, { gardens: [], loaded: false, loading: true, error: null });
      const gardens = await firstValueFrom(client.listGardens());
      patchState(store, { gardens, loaded: true, loading: false, error: null });
    },
  })),
  withHooks({
    onInit(store) {
      store.loadGardens();
    },
  }),
);
