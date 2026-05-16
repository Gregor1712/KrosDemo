import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

import {
  ConditionType,
  InvoiceDTO,
  InvoicesClient,
  InvoiceStatus,
  SortType
} from '../../service/api-client';

@Component({
  selector: 'app-invoice-list',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTooltipModule
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

  data = signal<InvoiceDTO[]>([]);
  totalRecords = signal(0);
  loading = signal(false);

  pageIndex = 0;
  pageSize = 10;
  customerFilter = '';
  sortProperty: string | null = null;
  sortDirection: SortType | undefined = undefined;

  InvoiceStatus = InvoiceStatus;

  @ViewChild(MatPaginator) paginator?: MatPaginator;

  private filter$ = new Subject<string>();

  ngOnInit(): void {
    this.filter$
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => {
        this.pageIndex = 0;
        if (this.paginator) {
          this.paginator.pageIndex = 0;
        }
        this.load();
      });
    this.load();
  }

  load(): void {
    this.loading.set(true);

    const nameOp = this.customerFilter ? ConditionType.Contains : undefined;
    const nameValues = this.customerFilter ? [this.customerFilter] : undefined;

    this.invoicesClient
      .getInvoices(
        undefined, undefined,
        nameOp, nameValues,
        undefined, undefined,
        undefined, undefined,
        undefined, undefined,
        undefined, undefined,
        this.sortProperty ?? undefined,
        this.sortDirection,
        this.pageIndex + 1,
        this.pageSize
      )
      .subscribe({
        next: response => {
          this.data.set(response.data ?? []);
          this.totalRecords.set(response.totalRecords ?? 0);
          this.loading.set(false);
        },
        error: err => {
          this.loading.set(false);
          this.snackBar.open('Nepodarilo sa načítať faktúry', 'OK', { duration: 4000 });
          console.error(err);
        }
      });
  }

  onFilterChange(value: string): void {
    this.customerFilter = value;
    this.filter$.next(value);
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
}
