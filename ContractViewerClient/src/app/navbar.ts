import {ChangeDetectionStrategy, Component, signal, viewChild} from '@angular/core';
import { RouterLink } from '@angular/router';
import { MenubarModule } from 'primeng/menubar';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import type { MenuItem } from 'primeng/api';
import {CacheEditor} from './cache-editor/cache-editor';

@Component({
  selector: 'app-navbar',
  imports: [MenubarModule, ButtonModule, DialogModule, RouterLink, CacheEditor],
  template: `
    <p-menubar
      [model]="items()"
      ariaLabel="Main navigation"
      class="bg-transparent text-primary-contrast border-none! rounded-none!"
    >
      <ng-template #start>
        <a routerLink="/" class="inline-flex items-center gap-2 font-semibold tracking-wide">
          <i class="pi pi-inbox" aria-hidden="true"></i>
          <span>Contract Viewer</span>
          <span class="sr-only">Home</span>
        </a>
      </ng-template>

      <ng-template #end>
        <p-button
          label="Cache Editor"
          icon="pi pi-database"
          (onClick)="openCacheEditor(true)"
          [rounded]="true"
        />
      </ng-template>
    </p-menubar>

    <p-dialog
      header="Cache Editor"
      [modal]="true"
      [draggable]="false"
      [resizable]="false"
      [visible]="cacheEditorOpen()"
      (visibleChange)="openCacheEditor($event)"
      [style]="{ width: '70vw', maxWidth: '90%' }"
      [breakpoints]="{ '960px': '85vw', '640px': '95vw' }"
    >
      <app-cache-editor #cacheEditor/>
    </p-dialog>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block sticky top-0 z-50 bg-primary text-primary-contrast shadow'
  }
})
export class Navbar {
  readonly cacheEditorOpen = signal(false);
  readonly cacheEditor = viewChild.required<CacheEditor>("cacheEditor");

  readonly items = signal<MenuItem[]>([
    { label: 'Login', routerLink: '/login' },
    { label: 'Contracts', routerLink: '/contracts' },
    { label: 'Documents', routerLink: '/documents' },
  ]);

  openCacheEditor(value: boolean) {
    this.cacheEditorOpen.set(value);
    if (value) {
      this.cacheEditor().reload();
    }
  }
}
