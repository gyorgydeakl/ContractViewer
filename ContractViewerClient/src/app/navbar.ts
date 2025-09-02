import {ChangeDetectionStrategy, Component, signal, viewChild} from '@angular/core';
import { RouterLink } from '@angular/router';
import { MenubarModule } from 'primeng/menubar';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import type { MenuItem } from 'primeng/api';
import {CacheViewer} from './cache-viewer/cache-viewer';

@Component({
  selector: 'app-navbar',
  imports: [MenubarModule, ButtonModule, DialogModule, RouterLink, CacheViewer, CacheViewer],
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
          label="View Cache"
          icon="pi pi-database"
          (onClick)="openCacheViewer(true)"
          [rounded]="true"
        />
      </ng-template>
    </p-menubar>

    <p-dialog
      header="Cache"
      [modal]="true"
      [draggable]="false"
      [resizable]="false"
      [visible]="cacheEditorOpen()"
      (visibleChange)="openCacheViewer($event)"
      [style]="{ width: '70vw', maxWidth: '90%' }"
      [breakpoints]="{ '960px': '85vw', '640px': '95vw' }"
    >
      <app-cache-viewer #cacheViewer/>
    </p-dialog>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block sticky top-0 z-50 bg-primary text-primary-contrast shadow'
  }
})
export class Navbar {
  readonly cacheEditorOpen = signal(false);
  readonly cacheViewer = viewChild.required<CacheViewer>("cacheViewer");

  readonly items = signal<MenuItem[]>([
    { label: 'Login', routerLink: '/login' },
    { label: 'Contracts', routerLink: '/contracts' },
    { label: 'POAs', routerLink: '/poas' },
    { label: 'Cache Manager', routerLink: '/cache' },
  ]);

  openCacheViewer(value: boolean) {
    this.cacheEditorOpen.set(value);
    if (value) {
      this.cacheViewer().reload();
    }
  }
}
