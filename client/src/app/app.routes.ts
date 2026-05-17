import { Routes } from '@angular/router';
import { HomeComponent } from './home/home';
import { InvoiceListComponent } from './invoices/invoice-list/invoice-list';
import { InvoiceFormComponent } from './invoices/invoice-form/invoice-form';
import { authGuard } from './service/auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'invoices', component: InvoiceListComponent, canActivate: [authGuard] },
  { path: 'invoices/new', component: InvoiceFormComponent, canActivate: [authGuard] },
  { path: 'invoices/:id', component: InvoiceFormComponent, canActivate: [authGuard] }
];
