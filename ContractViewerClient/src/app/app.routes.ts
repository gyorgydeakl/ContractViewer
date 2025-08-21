import { Routes } from '@angular/router';
import {ContractList} from './contract-list/contract-list';
import {Login} from './login/login';
import {DocumentList} from './document-list/document-list';

export const routes: Routes = [
  { path: 'contracts', component: ContractList },
  { path: 'login', component: Login },
  { path: 'documents', component: DocumentList },
];
