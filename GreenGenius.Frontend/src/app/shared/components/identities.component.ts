import { Component, input } from '@angular/core';
import { NgIcon } from '@ng-icons/core';
import { TranslatePipe } from '@ngx-translate/core';

export interface Row {
  labelKey: string;
  valueKey?: string;
  value?: string;
  badge?: Badge;
}

export interface Badge {
  kind: 'sunlight' | 'water-need' | 'root-depth' | 'soil-ph' | 'trait';
  iconKey: string;
  textKey: string;
  title: string;
  count?: number;
  params?: Record<string, string | number>;
}

@Component({
  imports: [NgIcon, TranslatePipe],
  selector: 'app-identities',
  templateUrl: './identities.html',
})
export class IdentitiesComponent {
  readonly title = input<string>('');
  readonly items = input<Row[]>([]);

  private dropIndices(count: number | undefined): number[] {
    return Array.from({ length: count ?? 1 }, (_, i) => i);
  }
}
