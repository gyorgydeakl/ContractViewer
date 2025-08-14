import {ChangeDetectionStrategy, Component, computed, inject, signal} from '@angular/core';
import {ContractSummary, ContractViewerApiClient} from '../../client';
import {resourceObs} from '../utils';
import {TableModule} from 'primeng/table';
import {IconField} from 'primeng/iconfield';
import {InputIcon} from 'primeng/inputicon';
import {InputText} from 'primeng/inputtext';
import {Button} from 'primeng/button';
import {ProgressSpinner} from 'primeng/progressspinner';

@Component({
  selector: 'app-contract-list',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    InputText,
    Button,
    ProgressSpinner
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
  protected readonly userId = signal<string>('');
  protected readonly userInput = signal<string>('');

  // server resource -> depends on userId
  protected readonly contracts = resourceObs(this.userId, (id) => this.client.getContracts(id));

  // local UI state
  private readonly query = signal<string>('');
  private readonly pageIndex = signal(0);
  protected readonly rowsPerPage = signal(10);

  // derived rows
  private readonly allContracts = computed<ContractSummary[]>(() => this.contracts.value() ?? []);

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

  // actions
  protected applyUser() {
    const id = this.userInput().trim();
    if (id && id !== this.userId()) {
      // reset paging when switching user
      this.pageIndex.set(0);
      this.userId.set(id);
    }
  }

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
}
