import {computed, effect, inject} from '@angular/core';
import {patchState, signalStore, withComputed, withHooks, withMethods, withState} from '@ngrx/signals';
import {firstValueFrom} from 'rxjs';
import {Client, GardenDto} from '../../api/api-client.generated';
import {GardensStore} from '../../shared/stores/gardens.store'

export interface PlannerState {
  selectedGardenId: string | null;
}

export const PlannerStore = signalStore({providedIn: 'root'},
  withState<PlannerState>({
    selectedGardenId: null
  }),
  withMethods((store, client = inject(Client)) => ({
    selectGarden(id: string): void {
      patchState(store, {selectedGardenId: id});
    }
  })),
  withComputed((store) => ({
    selectedGarden: computed((gardensStore = inject(GardensStore)) => {
      if (!store.selectedGardenId()) {
        return null;
      }
      return gardensStore.getGarden(store.selectedGardenId()!);
    })
  })),
  withHooks({
    onInit(store) {
      const gardensStore = inject(GardensStore);
      effect(() => {
        if (!gardensStore.loaded()) {
          return;
        }
        if (store.selectedGardenId() !== null) {
          return;
        }
        const gardens = gardensStore.gardens();
        if (gardens.length > 0) {
          patchState(store, {selectedGardenId: gardens[0].id});
        }
      });
    },
  }),
);
