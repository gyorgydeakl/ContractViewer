import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { Button } from 'primeng/button';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { ContractViewerClient, PowerOfAttorneyDto, GeneratePoaRequest } from '../contract-viewer-client';
import { MessageService } from 'primeng/api';
import { resourceObsNoParams } from './utils';
import {firstValueFrom} from 'rxjs';

type PoaMode = 'byMe' | 'forMe';

@Component({
  selector: 'app-poa-list',
  standalone: true,
  imports: [FormsModule, TableModule, Button, ProgressSpinner, Select],
  template: `
    <div class="flex items-center justify-between gap-3 mb-3 flex-wrap">
      <div class="text-sm opacity-75">Total: {{ poasCount() }}</div>

      <div class="flex items-center gap-2">
        <p-select
          [options]="modeOptions"
          [ngModel]="selectedMode()"
          (ngModelChange)="onModeChange($event)"
          class="min-w-56"
        ></p-select>

        <p-button text icon="pi pi-refresh" (onClick)="reloadCurrent()"></p-button>
        <p-button text icon="pi pi-plus" label="Generate" (onClick)="generatePoas()"></p-button>
      </div>
    </div>

    @if (current().isLoading()) {
      <div class="flex items-center justify-center py-10">
        <p-progress-spinner aria-label="Loading POAs"></p-progress-spinner>
      </div>
    } @else if (!current().hasValue()) {
      <div class="flex flex-col items-center gap-4 py-10">
        <p>Failed to fetch powers of attorney.</p>
      </div>
    } @else {
      <p-table
        [value]="current().value()!"
        [paginator]="true"
        [rows]="10"
        [rowsPerPageOptions]="[10,25,50]"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first}–{last} of {totalRecords}"
        [tableStyle]="{ 'min-width': '56rem' }"
      >
        <ng-template #header>
          <tr>
            <th class="w-[22%]">POA ID</th>
            <th class="w-[22%]">Contract ID</th>
            <th class="w-[18%]">{{ selectedMode() === 'byMe' ? 'Delegate User' : 'Grantor User' }}</th>
            <th class="w-[14%]">Valid From</th>
            <th class="w-[14%]">Valid Until</th>
            <th class="w-[10%] text-right">Status</th>
          </tr>
        </ng-template>

        <ng-template #body let-row>
          <tr>
            <td class="break-words">{{ row.id }}</td>
            <td class="break-words">{{ row.contractId }}</td>
            <td class="break-words">
              {{ selectedMode() === 'byMe' ? row.delegateUserId : row.grantorUserId }}
            </td>
            <td class="break-words">{{ row.validFrom || '—' }}</td>
            <td class="break-words">{{ row.validUntil || '—' }}</td>
            <td class="text-right">
              <span
                class="inline-flex items-center px-2 py-1 rounded-full text-xs"
                [class.bg-emerald-100]="row.isActive"
                [class.text-emerald-700]="row.isActive"
                [class.dark:bg-emerald-900]="row.isActive"
                [class.dark:text-emerald-200]="row.isActive"
                [class.bg-zinc-200]="!row.isActive"
                [class.text-zinc-700]="!row.isActive"
                [class.dark:bg-zinc-800]="!row.isActive"
                [class.dark:text-zinc-300]="!row.isActive"
              >
                {{ row.isActive ? 'Active' : 'Revoked/Expired' }}
              </span>
            </td>
          </tr>
        </ng-template>
      </p-table>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block mx-auto max-w-6xl py-5' }
})
export class PoaList {
  private readonly client = inject(ContractViewerClient);
  protected readonly msg = inject(MessageService);

  protected readonly modeOptions = [
    { label: 'Granted by me', value: 'byMe' as PoaMode },
    { label: 'Granted for me', value: 'forMe' as PoaMode }
  ];
  protected readonly selectedMode = signal<PoaMode>('byMe');

  protected readonly poasByMe = resourceObsNoParams(() => this.client.getPoasGrantedByMe());
  protected readonly poasForMe = resourceObsNoParams(() => this.client.getPoasGrantedForMe());

  protected readonly current = computed(() => this.selectedMode() === 'byMe' ? this.poasByMe : this.poasForMe);

  protected readonly poasCount = computed<number>(() => {
    const res = this.current();
    return res.hasValue() ? (res.value() as PowerOfAttorneyDto[]).length : 0;
  });

  protected onModeChange(mode: PoaMode) {
    this.selectedMode.set(mode);
    this.reloadCurrent();
  }

  protected reloadCurrent() {
    (this.selectedMode() === 'byMe' ? this.poasByMe : this.poasForMe).reload();
  }

  protected async generatePoas() {
    const contracts = await firstValueFrom(this.client.getContracts());
    const users = await firstValueFrom(this.client.getUsers());
    const body: GeneratePoaRequest = {
      contractIds: contracts.map(c => c.contractId),
      userIds: users.map(u => u.id ?? ''),
      count: 5
    };

    this.client.generatePoas(body).subscribe({
      next: () => this.reloadCurrent(),
      error: () =>
        this.msg.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to generate POAs.'
        })
    });
  }
}
