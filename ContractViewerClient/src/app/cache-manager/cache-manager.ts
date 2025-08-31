import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { resourceObsNoParams } from '../utils';

import { ProgressSpinner } from 'primeng/progressspinner';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfirmPopup } from 'primeng/confirmpopup';
import { Button } from 'primeng/button';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { InputText } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { Dialog } from 'primeng/dialog';
import { Tooltip } from 'primeng/tooltip';

import { ContractViewerClient } from '../../contract-viewer-client';
import {OtherServiceClient} from '../../other-service-client';
import {Select} from 'primeng/select';
import {Textarea} from 'primeng/textarea';

type KV = { key: string; value: string };
interface Cache {
  [key: string]: string;
}

type TenantKey = 'contract viewer' | 'other service';

@Component({
  selector: 'app-cache-editor-tenant',
  imports: [
    FormsModule,
    ProgressSpinner,
    ConfirmPopup,
    Button,
    IconField,
    InputIcon,
    InputText,
    TableModule,
    Dialog,
    Tooltip,
    Select,
    Textarea,
  ],
  templateUrl: './cache-manager.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block p-5',
  },
})
export class CacheManager {
  private readonly msg = inject(MessageService);
  private readonly confirm = inject(ConfirmationService);

  // Underlying API clients (per-tenant)
  private readonly cvClient = inject(ContractViewerClient);
  private readonly otherClient = inject(OtherServiceClient);

  // Tenant selection state
  protected readonly tenantSelection = 'contract viewer' as TenantKey;
  protected readonly tenant = signal<TenantKey>(this.tenantSelection);

  protected readonly tenantOptions = [
    { label: 'Contract Viewer', value: 'contract viewer' as TenantKey },
    { label: 'Other Service',   value: 'other service'   as TenantKey }
  ];

  // Active client depends on tenant selection
  private readonly client = computed(() => {
    return this.tenant() === 'contract viewer'
      ? this.cvClient
      : (this.otherClient ?? this.cvClient);
  });

  // Data + UI state
  protected readonly cache = resourceObsNoParams<Cache>(() => this.client().getCache());
  protected readonly query = signal('');
  protected readonly viewerOpen = signal(false);
  protected readonly viewerTitle = signal('Viewer');
  protected readonly viewerContent = signal('');
  protected readonly viewerIsJson = signal(false);

  // Add item dialog state
  protected readonly addOpen = signal(false);
  protected readonly addKey = signal('');
  protected readonly addValue = signal('');

  constructor() {
    // When tenant changes, reload the cache from the new client
    effect(() => {
      this.tenant();
      this.cache.reload();
    });
  }

  protected onTenantChange(value: TenantKey) {
    this.tenant.set(value);
  }

  protected readonly filteredRows = computed<KV[]>(() => {
    const q = this.query().trim().toLowerCase();
    if (!q) return this.rows();
    return this.rows().filter(r => r.key.toLowerCase().includes(q) || r.value.toLowerCase().includes(q));
  });

  private readonly rows = computed<KV[]>(() => {
    const m = this.cache.value();
    if (!m) return [];
    return Object.entries(m)
      .map(([key, value]) => ({ key, value }))
      .sort((a, b) => a.key.localeCompare(b.key));
  });

  // Cache ops (tenant-aware via this.client())
  protected clearCache() {
    this.client().clearCache().subscribe({
      next: _ => {
        this.msg.add({ severity: 'success', summary: 'Cache cleared' });
        this.cache.reload();
      },
      error: error =>
        this.msg.add({ severity: 'error', summary: 'Error', detail: error?.message ?? 'Failed to clear cache' })
    });
  }

  protected deleteCacheItem(key: string) {
    this.client().deleteCacheItem(key).subscribe({
      next: _ => {
        this.msg.add({ severity: 'success', summary: 'Deleted', detail: key });
        this.cache.reload();
      },
      error: error =>
        this.msg.add({ severity: 'error', summary: 'Error', detail: error?.message ?? `Failed to delete "${key}"` })
    });
  }

  protected confirmDelete(event: Event, key: string) {
    this.confirm.confirm({
      target: event.currentTarget as EventTarget,
      message: `Delete cache item "${key}"?`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', text: true },
      accept: () => this.deleteCacheItem(key)
    });
  }

  protected confirmClear(event: Event) {
    this.confirm.confirm({
      target: event.currentTarget as EventTarget,
      message: 'Clear ALL cache items?',
      icon: 'pi pi-trash',
      acceptLabel: 'Clear All',
      rejectLabel: 'Cancel',
      acceptButtonProps: { severity: 'danger' },
      rejectButtonProps: { severity: 'secondary', text: true },
      accept: () => this.clearCache()
    });
  }

  reload() {
    this.cache.reload();
  }

  protected openViewer(value: unknown, label: string) {
    const { pretty, isJson } = this.formatForViewer(value);
    this.viewerTitle.set(`${label}${isJson ? '' : ' (raw)'}`);
    this.viewerContent.set(pretty);
    this.viewerIsJson.set(isJson);
    this.viewerOpen.set(true);
  }

  private formatForViewer(value: unknown): { pretty: string; isJson: boolean } {
    // If it's a string, try to parse as JSON
    if (typeof value === 'string') {
      try {
        const parsed = JSON.parse(value);
        return { pretty: JSON.stringify(parsed, null, 2), isJson: true };
      } catch {
        // Not JSON — show raw string
        return { pretty: value, isJson: false };
      }
    }
    // Non-string — pretty-print as JSON when possible
    try {
      return { pretty: JSON.stringify(value, null, 2), isJson: true };
    } catch {
      return { pretty: String(value), isJson: false };
    }
  }

  protected async copyViewer() {
    try {
      await navigator.clipboard.writeText(this.viewerContent());
    } catch {
      // no-op; clipboard might be blocked
    }
  }

  // --- Add item flow ---
  protected openAddDialog() {
    this.addKey.set('');
    this.addValue.set('');
    this.addOpen.set(true);
  }

  protected canSaveAdd(): boolean {
    return this.addKey().trim().length > 0;
  }

  protected saveAdd() {
    const key = this.addKey().trim();
    const value = this.addValue();

    if (!key) return;

    this.client().addCacheItem({ key, value }).subscribe({
      next: _ => {
        this.msg.add({ severity: 'success', summary: 'Added', detail: key });
        this.addOpen.set(false);
        this.cache.reload();
      },
      error: error =>
        this.msg.add({ severity: 'error', summary: 'Error', detail: error?.message ?? `Failed to add "${key}"` })
    });
  }
}
