import { Component, inject, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmComboboxImports } from '@spartan-ng/helm/combobox';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { PlantSummaryDto } from '../../api/api-client.generated';
import { EncyclopediaStore } from './encyclopedia.store';

@Component({
  imports: [TranslatePipe, HlmComboboxImports, HlmLabelImports],
  selector: 'app-encyclopedia-search',
  templateUrl: './encyclopedia-search.html',
})
export class EncyclopediaSearchComponent {
  private readonly store = inject(EncyclopediaStore);

  readonly plants = this.store.plants;
  readonly selected = output<PlantSummaryDto>();

  readonly searchedText = (plant: PlantSummaryDto): string =>
    `${plant.nameFr} ${plant.binomialName ?? ''}`;

  select(plant: PlantSummaryDto | null | undefined): void {
    if (plant) {
      this.selected.emit(plant);
    }
  }
}
