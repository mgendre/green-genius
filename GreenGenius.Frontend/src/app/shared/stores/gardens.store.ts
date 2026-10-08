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
    async createGarden(name: string): Promise<void> {
      patchState(store, { loading: true, error: null });
      try {
        const garden = await firstValueFrom(client.createGarden({ name }));
        patchState(store, { gardens: [garden, ...store.gardens()] });
      } catch (e) {
        patchState(store, { error: e instanceof Error ? e.message : 'An error occurred' });
      } finally {
        patchState(store, { loading: false });
      }
    },
    async updateGarden(id: string, name: string): Promise<void> {
      patchState(store, { loading: true, error: null });
      try {
        const garden = await firstValueFrom(client.updateGarden(id, { name }));
        patchState(store, { gardens: store.gardens().map((existing) => (existing.id === id ? garden : existing)) });
      } catch (e) {
        patchState(store, { error: e instanceof Error ? e.message : 'An error occurred' });
      } finally {
        patchState(store, { loading: false });
      }
    },
    async deleteGarden(id: string): Promise<void> {
      patchState(store, { loading: true, error: null });
      try {
        await firstValueFrom(client.deleteGarden(id));
        patchState(store, { gardens: store.gardens().filter((existing) => existing.id !== id) });
      } catch (e) {
        patchState(store, { error: e instanceof Error ? e.message : 'An error occurred' });
      } finally {
        patchState(store, { loading: false });
      }
    },
  })),
  withHooks({
    onInit(store) {
      store.loadGardens().finally();
    },
  }),
);
