import { inject } from '@angular/core';
import { patchState, signalStore, withHooks, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { Client, PlantSummaryDto } from '../../api/api-client.generated';

export interface EncyclopediaState {
  plants: PlantSummaryDto[];
  loading: boolean;
  loaded: boolean;
  error: string | null;
}

export const EncyclopediaStore = signalStore(
  { providedIn: 'root' },
  withState<EncyclopediaState>({
    plants: [],
    loading: false,
    loaded: false,
    error: null,
  }),
  withMethods((store, client = inject(Client)) => ({
    async loadPlants(): Promise<void> {
      patchState(store, { loading: true, error: null });
      try {
        const plants = await firstValueFrom(client.listPlants());
        patchState(store, { plants, loaded: true, error: null });
      } catch (e) {
        patchState(store, { error: e instanceof Error ? e.message : 'An error occurred' });
      } finally {
        patchState(store, { loading: false });
      }
    },
  })),
  withHooks({
    onInit(store) {
      store.loadPlants().finally();
    },
  }),
);
