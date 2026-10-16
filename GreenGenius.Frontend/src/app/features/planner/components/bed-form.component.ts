import { Component, inject, input, linkedSignal, output } from '@angular/core';
import { form, FormField, FormRoot, required } from '@angular/forms/signals';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { BedsStore } from '../beds.store';

@Component({
  imports: [FormField, FormRoot, TranslatePipe, HlmButtonImports, HlmDialogImports, HlmFieldImports, HlmInputImports],
  selector: 'app-bed-form',
  templateUrl: './bed-form.component.html',
})
export class BedFormComponent {
  private readonly bedsStore = inject(BedsStore);

  readonly bedId = input<string>();
  readonly gardenId = input.required<string>();
  readonly saved = output<void>();
  readonly cancelled = output<void>();

  readonly bedModel = linkedSignal(() => {
    const bed = this.bedsStore.beds().find((b) => b.id === this.bedId());
    return { name: bed?.name ?? '' };
  });

  readonly bedForm = form(this.bedModel, (path) => {
    required(path.name, { message: 'beds.form.name.required' });
  }, { submission: { action: () => this.submit() } });

  private async submit(): Promise<void> {
    const name = this.bedModel().name;
    const bedId = this.bedId();
    const success = bedId
      ? await this.bedsStore.updateBed(this.gardenId(), bedId, name)
      : await this.bedsStore.createBed(this.gardenId(), name);
    if (success) {
      this.saved.emit();
    }
  }
}
