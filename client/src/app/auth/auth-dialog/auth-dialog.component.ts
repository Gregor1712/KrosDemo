import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { AuthService } from '../../service/auth.service';

export type AuthDialogMode = 'login' | 'register';

export interface AuthDialogData {
  mode: AuthDialogMode;
}

@Component({
  selector: 'app-auth-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressBarModule
  ],
  templateUrl: './auth-dialog.component.html',
  styleUrl: './auth-dialog.component.scss'
})
export class AuthDialogComponent {
  readonly data = inject<AuthDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<AuthDialogComponent>);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  get title(): string {
    return this.data.mode === 'login' ? 'Prihlásenie' : 'Registrácia';
  }

  get submitLabel(): string {
    return this.data.mode === 'login' ? 'Prihlásiť' : 'Zaregistrovať';
  }

  submit(): void {
    if (this.form.invalid || this.loading()) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);

    const credentials = this.form.getRawValue();
    const request$ =
      this.data.mode === 'login'
        ? this.auth.login(credentials)
        : this.auth.register(credentials);

    request$.subscribe({
      next: () => {
        this.loading.set(false);
        this.dialogRef.close(true);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.errorMessage.set(this.extractMessage(err));
      }
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }

  private extractMessage(err: HttpErrorResponse): string {
    const body = err.error;
    if (body && typeof body === 'object' && typeof body.message === 'string') {
      return body.message;
    }
    if (err.status === 401) {
      return 'Nesprávne meno alebo heslo.';
    }
    if (err.status === 409) {
      return 'Používateľ už existuje.';
    }
    return 'Akciu sa nepodarilo dokončiť. Skúste to znova.';
  }
}
