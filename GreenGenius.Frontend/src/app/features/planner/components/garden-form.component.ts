import { Component, inject, input, output, linkedSignal } from '@angular/core';
import { form, FormField, FormRoot, maxLength, required } from '@angular/forms/signals';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { GardensStore } from '../../../shared/stores/gardens.store';
import { GardenDto } from '../../../api/api-client.generated';

const NAME_MAX_LENGTH = 250;

@Component({
  imports: [FormField, FormRoot, TranslatePipe, HlmButtonImports, HlmDialogImports, HlmFieldImports, HlmInputImports],
  selector: 'app-garden-form',
  templateUrl: './garden-form.component.html',
})
export class GardenFormComponent {
  private readonly gardensStore = inject(GardensStore);

  readonly garden = input<GardenDto>();
  readonly saved = output<void>();
  readonly cancelled = output<void>();

  readonly gardenModel = linkedSignal(() => ({ name: this.garden()?.name ?? '' }));

  readonly gardenForm = form(this.gardenModel, (path) => {
    required(path.name, { message: 'gardens.form.name.required' });
    maxLength(path.name, NAME_MAX_LENGTH, { message: 'gardens.form.name.too-long' });
  }, { submission: { action: () => this.submit() } });

  private async submit(): Promise<void> {
    const name = this.gardenModel().name;
    const garden = this.garden();
    if (garden?.id) {
      await this.gardensStore.updateGarden(garden.id, name);
    } else {
      await this.gardensStore.createGarden(name);
    }
    this.saved.emit();
  }
}
