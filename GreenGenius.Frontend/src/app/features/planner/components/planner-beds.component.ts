import { Component, computed, inject, signal } from '@angular/core';
import { NgIcon } from '@ng-icons/core';
import { TranslatePipe } from '@ngx-translate/core';
import { HlmAlertDialogImports } from '@spartan-ng/helm/alert-dialog';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmDialogImports } from '@spartan-ng/helm/dialog';
import { BedDto } from '../../../api/api-client.generated';
import { PlannerStore } from '../planner.store';
import { BedsStore } from '../beds.store';
import { BedFormComponent } from './bed-form.component';

@Component({
  imports: [
    TranslatePipe,
    HlmButtonImports,
    HlmCardImports,
    HlmDialogImports,
    HlmAlertDialogImports,
    BedFormComponent,
    NgIcon,
  ],
  selector: 'app-planner-beds',
  templateUrl: './planner-beds.component.html',
  host: { class: 'app-beds-list' },
})
export class PlannerBedsComponent {
  readonly bedsStore = inject(BedsStore);
  readonly plannerStore = inject(PlannerStore);

  readonly dialogOpen = signal(false);
  readonly deleteDialogOpen = signal(false);
  readonly editedBedId = signal<string | undefined>(undefined);
  readonly deletingBed = signal<BedDto | undefined>(undefined);
  readonly dialogState = computed(() => (this.dialogOpen() ? 'open' : 'closed'));
  readonly deleteDialogState = computed(() => (this.deleteDialogOpen() ? 'open' : 'closed'));
  readonly isBedOpen = signal<string | undefined>(undefined);

  toggleBed(bed: BedDto): void {
    this.isBedOpen.update((id) => (id === bed.id ? undefined : bed.id));
  }

  onKeydown(event: KeyboardEvent, bed: BedDto): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.toggleBed(bed);
    }
  }

  isBedOpenAt(bedId: string | undefined): boolean {
    return this.isBedOpen() === bedId;
  }

  addBed(): void {
    this.openEditDialog(undefined);
  }

  editBed(bed: BedDto): void {
    this.openEditDialog(bed.id);
  }

  askDeleteBed(bed: BedDto): void {
    this.deletingBed.set(bed);
    this.deleteDialogOpen.set(true);
  }

  async confirmDeleteBed(): Promise<void> {
    const bed = this.deletingBed();
    const gardenId = this.plannerStore.selectedGardenId();
    if (bed?.id && gardenId && (await this.bedsStore.deleteBed(gardenId, bed.id))) {
      this.closeDeleteDialog();
    }
  }

  closeEditDialog(): void {
    this.dialogOpen.set(false);
    this.editedBedId.set(undefined);
  }

  closeDeleteDialog(): void {
    this.deleteDialogOpen.set(false);
    this.deletingBed.set(undefined);
  }

  private openEditDialog(bedId: string | undefined): void {
    this.editedBedId.set(bedId);
    this.deleteDialogOpen.set(false);
    this.dialogOpen.set(true);
  }
}
