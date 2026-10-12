import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  imports: [TranslatePipe, RouterOutlet],
  selector: 'app-encyclopedia',
  templateUrl: './encyclopedia.html',
})
export class EncyclopediaComponent {}
