import {ChangeDetectionStrategy, Component, computed, inject} from '@angular/core';
import { ContractSummary, ContractViewerApiClient, DocumentDto } from '../../client';
import { resourceObs, resourceObsNoParams } from '../utils';

// PrimeNG
import { Button } from 'primeng/button';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Message } from 'primeng/message';
import { TableModule } from 'primeng/table';
import { DatePipe } from '@angular/common';

type JoinedDoc = DocumentDto & {
  registrationNumber?: string | null;
};

@Component({
  selector: 'app-document-list',
  imports: [Button, ProgressSpinner, Message, TableModule, DatePipe],
  templateUrl: './document-list.html',
  styleUrl: './document-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'mx-auto max-w-6xl py-5'
  }
})
export class DocumentList {
  private readonly client = inject(ContractViewerApiClient);

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
