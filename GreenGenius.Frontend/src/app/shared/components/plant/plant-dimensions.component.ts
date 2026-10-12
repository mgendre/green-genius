import { Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PlantDto } from '../../../api/api-client.generated';

@Component({
  imports: [TranslatePipe],
  selector: 'app-plant-dimensions',
  templateUrl: './plant-dimensions.html',
})
export class PlantDimensionsComponent {
  readonly plant = input.required<PlantDto>();
}
