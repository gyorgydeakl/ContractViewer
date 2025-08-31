import { Routes } from '@angular/router';
import {Login} from './login/login';
import {ContractList} from './contract-list';
import {PoaList} from './poa-list';
import {CacheManager} from './cache-manager/cache-manager';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'contracts', component: ContractList },
  { path: 'cache', component: CacheManager },
  { path: 'login', component: Login },
  { path: 'poas', component: PoaList },
];
