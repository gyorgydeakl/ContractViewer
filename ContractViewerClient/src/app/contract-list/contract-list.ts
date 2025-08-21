import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ContractSummary, ContractViewerApiClient } from '../../client';
import { resourceObsNoParams } from '../utils';

// PrimeNG
import { TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { ProgressSpinner } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { Dialog } from 'primeng/dialog';

// Details component
import { ContractDetails } from '../contract-details/contract-details';

@Component({
  selector: 'app-contract-list',
  imports: [
    TableModule,
    Button,
    ProgressSpinner,
    Dialog,
    ContractDetails
  ],
  templateUrl: './contract-list.html',
  styleUrl: './contract-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'mx-auto max-w-6xl py-5'
  }
})
export class ContractList {
  private readonly client = inject(ContractViewerApiClient);
  protected readonly msg = inject(MessageService);

  // Data
  protected readonly contracts = resourceObsNoParams(() => this.client.getContracts());

  // Derived (no search; just expose all)
  protected readonly allContracts = computed<ContractSummary[]>(() => {
    if (!this.contracts.hasValue()) return [];
    return this.contracts.value();
  });

  // Dialog state
  protected readonly selectedContractId = signal<string | null>(null);
  protected readonly detailsOpen = signal(false);

  protected openDetails(id: string) {
    this.selectedContractId.set(id);
    this.detailsOpen.set(true);
  }

  protected reload() {
    this.contracts.reload();
  }

  protected generateContracts() {
    this.client.generateRandomContracts({ count: 5 }).subscribe({
      next: () => this.contracts.reload(),
      error: () => this.msg.add({ severity: 'error', summary: 'Error', detail: 'Failed to generate contracts.' })
    });
  }
}
