import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { AuthService } from './auth.service';
import { AuthDialogComponent } from '../auth/auth-dialog/auth-dialog.component';

function isAuthEndpoint(url: string): boolean {
  return /\/api\/Auth\/(login|register|logout)$/i.test(url);
}

export const unauthorizedInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const dialog = inject(MatDialog);
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (!(err instanceof HttpErrorResponse) || err.status !== 401 || isAuthEndpoint(req.url)) {
        return throwError(() => err);
      }

      const wasLoggedIn = auth.isLoggedIn();
      auth.clearLocalSession();

      if (wasLoggedIn) {
        snackBar.open('Vaše prihlásenie vypršalo. Prihláste sa znova.', 'OK', { duration: 3500 });
      }

      const alreadyOpen = dialog.openDialogs.some(
        d => d.componentInstance instanceof AuthDialogComponent
      );
      if (!alreadyOpen) {
        dialog
          .open(AuthDialogComponent, {
            data: { mode: 'login' },
            width: '380px',
            autoFocus: 'first-tabbable',
            disableClose: true
          })
          .afterClosed()
          .subscribe(success => {
            if (!success) {
              router.navigateByUrl('/');
            }
          });
      }

      return throwError(() => err);
    })
  );
};
