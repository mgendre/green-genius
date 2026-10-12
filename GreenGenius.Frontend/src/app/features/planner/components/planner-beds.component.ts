import { Component, computed, inject, signal } from '@angular/core';
import { NgIcon } from '@ng-icons/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmAlertDialogImports } from '@spartan-ng/helm/alert-dialog';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmDialogImports } from '@spartan-ng/helm/dialog';
import { PlannerStore } from '../planner.store';
import { BedsStore, BedsState } from '../beds.store';
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
  private readonly translate = inject(TranslateService);
  readonly bedsStore = inject(BedsStore);
  readonly plannerStore = inject(PlannerStore);

  readonly dialogOpen = signal(false);
  readonly deleteDialogOpen = signal(false);
  readonly editedBedId = signal<string | undefined>(undefined);
  readonly deletingBed = signal<BedsState['beds'][number] | undefined>(undefined);
  readonly dialogState = computed(() => (this.dialogOpen() ? 'open' : 'closed'));
  readonly deleteDialogState = computed(() => (this.deleteDialogOpen() ? 'open' : 'closed'));

  toggleBed(bed: BedsState['beds'][number]): void {
    this.bedsStore.toggleBed(bed.id);
  }

  onKeydown(event: KeyboardEvent, bed: BedsState['beds'][number]): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.bedsStore.toggleBed(bed.id);
    }
  }

  addBed(): void {
    this.openEditDialog(undefined);
  }

  editBed(bed: BedsState['beds'][number]): void {
    this.openEditDialog(bed.id);
  }

  askDeleteBed(bed: BedsState['beds'][number]): void {
    this.deletingBed.set(bed);
    this.deleteDialogOpen.set(true);
  }

  async confirmDeleteBed(): Promise<void> {
    const bed = this.deletingBed();
    if (bed?.id) {
      await this.bedsStore.deleteBed(bed.id);
      if (this.bedsStore.error()) {
        toast.error(this.translate.instant('beds.delete.failed') as string);
        this.closeDeleteDialog();
        return;
      }
    }
    this.closeDeleteDialog();
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
