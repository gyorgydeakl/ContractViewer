import { Component, ChangeDetectionStrategy, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import {ContractViewerClient} from '../../contract-viewer-client';
import { AuthStore } from './auth-store';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-login',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, InputText, Password, Button, Message],
  templateUrl: './login.html',
})
export class Login {
  private readonly client = inject(ContractViewerClient);
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);

  protected readonly email = signal<string>('');
  protected readonly password = signal<string>('');
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly submitted = signal(false);

  protected readonly canSubmit = computed(
    () => !!this.email().trim() && !!this.password().trim()
  );

  protected onLogin(): void {
    this.submitted.set(true);
    this.error.set(null);

    if (!this.canSubmit()) return;

    this.loading.set(true);
    this.client
      .login({ email: this.email().trim(), password: this.password() })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: token => {
          this.auth.setToken(token.accessToken);
          this.router.navigate(['contracts']);
        },
        error: (err: unknown) => {
          const message =
            (err as any)?.error?.message ??
            'Invalid email or password. Please try again.';
          this.error.set(message);
        }
      });
  }
}
