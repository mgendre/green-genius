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
      effect(() => {
        if (store.selectedGardenId() !== undefined) return;
      });
      const gardensStore = inject(GardensStore);
      effect(() => {
        if (!gardensStore.loaded()) return;
        const ids = gardensStore.gardens().map((g) => g.id);
        if (!ids.includes(store.selectedGardenId())) {
          patchState(store, {
            selectedGardenId: gardensStore.gardens()[0]?.id,
          });
        }
      });
    },
  }),
);
