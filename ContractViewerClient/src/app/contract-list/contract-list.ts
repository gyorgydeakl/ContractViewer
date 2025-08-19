import {ChangeDetectionStrategy, Component, computed, inject, input, signal} from '@angular/core';
import {ContractSummary, ContractViewerApiClient} from '../../client';
import {resourceObs, resourceObsNoParams} from '../utils';
import {TableModule} from 'primeng/table';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {InputText} from 'primeng/inputtext';
import {Button} from 'primeng/button';
import {ProgressSpinner} from 'primeng/progressspinner';
import {AuthStore} from '../login/auth-store';
import {InputNumber} from 'primeng/inputnumber';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-contract-list',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    InputText,
    Button,
    ProgressSpinner,
    InputNumber
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
  protected readonly auth = inject(AuthStore);
  protected readonly msg = inject(MessageService);
  protected readonly contracts = resourceObsNoParams(() => this.client.getContracts());

  // local UI state
  private readonly query = signal<string>('');
  private readonly pageIndex = signal(0);
  protected readonly rowsPerPage = signal(10);

  // derived rows
  private readonly allContracts = computed<ContractSummary[]>(() => {
    if (!this.contracts.hasValue()) {
      return [];
    }
    return this.contracts.value();
  });

  protected readonly filteredContracts = computed<ContractSummary[]>(() => {
    const q = this.query().trim().toLowerCase();
    if (!q) return this.allContracts();
    return this.allContracts().filter((c) =>
      c.contractId.toLowerCase().includes(q) ||
      c.role.toLowerCase().includes(q) ||
      c.registrationNumber.toLowerCase().includes(q)
    );
  });

  protected readonly pagedContracts = computed<ContractSummary[]>(() => {
    const start = this.pageIndex() * this.rowsPerPage();
    return this.filteredContracts().slice(start, start + this.rowsPerPage());
  });

  protected onQuery(v: string) {
    this.pageIndex.set(0);
    this.query.set(v ?? '');
  }

  protected onPage(e: { first: number; rows: number }) {
    this.rowsPerPage.set(e.rows);
    this.pageIndex.set(Math.floor(e.first / e.rows));
  }

  protected reload() {
    this.contracts.reload();
  }

  getHtmlInputElement($event: Event | null) {
    return $event?.target as HTMLInputElement;
  }

  generateContracts() {
    this.client.generateRandomContracts({count: 5}).subscribe({
      next: value => this.contracts.reload(),
      error: error => this.msg.add({

      })
    })
  }
}
