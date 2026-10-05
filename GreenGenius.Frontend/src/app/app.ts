import { Component } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideBookOpen, lucideLayout, lucideSprout } from '@ng-icons/lucide';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  imports: [NgIcon, RouterLink, RouterLinkActive, RouterOutlet, TranslatePipe],
  providers: [provideIcons({ lucideBookOpen, lucideLayout, lucideSprout })],
  selector: 'app-root',
  templateUrl: './app.html',
})
export class App {}
