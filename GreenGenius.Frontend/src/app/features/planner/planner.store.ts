import { effect, inject } from '@angular/core';
import { patchState, signalStore, withHooks, withMethods, withState } from '@ngrx/signals';
import { GardensStore } from '../../shared/stores/gardens.store';

export interface PlannerState {
  selectedGardenId: string | undefined;
}

export const PlannerStore = signalStore(
  { providedIn: 'root' },
  withState<PlannerState>({ selectedGardenId: undefined }),
  withMethods((store) => ({
    selectGarden(id: string | undefined): void {
      patchState(store, { selectedGardenId: id });
    },
  })),
  withHooks({
    onInit(store) {
      const gardensStore = inject(GardensStore);
      effect(() => {
        if (!gardensStore.loaded() || store.selectedGardenId() !== undefined) {
          return;
        }
        patchState(store, { selectedGardenId: gardensStore.gardens()[0]?.id });
      });
    },
  }),
);
