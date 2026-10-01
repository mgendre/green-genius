import { TuiRoot } from '@taiga-ui/core';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Client } from './api/api-client.generated';

@Component({
  imports: [RouterOutlet, TuiRoot],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App implements OnInit {
  protected readonly title = signal('GreenGenius.Frontend');
  private readonly client = inject(Client);

  ngOnInit(): void {
    this.client.listGardens().subscribe(gardens => console.log(gardens));
  }
}
