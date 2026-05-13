import {Component, inject, OnInit} from '@angular/core';
import { MatFormField, MatLabel } from '@angular/material/input';
import {CpuDBO, IPaginationOfCpuDBO, PaginationOfCpuDBO, ProductsClient} from '../service/api-client';
import {Pagination} from '../shared/models/pagination';
import {MatHeaderRowDef, MatRowDef, MatTable, MatTableDataSource} from '@angular/material/table';

import {MatTableModule} from '@angular/material/table';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatPaginator} from '@angular/material/paginator';

@Component({
  selector: 'app-products',
  imports: [
    MatTableModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './products.html',
  styleUrl: './products.css',
})
export class ProductsComponent implements OnInit {
  private shopService = inject(ProductsClient);
  displayedColumns: string[] = ['name'];
  public products?: PaginationOfCpuDBO;
  dataSource: MatTableDataSource<CpuDBO>;

  name: string[] = [];

  constructor() {
    this.dataSource = new MatTableDataSource<CpuDBO>([]);
  }

  ngOnInit() {
    this.initialiseProducts();
  }

  initialiseProducts() {

    this.name.push("AMD Ryzen 5");

    this.getProducts(this.name);
  }

  getProducts(name?: string[]) {

    console.log("Name::", name);

    this.shopService.getProducts(name).subscribe({
      next: response => {
        this.products = response;
        console.log(response);
        if (this.products?.data) {
          this.dataSource.data = this.products.data;
        }
      },
      error: error => console.error(error)
    })
  }

  protected applyFilter($event: KeyboardEvent) {

  }
}
