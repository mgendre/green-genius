import { Component, computed, inject, signal } from '@angular/core';
import { NgIcon } from '@ng-icons/core';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmButton } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmAlertDialogImports } from '@spartan-ng/helm/alert-dialog';
import { GardenDto } from '../../../api/api-client.generated';
import { PlannerStore } from '../planner.store';
import { GardensStore } from '../../../shared/stores/gardens.store';
import { GardenFormComponent } from './garden-form.component';

@Component({
  imports: [
    TranslatePipe,
    HlmButton,
    HlmCardImports,
    HlmDialogImports,
    HlmAlertDialogImports,
    GardenFormComponent,
    NgIcon,
  ],
  selector: 'app-planner-gardens-list',
  templateUrl: './planner-gardens-list.component.html',
})
export class PlannerGardensListComponent {
  readonly gardensStore = inject(GardensStore);
  readonly plannerStore = inject(PlannerStore);

  readonly dialogOpen = signal(false);
  readonly deleteDialogOpen = signal(false);
  readonly editedGarden = signal<GardenDto | undefined>(undefined);
  readonly deletingGarden = signal<GardenDto | undefined>(undefined);
  readonly dialogState = computed(() => (this.dialogOpen() ? 'open' : 'closed'));
  readonly deleteDialogState = computed(() => (this.deleteDialogOpen() ? 'open' : 'closed'));

  editing = computed<boolean>(() => this.editedGarden() !== undefined);

  addGarden(): void {
    this.openEditDialog(undefined);
  }

  editGarden(garden: GardenDto): void {
    this.openEditDialog(garden);
  }

  openDeleteDialog(garden: GardenDto): void {
    this.deletingGarden.set(garden);
    this.deleteDialogOpen.set(true);
  }

  async confirmDeleteGarden(): Promise<void> {
    const garden = this.deletingGarden();
    if (!garden?.id) {
      return;
    }
    await this.gardensStore.deleteGarden(garden.id);
    this.closeDeleteDialog();
  }

  closeEditDialog(): void {
    this.dialogOpen.set(false);
  }

  closeDeleteDialog(): void {
    this.deleteDialogOpen.set(false);
    this.deletingGarden.set(undefined);
  }

  private openEditDialog(garden: GardenDto | undefined): void {
    this.editedGarden.set(garden);
    this.deleteDialogOpen.set(false);
    this.dialogOpen.set(true);
  }
}
