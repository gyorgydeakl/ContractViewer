import {ChangeDetectionStrategy, Component, inject, input} from '@angular/core';
import {resourceObs} from '../utils';
import {ContractViewerClient} from '../../contract-viewer-client';
import {Skeleton} from 'primeng/skeleton';
import {Divider} from 'primeng/divider';
import {DatePipe, DecimalPipe} from '@angular/common';
import {Card} from 'primeng/card';
import {PrimeTemplate} from 'primeng/api';
import {Tag} from 'primeng/tag';
import {Message} from 'primeng/message';

@Component({
  selector: 'app-contract-details',
  imports: [
    Skeleton,
    Divider,
    DecimalPipe,
    DatePipe,
    Card,
    PrimeTemplate,
    Tag,
    Message
  ],
  templateUrl: './contract-details.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContractDetails {
  private readonly client = inject(ContractViewerClient);
  readonly contractId = input.required<string>();

  protected readonly contract = resourceObs(this.contractId, id => this.client.getContract(id));
}
