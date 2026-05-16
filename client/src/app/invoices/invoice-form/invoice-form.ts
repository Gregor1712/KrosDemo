import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';

import {
  InvoiceCreateDTO,
  InvoiceItemCreateDTO,
  InvoicesClient,
  InvoiceStatus,
  InvoiceUpdateDTO
} from '../../service/api-client';

@Component({
  selector: 'app-invoice-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatProgressBarModule,
    MatDividerModule
  ],
  templateUrl: './invoice-form.html',
  styleUrl: './invoice-form.css'
})
export class InvoiceFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private invoicesClient = inject(InvoicesClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  id = signal<number | null>(null);
  loading = signal(false);
  saving = signal(false);

  isEdit = computed(() => this.id() !== null);

  statusOptions = [
    { value: InvoiceStatus.Issued, label: 'Vystavená' },
    { value: InvoiceStatus.Sent, label: 'Odoslaná' },
    { value: InvoiceStatus.Paid, label: 'Zaplatená' },
    { value: InvoiceStatus.Overdue, label: 'Po splatnosti' },
    { value: InvoiceStatus.Cancelled, label: 'Zrušená' }
  ];

  form: FormGroup = this.fb.group({
    invoiceNumber: ['', [Validators.required, Validators.maxLength(40)]],
    customerName: ['', [Validators.required, Validators.maxLength(200)]],
    customerBusinessId: [''],
    issueDate: [this.toIsoDate(new Date()), Validators.required],
    dueDate: [this.toIsoDate(this.addDays(new Date(), 14)), Validators.required],
    status: [InvoiceStatus.Issued, Validators.required],
    currencyCode: ['EUR', [Validators.required, Validators.maxLength(3)]],
    items: this.fb.array<FormGroup>([])
  });

  get items(): FormArray<FormGroup> {
    return this.form.get('items') as FormArray<FormGroup>;
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      this.id.set(id);
      this.loadInvoice(id);
    } else {
      this.addItem();
    }
  }

  private loadInvoice(id: number): void {
    this.loading.set(true);
    this.invoicesClient.getInvoiceById(id).subscribe({
      next: invoice => {
        this.form.patchValue({
          invoiceNumber: invoice.invoiceNumber ?? '',
          customerName: invoice.customerName ?? '',
          customerBusinessId: invoice.customerBusinessId ?? '',
          issueDate: this.toIsoDate(invoice.issueDate ? new Date(invoice.issueDate) : new Date()),
          dueDate: this.toIsoDate(invoice.dueDate ? new Date(invoice.dueDate) : new Date()),
          status: invoice.status ?? InvoiceStatus.Issued,
          currencyCode: invoice.currencyCode ?? 'EUR'
        });

        this.items.clear();
        for (const item of invoice.items ?? []) {
          this.items.push(this.fb.group({
            description: [{ value: item.description ?? '', disabled: true }],
            unit: [{ value: item.unit ?? '', disabled: true }],
            quantity: [{ value: item.quantity ?? 0, disabled: true }],
            unitPrice: [{ value: item.unitPrice ?? 0, disabled: true }],
            vatRate: [{ value: item.vatRate ?? 0, disabled: true }]
          }));
        }

        this.loading.set(false);
      },
      error: err => {
        this.loading.set(false);
        this.snackBar.open('Faktúra sa nenašla', 'OK', { duration: 4000 });
        console.error(err);
        this.router.navigate(['/invoices']);
      }
    });
  }

  addItem(): void {
    if (this.isEdit()) return;
    this.items.push(this.fb.group({
      description: ['', [Validators.required, Validators.maxLength(500)]],
      unit: ['ks', [Validators.required, Validators.maxLength(20)]],
      quantity: [1, [Validators.required, Validators.min(0.0001)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]],
      vatRate: [20, [Validators.required, Validators.min(0), Validators.max(100)]]
    }));
  }

  removeItem(index: number): void {
    if (this.isEdit()) return;
    this.items.removeAt(index);
  }

  itemNet(index: number): number {
    const group = this.items.at(index);
    const qty = Number(group.get('quantity')?.value ?? 0);
    const price = Number(group.get('unitPrice')?.value ?? 0);
    return qty * price;
  }

  itemGross(index: number): number {
    const group = this.items.at(index);
    const vat = Number(group.get('vatRate')?.value ?? 0);
    return this.itemNet(index) * (1 + vat / 100);
  }

  totalNet(): number {
    let total = 0;
    for (let i = 0; i < this.items.length; i++) total += this.itemNet(i);
    return total;
  }

  totalGross(): number {
    let total = 0;
    for (let i = 0; i < this.items.length; i++) total += this.itemGross(i);
    return total;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);

    if (this.isEdit()) {
      const dto = new InvoiceUpdateDTO({
        invoiceNumber: this.form.value.invoiceNumber,
        customerName: this.form.value.customerName,
        customerBusinessId: this.form.value.customerBusinessId || undefined,
        issueDate: this.form.value.issueDate,
        dueDate: this.form.value.dueDate,
        status: this.form.value.status,
        currencyCode: this.form.value.currencyCode
      });

      this.invoicesClient.updateInvoice(this.id()!, dto).subscribe({
        next: () => {
          this.saving.set(false);
          this.snackBar.open('Faktúra uložená', 'OK', { duration: 2500 });
          this.router.navigate(['/invoices']);
        },
        error: err => {
          this.saving.set(false);
          this.snackBar.open('Chyba pri ukladaní', 'OK', { duration: 4000 });
          console.error(err);
        }
      });
    } else {
      const items = this.items.controls.map(c => new InvoiceItemCreateDTO({
        description: c.value.description,
        unit: c.value.unit,
        quantity: c.value.quantity,
        unitPrice: c.value.unitPrice,
        vatRate: c.value.vatRate
      }));

      const dto = new InvoiceCreateDTO({
        invoiceNumber: this.form.value.invoiceNumber,
        customerName: this.form.value.customerName,
        customerBusinessId: this.form.value.customerBusinessId || undefined,
        issueDate: this.form.value.issueDate,
        dueDate: this.form.value.dueDate,
        status: this.form.value.status,
        currencyCode: this.form.value.currencyCode,
        items
      });

      this.invoicesClient.createInvoice(dto).subscribe({
        next: () => {
          this.saving.set(false);
          this.snackBar.open('Faktúra vytvorená', 'OK', { duration: 2500 });
          this.router.navigate(['/invoices']);
        },
        error: err => {
          this.saving.set(false);
          this.snackBar.open('Chyba pri vytváraní', 'OK', { duration: 4000 });
          console.error(err);
        }
      });
    }
  }

  private toIsoDate(d: Date): string {
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  private addDays(d: Date, days: number): Date {
    const r = new Date(d);
    r.setDate(r.getDate() + days);
    return r;
  }
}
