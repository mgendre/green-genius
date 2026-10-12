import { effect, inject } from '@angular/core';
import { patchState, signalStore, withHooks, withMethods, withState } from '@ngrx/signals';
import { firstValueFrom } from 'rxjs';
import { BedDto, Client, PlantSummaryDto } from '../../api/api-client.generated';
import { PlannerStore } from './planner.store';

export interface BedPlant {
  id: string;
  nameFr: string;
  binomialName?: string;
  emoji?: string;
  family?: string;
}

export interface Bed {
  id: string;
  name: string;
  gardenId: string;
  open: boolean;
  plants: BedPlant[];
}

export interface BedsState {
  beds: Bed[];
  loading: boolean;
  loaded: boolean;
  error: boolean;
}

function mapApiBed(dto: BedDto): Bed {
  return {
    id: dto.id || '',
    name: dto.name,
    gardenId: dto.gardenId,
    open: false,
    plants: dto.plants.map((p: PlantSummaryDto) => ({
      id: p.id || '',
      nameFr: p.nameFr,
      binomialName: p.binomialName,
      emoji: p.emoji,
      family: p.family,
    })),
  };
}

export const BedsStore = signalStore(
  { providedIn: 'root' },
  withState<BedsState>({
    beds: [],
    loading: false,
    loaded: false,
    error: false,
  }),
  withMethods((store, client = inject(Client)) => ({
    async loadBeds(gardenId: string): Promise<void> {
      patchState(store, { beds: [], loading: true, error: false });
      try {
        const beds = await firstValueFrom(client.listBeds(gardenId));
        patchState(store, {
          beds: beds.map(mapApiBed),
          loaded: true,
          loading: false,
        });
      } catch {
        patchState(store, { error: true, loading: false, loaded: false });
      }
    },
    async createBed(gardenId: string, name: string): Promise<void> {
      patchState(store, { loading: true, error: false });
      try {
        const bed = await firstValueFrom(client.createBed({ gardenId, name }));
        patchState(store, {
          beds: [...store.beds(), mapApiBed(bed)],
        });
      } catch {
        patchState(store, { error: true });
      } finally {
        patchState(store, { loading: false });
      }
    },
    async updateBed(id: string, name: string, plantIds: string[]): Promise<void> {
      patchState(store, { loading: true, error: false });
      try {
        const bed = await firstValueFrom(client.updateBed(id, { name, plantIds }));
        patchState(store, {
          beds: store.beds().map((existing) =>
            existing.id === id
              ? {
                  ...existing,
                  name: bed.name,
                  gardenId: bed.gardenId,
                  plants: mapApiBed(bed).plants,
                }
              : existing,
          ),
        });
      } catch {
        patchState(store, { error: true });
      } finally {
        patchState(store, { loading: false });
      }
    },
    async deleteBed(id: string): Promise<void> {
      patchState(store, { loading: true, error: false });
      try {
        await firstValueFrom(client.deleteBed(id));
        patchState(store, { beds: store.beds().filter((existing) => existing.id !== id) });
      } catch {
        patchState(store, { error: true });
      } finally {
        patchState(store, { loading: false });
      }
    },
    toggleBed(id: string): void {
      patchState(
        store,
        { beds: store.beds().map((bed) => (bed.id === id ? { ...bed, open: !bed.open } : bed)) },
      );
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
