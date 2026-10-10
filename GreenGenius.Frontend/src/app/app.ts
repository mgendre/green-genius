import { Component } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideArrowDown, lucideBookOpen, lucideCheck, lucideFlaskConical, lucideChevronsDown, lucideChevronDown, lucideCloud, lucideCloudSun, lucideDroplet, lucideLayout, lucidePencil, lucideSprout, lucideSun, lucideTrash } from '@ng-icons/lucide';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  imports: [NgIcon, RouterLink, RouterLinkActive, RouterOutlet, TranslatePipe],
  providers: [provideIcons({ lucideArrowDown, lucideBookOpen, lucideCheck, lucideFlaskConical, lucideChevronsDown, lucideChevronDown, lucideCloud, lucideCloudSun, lucideDroplet, lucideLayout, lucidePencil, lucideSprout, lucideSun, lucideTrash })],
  selector: 'app-root',
  templateUrl: './app.html',
})
export class App {}
