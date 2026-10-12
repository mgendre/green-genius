import { Component, inject, input, linkedSignal, output } from '@angular/core';
import { form, FormField, FormRoot, maxLength, required } from '@angular/forms/signals';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { BedsStore } from '../beds.store';

const NAME_MAX_LENGTH = 250;

@Component({
  imports: [FormField, FormRoot, TranslatePipe, HlmButtonImports, HlmDialogImports, HlmFieldImports, HlmInputImports],
  selector: 'app-bed-form',
  templateUrl: './bed-form.component.html',
})
export class BedFormComponent {
  private readonly translate = inject(TranslateService);
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
    maxLength(path.name, NAME_MAX_LENGTH, { message: 'beds.form.name.too-long' });
  }, { submission: { action: () => this.submit() } });

  private async submit(): Promise<void> {
    const name = this.bedModel().name;
    const bedId = this.bedId();
    if (bedId) {
      const currentPlants = this.bedsStore.beds().find((b) => b.id === bedId);
      await this.bedsStore.updateBed(bedId, name, currentPlants?.plants.map((p) => p.id) || []);
    } else {
      await this.bedsStore.createBed(this.gardenId(), name);
    }
    if (this.bedsStore.error()) {
      toast.error(this.translate.instant('beds.save.failed') as string);
      return;
    }
    this.saved.emit();
  }
}
