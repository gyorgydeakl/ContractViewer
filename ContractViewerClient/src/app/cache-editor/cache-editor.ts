import {Component, computed, inject, signal} from '@angular/core';
import {resourceObsNoParams} from '../utils';
import {ContractViewerApiClient} from '../../client';
import {ProgressSpinner} from 'primeng/progressspinner';
import {ConfirmationService, MessageService} from 'primeng/api';
import {ConfirmPopup} from 'primeng/confirmpopup';
import {Button} from 'primeng/button';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {InputText} from 'primeng/inputtext';
import {TableModule} from 'primeng/table';

type KV = { key: string; value: string };

@Component({
  selector: 'app-cache-editor',
  imports: [
    ProgressSpinner,
    ConfirmPopup,
    Button,
    IconField,
    InputIcon,
    InputText,
    TableModule
  ],
  templateUrl: './cache-editor.html',
  styleUrl: './cache-editor.css'
})
export class CacheEditor {
  private readonly msg = inject(MessageService);
  private readonly confirm = inject(ConfirmationService);
  private readonly client = inject(ContractViewerApiClient);
  protected readonly cache = resourceObsNoParams<Map<string, string>>(() => this.client.getCache())
  private readonly query = signal<string>('');

  // derive rows from Map and apply search
  protected readonly rows = computed<KV[]>(() => {
    const m = this.cache.value();
    if (!m) return [];
    return Array.from(m.entries())
      .map(([key, value]) => ({ key, value }))
      .sort((a, b) => a.key.localeCompare(b.key));
  });

  protected readonly filteredRows = computed<KV[]>(() => {
    const q = this.query().trim().toLowerCase();
    if (!q) return this.rows();
    return this.rows().filter(r => r.key.toLowerCase().includes(q) || r.value.toLowerCase().includes(q));
  });

  protected onQuery(v: EventTarget | null) {
    this.query.set((v as HTMLInputElement)?.value ?? '');
  }

  protected reload() {
    this.cache.reload();
  }

  protected clearCache() {
    this.client.clearCache().subscribe({
      next: _ => {
        this.msg.add({ severity: 'success', summary: 'Cache cleared' });
        this.cache.reload();
      },
      error: error => this.msg.add({ severity: 'error', summary: 'Error', detail: error?.message ?? 'Failed to clear cache' })
    });
  }

  protected deleteCacheItem(key: string) {
    this.client.deleteCacheItem(key).subscribe({
      next: _ => {
        this.msg.add({ severity: 'success', summary: 'Deleted', detail: key });
        this.cache.reload();
      },
      error: error => this.msg.add({ severity: 'error', summary: 'Error', detail: error?.message ?? `Failed to delete "${key}"` })
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
}
