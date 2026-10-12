import { Component, inject, input, resource } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { Client } from '../../../api/api-client.generated';
import { PlantIdentitiesComponent } from './plant-identities.component';
import { PlantDimensionsComponent } from './plant-dimensions.component';
import { PlantTraitsComponent } from './plant-traits.component';
import { PlantNeedsComponent } from './plant-needs.component';
import { HlmCardImports } from '@spartan-ng/helm/card';

@Component({
  imports: [TranslatePipe, PlantIdentitiesComponent, PlantDimensionsComponent, PlantTraitsComponent, PlantNeedsComponent, HlmCardImports],
  selector: 'app-plant-identity',
  templateUrl: './plant-identity.html',
})
export class PlantIdentityComponent {
  private readonly client = inject(Client);

  readonly id = input.required<string>();

  readonly plant = resource({
    params: () => this.id(),
    loader: ({ params: id }) => firstValueFrom(this.client.getPlant(id)),
  });
}
