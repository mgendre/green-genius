import { Component, inject } from '@angular/core';
import { TranslateDirective } from '@ngx-translate/core';
import { GardensStore } from '../../../shared/stores/gardens.store';
import { PlannerStore } from '../planner.store';

@Component({
  imports: [TranslateDirective],
  selector: 'app-planner',
  templateUrl: './planner.html',
})
export class PlannerComponent {
  readonly gardensStore = inject(GardensStore);
  readonly plannerStore = inject(PlannerStore);
}
