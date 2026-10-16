import { effect, inject } from '@angular/core';
import { patchState, signalStore, withHooks, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { BedDto, Client } from '../../api/api-client.generated';
import { PlannerStore } from './planner.store';
import { toast } from '@spartan-ng/brain/sonner';
import { TranslateService } from '@ngx-translate/core';

export interface BedsState {
  beds: BedDto[];
  loading: boolean;
  loaded: boolean;
  loadError: boolean;
  deleting: boolean;
}

function sortedBeds(beds: BedDto[]): BedDto[] {
  return [...beds].sort((a, b) => a.name.localeCompare(b.name, 'fr'));
}

export const BedsStore = signalStore(
  { providedIn: 'root' },
  withState<BedsState>({
    beds: [],
    loading: false,
    loaded: false,
    loadError: false,
    deleting: false,
  }),
  withMethods((store, client = inject(Client), plannerStore = inject(PlannerStore), translate = inject(TranslateService)) => ({
    async loadBeds(gardenId: string): Promise<void> {
      patchState(store, { beds: [], loading: true, loadError: false, loaded: false });
      try {
        const beds = await firstValueFrom(client.listBeds(gardenId));
        if (plannerStore.selectedGardenId() !== gardenId) {
          return;
        }
        patchState(store, {
          beds,
          loaded: true,
          loading: false,
        });
      } catch {
        if (plannerStore.selectedGardenId() !== gardenId) {
          return;
        }
        patchState(store, { loadError: true, loading: false, loaded: false });
      }
    },
    async createBed(gardenId: string, name: string): Promise<boolean> {
      try {
        const bed = await firstValueFrom(client.createBed(gardenId, { name }));
        patchState(store, { beds: sortedBeds([...store.beds(), bed]) });
        toast.success(translate.instant('beds.save.done') as string);
        return true;
      } catch {
        toast.error(translate.instant('beds.save.failed') as string);
        return false;
      }
    },
    async updateBed(gardenId: string, id: string, name: string): Promise<boolean> {
      const current = store.beds().find((existing) => existing.id === id);
      const plantIds = current?.plants.map((plant) => plant.id).filter((plantId): plantId is string => plantId !== undefined) ?? [];
      try {
        const bed = await firstValueFrom(client.updateBed(gardenId, id, { name, plantIds }));
        patchState(store, {
          beds: sortedBeds(store.beds().map((existing) =>
            existing.id === bed.id ? bed : existing,
          )),
        });
        toast.success(translate.instant('beds.save.done') as string);
        return true;
      } catch {
        toast.error(translate.instant('beds.save.failed') as string);
        return false;
      }
    },
    async deleteBed(gardenId: string, id: string): Promise<boolean> {
      patchState(store, { deleting: true });
      try {
        await firstValueFrom(client.deleteBed(gardenId, id));
        patchState(store, { beds: store.beds().filter((existing) => existing.id !== id) });
        toast.success(translate.instant('beds.delete.done') as string);
        return true;
      } catch {
        toast.error(translate.instant('beds.delete.failed') as string);
        return false;
      } finally {
        patchState(store, { deleting: false });
      }
    },
  })),
  withHooks({
    onInit(store) {
      const plannerStore = inject(PlannerStore);
      effect(() => {
        const gardenId = plannerStore.selectedGardenId();
        if (gardenId) {
          void store.loadBeds(gardenId);
        }
      });
    },
  }),
);
