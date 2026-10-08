import { Directive, input, signal } from '@angular/core';
import { injectExposesStateProvider } from '@spartan-ng/brain/core';
import { classes } from '@spartan-ng/helm/utils';

@Directive({
  selector: '[hlmAlertDialogContent],hlm-alert-dialog-content',
  host: {
    'data-slot': 'alert-dialog-content',
    '[attr.data-state]': 'state()',
    '[attr.data-size]': 'size()',
  },
})
export class HlmAlertDialogContent {
  private readonly _stateProvider = injectExposesStateProvider({ optional: true, host: true });
  public readonly state = this._stateProvider?.state ?? signal('closed');

  public readonly size = input<'sm' | 'default'>('default');

  constructor() {
    classes(
      () =>
        'data-open:animate-in data-closed:animate-out data-closed:fade-out-0 data-open:fade-in-0 data-closed:zoom-out-95 data-open:zoom-in-95 bg-popover text-popover-foreground border-border gap-md rounded-lg border p-md text-sm shadow-sm duration-100 max-w-full data-[size=default]:sm:max-w-sm data-[size=sm]:sm:max-w-xs group/alert-dialog-content relative mx-auto grid w-full outline-none sm:mx-0',
    );
  }
}
