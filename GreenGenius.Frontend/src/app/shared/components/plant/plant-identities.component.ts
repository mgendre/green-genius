import { Component, computed, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PlantFamily, PlantLifeCycle, PlantDto } from '../../../api/api-client.generated';

@Component({
  imports: [TranslatePipe],
  selector: 'app-plant-identities',
  templateUrl: './plant-identities.html',
})
export class PlantIdentitiesComponent {
  readonly plant = input.required<PlantDto>();

  readonly familyKey = computed<string>(() => {
    const p = this.plant();
    if (!p.family) return '';
    return FAMILY_LABELS[p.family];
  });

  readonly lifeCycleKey = computed<string>(() => {
    const p = this.plant();
    if (!p.lifeCycle) return '';
    return LIFE_CYCLE_LABELS[p.lifeCycle];
  });
}

const FAMILY_LABELS: Record<PlantFamily, string> = {
  [PlantFamily.Solanaceae]: 'plants.family.solanaceae',
  [PlantFamily.Brassicaceae]: 'plants.family.brassicaceae',
  [PlantFamily.Fabaceae]: 'plants.family.fabaceae',
  [PlantFamily.Apiaceae]: 'plants.family.apiaceae',
  [PlantFamily.Cucurbitaceae]: 'plants.family.cucurbitaceae',
  [PlantFamily.Amaryllidaceae]: 'plants.family.amaryllidaceae',
  [PlantFamily.Asteraceae]: 'plants.family.asteraceae',
  [PlantFamily.Amaranthaceae]: 'plants.family.amaranthaceae',
  [PlantFamily.Poaceae]: 'plants.family.poaceae',
  [PlantFamily.Lamiaceae]: 'plants.family.lamiaceae',
  [PlantFamily.Rosaceae]: 'plants.family.rosaceae',
  [PlantFamily.Polygonaceae]: 'plants.family.polygonaceae',
  [PlantFamily.Other]: 'plants.family.other',
};

const LIFE_CYCLE_LABELS: Record<PlantLifeCycle, string> = {
  [PlantLifeCycle.Annual]: 'plants.life-cycle.annual',
  [PlantLifeCycle.Biennial]: 'plants.life-cycle.biennial',
  [PlantLifeCycle.Perennial]: 'plants.life-cycle.perennial',
};
