import { Routes } from '@angular/router';
import { HomeComponent } from './home/home';
import { InvoiceListComponent } from './invoices/invoice-list/invoice-list';
import { InvoiceFormComponent } from './invoices/invoice-form/invoice-form';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'invoices', component: InvoiceListComponent },
  { path: 'invoices/new', component: InvoiceFormComponent },
  { path: 'invoices/:id', component: InvoiceFormComponent }
];
