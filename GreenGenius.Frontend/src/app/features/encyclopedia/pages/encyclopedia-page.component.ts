import { Component, input, inject } from '@angular/core';
import { Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { PlantSummaryDto } from '../../../api/api-client.generated';
import { PlantIdentityComponent } from '../../../shared/components/plant-identity.component';
import { EncyclopediaSearchComponent } from '../encyclopedia-search.component';

@Component({
  imports: [TranslatePipe, PlantIdentityComponent, EncyclopediaSearchComponent],
  selector: 'app-encyclopedia-page',
  templateUrl: './encyclopedia-page.html',
})
export class EncyclopediaPageComponent {
  private readonly router = inject(Router);
  readonly id = input<string>();

  onPlantSelected(plant: PlantSummaryDto): void {
    if (plant.id) {
      this.router.navigate(['/encyclopedia', plant.id]).finally();
    }
  }
}
