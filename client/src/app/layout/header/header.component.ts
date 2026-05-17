import { MatToolbar } from '@angular/material/toolbar';
import { Component, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { AuthService } from '../../service/auth.service';
import {
  AuthDialogComponent,
  AuthDialogMode
} from '../../auth/auth-dialog/auth-dialog.component';

@Component({
  selector: 'app-header',
  imports: [
    MatToolbar,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  protected auth = inject(AuthService);

  openAuthDialog(mode: AuthDialogMode): void {
    this.dialog
      .open(AuthDialogComponent, {
        data: { mode },
        width: '380px',
        autoFocus: 'first-tabbable'
      })
      .afterClosed()
      .subscribe(success => {
        if (success) {
          const message = mode === 'login' ? 'Prihlásenie úspešné' : 'Registrácia úspešná';
          this.snackBar.open(message, 'OK', { duration: 2500 });
        }
      });
  }

  logout(): void {
    this.auth.logout().subscribe({
      next: () => {
        this.snackBar.open('Odhlásenie úspešné', 'OK', { duration: 2500 });
        this.router.navigateByUrl('/');
      },
      error: () => {
        this.snackBar.open('Odhlásenie zlyhalo', 'OK', { duration: 2500 });
      }
    });
  }
}
