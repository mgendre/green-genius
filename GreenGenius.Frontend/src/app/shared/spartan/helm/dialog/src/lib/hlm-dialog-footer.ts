import { Directive } from '@angular/core';
import { classes } from '@spartan-ng/helm/utils';

@Directive({
  selector: '[hlmDialogFooter],hlm-dialog-footer',
  host: { 'data-slot': 'dialog-footer' },
})
export class HlmDialogFooter {
  constructor() {
    classes(
      () =>
        'bg-muted/50 -mx-md -mb-md rounded-b-lg border-t border-border p-md flex flex-col-reverse gap-sm sm:flex-row sm:justify-end',
    );
  }
}
