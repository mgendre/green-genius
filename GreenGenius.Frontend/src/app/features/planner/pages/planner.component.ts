import { Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PlannerGardensListComponent } from '../components/planner-gardens-list.component';

@Component({
  imports: [TranslatePipe, PlannerGardensListComponent],
  selector: 'app-planner',
  templateUrl: './planner.html',
})
export class PlannerComponent {}
