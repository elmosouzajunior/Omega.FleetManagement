import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('Omega-Fleet-Web');

  // Declarando o signal da view com o valor inicial 'trips'
  protected readonly view = signal('trips');
}
