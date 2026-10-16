import { Component } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { PlannerBedsComponent } from '../components/planner-beds.component';
import { PlannerGardensListComponent } from '../components/planner-gardens-list.component';

@Component({
  imports: [TranslatePipe, PlannerGardensListComponent, PlannerBedsComponent],
  selector: 'app-planner',
  templateUrl: './planner.html',
})
export class PlannerComponent {}
