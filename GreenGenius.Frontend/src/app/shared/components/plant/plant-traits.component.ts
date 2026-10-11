import { Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PlantDto } from '../../../api/api-client.generated';

@Component({
  imports: [TranslatePipe],
  selector: 'plant-traits',
  templateUrl: './plant-traits.html',
})
export class PlantTraitsComponent {
  readonly plant = input.required<PlantDto>();
}
