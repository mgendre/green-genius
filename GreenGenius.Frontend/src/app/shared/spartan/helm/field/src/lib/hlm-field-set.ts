import { Directive } from '@angular/core';
import { classes } from '@spartan-ng/helm/utils';

@Directive({
  selector: 'fieldset[hlmFieldSet]',
  host: { 'data-slot': 'field-set' },
})
export class HlmFieldSet {
  constructor() {
    classes(
      () =>
        'gap-md has-[>[data-slot=checkbox-group]]:gap-sm has-[>[data-slot=radio-group]]:gap-sm flex flex-col',
    );
  }
}
