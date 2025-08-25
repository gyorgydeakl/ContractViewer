import { Routes } from '@angular/router';
import {Login} from './login/login';
import {ContractList} from './contract-list';
import {DocumentList} from './document-list';

export const routes: Routes = [
  { path: 'contracts', component: ContractList },
  { path: 'login', component: Login },
  { path: 'documents', component: DocumentList },
];
