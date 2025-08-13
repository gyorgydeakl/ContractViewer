import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {Button} from 'primeng/button';
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
})
export class App {

}
