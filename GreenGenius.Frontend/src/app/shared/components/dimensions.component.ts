import { Component, input } from '@angular/core';
import { NgIcon } from '@ng-icons/core';
import { TranslatePipe } from '@ngx-translate/core';
import { Badge, Row } from './identities.component';

@Component({
  imports: [NgIcon, TranslatePipe],
  selector: 'app-dimensions',
  templateUrl: './dimensions.html',
})
export class DimensionsComponent {
  readonly title = input<string>('');
  readonly items = input<Row[]>([]);

  private dropIndices(count: number | undefined): number[] {
    return Array.from({ length: count ?? 1 }, (_, i) => i);
  }
}
