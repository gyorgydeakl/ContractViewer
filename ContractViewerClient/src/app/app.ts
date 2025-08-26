import {ChangeDetectionStrategy, Component} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {Navbar} from './navbar';
import {Toast} from 'primeng/toast';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, Toast],
  template: `
    <app-navbar></app-navbar>
    <router-outlet/>
    <p-toast />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {

}
