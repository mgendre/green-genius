import { Component, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmButton } from '@spartan-ng/helm/button';
import { HlmCard, HlmCardContent, HlmCardDescription, HlmCardFooter, HlmCardHeader, HlmCardTitle } from '@spartan-ng/helm/card';
import { GardensStore } from '../../../shared/stores/gardens.store';
import { PlannerStore } from '../planner.store';

@Component({
  imports: [TranslatePipe, HlmButton, HlmCard, HlmCardContent, HlmCardDescription, HlmCardFooter, HlmCardHeader, HlmCardTitle],
  selector: 'app-planner-gardens-list',
  templateUrl: './planner-gardens-list.component.html',
})
export class PlannerGardensListComponent {
  readonly plannerStore = inject(PlannerStore);
  readonly gardensStore = inject(GardensStore);

  addGarden(): void {
    alert('Tâche en cours de développement');
  }
}
