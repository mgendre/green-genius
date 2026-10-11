import { Component, input } from '@angular/core';
import { NgIcon } from '@ng-icons/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PlantDto } from '../../../api/api-client.generated';

@Component({
  imports: [NgIcon, TranslatePipe],
  selector: 'plant-traits',
  templateUrl: './plant-traits.html',
})
export class PlantTraitsComponent {
  readonly plant = input.required<PlantDto>();
}
