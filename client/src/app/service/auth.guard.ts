import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { map, of } from 'rxjs';

import { AuthService } from './auth.service';
import { AuthDialogComponent } from '../auth/auth-dialog/auth-dialog.component';

export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const dialog = inject(MatDialog);

  if (auth.isLoggedIn()) {
    return true;
  }

  const home: UrlTree = router.parseUrl('/');

  if (dialog.openDialogs.some(d => d.componentInstance instanceof AuthDialogComponent)) {
    return of(home);
  }

  return dialog
    .open(AuthDialogComponent, {
      data: { mode: 'login' },
      width: '380px',
      autoFocus: 'first-tabbable',
      disableClose: true
    })
    .afterClosed()
    .pipe(
      map(success =>
        success && auth.isLoggedIn() ? router.parseUrl(state.url) : home
      )
    );
};
