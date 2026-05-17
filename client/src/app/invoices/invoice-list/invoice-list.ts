import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

import {
  ConditionType,
  InvoiceDTO,
  InvoicesClient,
  InvoiceStatus,
  SortType
} from '../../service/api-client';
import {
  AutocompleteColumnComponent
} from '../../shared/components/autocomplete-column-component/autocomplete-column-component';
import { FilterParams } from '../../shared/models/FilterParams';

@Component({
  selector: 'app-invoice-list',
  imports: [
    CommonModule,
    RouterLink,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTooltipModule,
    AutocompleteColumnComponent
  ],
  templateUrl: './invoice-list.html',
  styleUrl: './invoice-list.css'
})
export class InvoiceListComponent implements OnInit {
  private invoicesClient = inject(InvoicesClient);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  displayedColumns = [
    'invoiceNumber',
    'customerName',
    'issueDate',
    'dueDate',
    'status',
    'totalGross',
    'actions'
  ];

  filterColumns = [
    'invoiceNumber-filter',
    'customerName-filter',
    'issueDate-filter',
    'dueDate-filter',
    'status-filter',
    'totalGross-filter',
    'actions-filter'
  ];

  invoiceNumberOptions: string[] = [];
  customerNameOptions: string[] = [];

  data = signal<InvoiceDTO[]>([]);
  totalRecords = signal(0);
  loading = signal(false);

  pageIndex = 0;
  pageSize = 10;
  filter: FilterParams = {};
  sortProperty: string | null = null;
  sortDirection: SortType | undefined = undefined;

  InvoiceStatus = InvoiceStatus;

  @ViewChild(MatPaginator) paginator?: MatPaginator;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);

    const invoiceNumberOp = this.filter.invoiceNumber ? ConditionType.Contains : undefined;
    const invoiceNumberValues = this.filter.invoiceNumber ? [this.filter.invoiceNumber] : undefined;

    const customerNameOp = this.filter.customerName ? ConditionType.Contains : undefined;
    const customerNameValues = this.filter.customerName ? [this.filter.customerName] : undefined;

    const customerBusinessIdOp = this.filter.customerBusinessId ? ConditionType.Contains : undefined;
    const customerBusinessIdValues = this.filter.customerBusinessId ? [this.filter.customerBusinessId] : undefined;

    this.invoicesClient
      .getInvoices(
        invoiceNumberOp,
        invoiceNumberValues,
        customerNameOp,
        customerNameValues,
        customerBusinessIdOp,
        customerBusinessIdValues,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        undefined,
        this.sortProperty ?? undefined,
        this.sortDirection,
        this.pageIndex + 1,
        this.pageSize
      )
      .subscribe({
        next: response => {
          const rows = response.data ?? [];
          this.data.set(rows);
          this.totalRecords.set(response.totalRecords ?? 0);
          this.refreshSuggestions(rows);
          this.loading.set(false);
        },
        error: err => {
          this.loading.set(false);
          this.snackBar.open('Nepodarilo sa načítať faktúry', 'OK', { duration: 4000 });
          console.error(err);
        }
      });
  }

  onColumnFilter(column: keyof FilterParams, value: any): void {
    const val = value?.toString().trim();
    this.filter[column] = val ? val : undefined;

    this.pageIndex = 0;
    if (this.paginator) {
      this.paginator.pageIndex = 0;
    }
    this.load();
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.load();
  }

  onSort(sort: Sort): void {
    if (!sort.direction) {
      this.sortProperty = null;
      this.sortDirection = undefined;
    } else {
      this.sortProperty = sort.active;
      this.sortDirection = sort.direction === 'asc' ? SortType.Ascending : SortType.Descending;
    }
    this.load();
  }

  edit(invoice: InvoiceDTO): void {
    this.router.navigate(['/invoices', invoice.id]);
  }

  remove(invoice: InvoiceDTO): void {
    if (!invoice.id) return;
    if (!confirm(`Naozaj zmazať faktúru ${invoice.invoiceNumber}?`)) return;

    this.invoicesClient.getInvoiceById(invoice.id).subscribe({
      next: () => {
        this.invoicesClient.deleteInvoice(invoice.id!).subscribe({
          next: () => {
            this.snackBar.open('Faktúra zmazaná', 'OK', { duration: 2500 });
            this.load();
          },
          error: err => {
            this.snackBar.open('Chyba pri mazaní', 'OK', { duration: 4000 });
            console.error(err);
          }
        });
      },
      error: err => {
        this.snackBar.open('Chyba pri načítaní pred mazaním', 'OK', { duration: 4000 });
        console.error(err);
      }
    });
  }

  send(invoice: InvoiceDTO): void {
    if (!invoice.id) return;

    this.invoicesClient.getInvoiceById(invoice.id).subscribe({
      next: () => {
        this.invoicesClient.sendInvoice(invoice.id!).subscribe({
          next: () => {
            this.snackBar.open('Faktúra odoslaná', 'OK', { duration: 2500 });
            this.load();
          },
          error: err => {
            this.snackBar.open('Chyba pri odosielaní', 'OK', { duration: 4000 });
            console.error(err);
          }
        });
      },
      error: err => console.error(err)
    });
  }

  statusLabel(status?: InvoiceStatus): string {
    switch (status) {
      case InvoiceStatus.Issued: return 'Vystavená';
      case InvoiceStatus.Sent: return 'Odoslaná';
      case InvoiceStatus.Paid: return 'Zaplatená';
      case InvoiceStatus.Overdue: return 'Po splatnosti';
      case InvoiceStatus.Cancelled: return 'Zrušená';
      default: return '';
    }
  }

  private refreshSuggestions(rows: InvoiceDTO[]): void {
    const numbers = rows.map(r => r.invoiceNumber).filter((s): s is string => !!s);
    const names = rows.map(r => r.customerName).filter((s): s is string => !!s);
    this.invoiceNumberOptions = Array.from(new Set(numbers));
    this.customerNameOptions = Array.from(new Set(names));
  }
}
