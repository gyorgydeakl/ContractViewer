import {ChangeDetectionStrategy, Component, computed, inject} from '@angular/core';
import { Button } from 'primeng/button';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Message } from 'primeng/message';
import { TableModule } from 'primeng/table';
import { DatePipe } from '@angular/common';
import {ContractSummary, ContractViewerClient, DocumentDto} from '../client';
import {resourceObs, resourceObsNoParams} from './utils';

type JoinedDoc = DocumentDto & {
  registrationNumber?: string | null;
};

@Component({
  selector: 'app-document-list',
  imports: [Button, ProgressSpinner, Message, TableModule, DatePipe],
  template: `
    <div class="flex items-center justify-between gap-3 mb-4">
      <div class="text-sm opacity-75">
        Total: {{ joinedDocs().length }}
      </div>
      <div class="flex items-center gap-2">
        <p-button text icon="pi pi-refresh" (onClick)="reloadAll()"></p-button>
        <p-button text icon="pi pi-plus" (onClick)="generateDocuments()"></p-button>
      </div>
    </div>

    @if (contracts.isLoading() || documents.isLoading()) {
      <div class="flex items-center justify-center py-10">
        <p-progress-spinner aria-label="Loading documents"></p-progress-spinner>
      </div>
    } @else if (!contracts.hasValue() || !documents.hasValue()) {
      <div class="flex flex-col items-center gap-3 py-10">
        <p-message severity="warn">Failed to fetch documents or contracts.</p-message>
        <p-button icon="pi pi-refresh" label="Retry" (onClick)="reloadAll()"></p-button>
      </div>
    } @else {
      <p-table
        [value]="joinedDocs()"
        [paginator]="true"
        [rows]="10"
        [rowsPerPageOptions]="[10,25,50]"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first}–{last} of {totalRecords}"
        [tableStyle]="{ 'min-width': '56rem' }"
      >
        <ng-template #header>
          <tr>
            <th class="w-[30%]">Subject</th>
            <th class="w-[16%]">Date</th>
            <th class="w-[22%]">Contract ID</th>
            <th class="w-[22%]">Registration Number</th>
            <th>Document ID</th>
          </tr>
        </ng-template>
        <ng-template #body let-row>
          <tr>
            <td class="break-words">{{ row.subject || '—' }}</td>
            <td>{{ row.date ? (row.date | date:'mediumDate') : '—' }}</td>
            <td class="font-mono break-words">{{ row.contractId || '—' }}</td>
            <td class="break-words">{{ row.registrationNumber || '—' }}</td>
            <td class="font-mono break-words">{{ row.documentId || '—' }}</td>
          </tr>
        </ng-template>
      </p-table>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block mx-auto max-w-6xl py-5'
  }
})
export class DocumentList {
  private readonly client = inject(ContractViewerClient);

  protected readonly contracts = resourceObsNoParams(() => this.client.getContracts());
  private readonly contractIds = computed(() => this.contracts.hasValue() ? this.contracts.value().map(c => c.contractId) : []);
  protected readonly documents = resourceObs(this.contractIds, ids => this.client.getDocuments(ids.join(',')));

  private readonly contractIndex = computed<Map<string, ContractSummary>>(() => {
    if (!this.contracts.hasValue()) return new Map();
    return new Map(this.contracts.value().map(c => [c.contractId, c]));
  });

  protected readonly joinedDocs = computed<JoinedDoc[]>(() => {
    if (!this.contracts.hasValue() || !this.documents.hasValue()) return [];
    const idx = this.contractIndex();
    return this.documents.value().map(d => ({
      ...d,
      registrationNumber: d.contractId ? idx.get(d.contractId)?.registrationNumber ?? null : null
    }));
  });

  protected reloadAll() {
    this.contracts.reload();
    this.documents.reload();
  }

  protected generateDocuments() {
    this.client.generateDocuments({
      contractIds: this.contractIds(),
      count: 5
    }).subscribe({
      next: _ => {
        this.reloadAll();
      }
    })
  }
}
