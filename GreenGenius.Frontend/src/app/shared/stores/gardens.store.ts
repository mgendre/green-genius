import { inject } from '@angular/core';
import { patchState, signalStore, withHooks, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { Client, GardenDto } from '../../api/api-client.generated';
import { toast } from '@spartan-ng/brain/sonner';
import { TranslateService } from '@ngx-translate/core';

export interface GardensState {
  gardens: GardenDto[];
  loading: boolean;
  loaded: boolean;
  error: boolean;
}

export const GardensStore = signalStore(
  { providedIn: 'root' },
  withState<GardensState>({
    gardens: [],
    loading: false,
    loaded: false,
    error: false,
  }),
  withMethods((store, client = inject(Client), translate = inject(TranslateService)) => ({
    async loadGardens(): Promise<void> {
      patchState(store, { gardens: [], loaded: false, loading: true, error: false });
      try {
        const gardens = await firstValueFrom(client.listGardens());
        patchState(store, { gardens, loaded: true, loading: false, error: false });
      } catch {
        patchState(store, { error: true, loading: false });
      }
    },
    async createGarden(name: string): Promise<boolean> {
      patchState(store, { loading: true });
      try {
        const garden = await firstValueFrom(client.createGarden({ name }));
        const gardens = sortByName([garden, ...store.gardens()]);
        patchState(store, { gardens });
        toast.success(translate.instant('gardens.save.done') as string);
        return true;
      } catch {
        toast.error(translate.instant('gardens.save.failed') as string);
        return false;
      } finally {
        patchState(store, { loading: false });
      }
    },
    async updateGarden(id: string, name: string): Promise<boolean> {
      patchState(store, { loading: true });
      try {
        const garden = await firstValueFrom(client.updateGarden(id, { name }));
        const gardens = sortByName(store.gardens().map((existing) => (existing.id === id ? garden : existing)));
        patchState(store, { gardens });
        toast.success(translate.instant('gardens.save.done') as string);
        return true;
      } catch {
        toast.error(translate.instant('gardens.save.failed') as string);
        return false;
      } finally {
        patchState(store, { loading: false });
      }
    },
    async deleteGarden(id: string): Promise<boolean> {
      patchState(store, { loading: true });
      try {
        await firstValueFrom(client.deleteGarden(id));
        patchState(store, { gardens: sortByName(store.gardens().filter((existing) => existing.id !== id)) });
        toast.success(translate.instant('gardens.delete.done') as string);
        return true;
      } catch {
        toast.error(translate.instant('gardens.delete.failed') as string);
        return false;
      } finally {
        patchState(store, { loading: false });
      }
    },
  })),
  withHooks({
    onInit(store) {
      void store.loadGardens();
    },
  }),
);

function sortByName(gardens: GardenDto[]): GardenDto[] {
  return [...gardens].sort((a, b) => a.name.localeCompare(b.name, 'fr'));
}
