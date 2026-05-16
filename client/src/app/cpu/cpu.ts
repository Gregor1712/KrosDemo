// Old solution — replaced by AutocompleteColumnComponent usage in invoice-form.
// Kept for reference; not registered in any route.
/*
import {ChangeDetectorRef, Component, inject, OnInit, ViewChild, AfterViewInit, OnDestroy} from '@angular/core';
import {
  ConditionType,
  CpuClient,
  CpuDTO,
  PagedResponseOfListOfCpuDTO,
  SortType,
  SwaggerException
} from '../service/api-client';
import {MatTableDataSource} from '@angular/material/table';
import {MatTableModule} from '@angular/material/table';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatAutocompleteModule} from '@angular/material/autocomplete';
import {AutocompleteColumnComponent} from '../shared/components/autocomplete-column-component/autocomplete-column-component';
import {MatSort, MatSortModule} from '@angular/material/sort';
import {MatPaginator, MatPaginatorModule, PageEvent} from '@angular/material/paginator';
import {FilterParams} from '../shared/models/FilterParams';
import {Subscription} from 'rxjs';
import {CommonModule} from '@angular/common';
import {SafeUnsubscribeComponent} from '../shared/abstract/SafeUnsubscribeComponent';
import {MatDialog} from '@angular/material/dialog';
import {ErrorDialogComponent} from '../shared/components/error-dialog/error-dialog.component';
import {getErrorMessage} from '../shared/utils/error-message.util';
import {ApiError} from '../shared/models/apiError';

@Component({
  selector: 'app-cpu',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    AutocompleteColumnComponent,
    MatSortModule,
    MatPaginatorModule
  ],
  templateUrl: './cpu.html',
  styleUrls: ['./cpu.css'],
})
export class CpuComponent extends SafeUnsubscribeComponent implements OnInit, AfterViewInit, OnDestroy {
  private cpuService = inject(CpuClient);
  private dialog = inject(MatDialog);
  private cdr = inject(ChangeDetectorRef);
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  displayedColumns: string[] = ['name', 'socket', 'cores'];
  filterColumns: string[] = ['name-filter', 'socket-filter', 'cores-filter'];
  cpu?: PagedResponseOfListOfCpuDTO;
  dataSource: MatTableDataSource<CpuDTO>;
  filter: FilterParams = {};
  private _sortSub?: Subscription;

  nameOptions = ['AMD Ryzen 5 3600', 'AMD Ryzen 7 5800X', 'Intel Core i9-11900K'];
  socketOptions = ['LGA1700', 'AM4', 'AM5'];
  coresOptions = [8,12,16];
  private loading: boolean = false;

  constructor() {
    super();
    this.dataSource = new MatTableDataSource<CpuDTO>([]);
  }

  ngOnInit() {}

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
    this.paginator.pageIndex = 0; // reset to first page on sort
    this.getCPU();
    this._sortSub = this.sort.sortChange.subscribe(() => {
      this.getCPU();
    });
  }

  onColumnFilter(column: keyof FilterParams, value: any) {
    const val = value?.toString().trim();
    if (!val) {
      this.filter[column] = undefined;
    } else {
      this.filter[column] = (column === 'cores') ? Number(val) : val;
    }
    this.getCPU();
  }

  getCPU() {
    const pageIndex = (this.paginator?.pageIndex ?? 0) + 1;
    const pageSize = this.paginator?.pageSize ?? 10;

    const sortProperty = (this.sort && this.sort.direction)
      ? this.sort.active
      : undefined;

    const sortDirection = (this.sort && this.sort.direction)
      ? this.sort.direction === 'desc' ? SortType.Descending : SortType.Ascending
      : undefined

    const name_Operator = ConditionType.Contains;

    const nameArray: Array<string | null | undefined> = [this.filter?.name];
    const name_Values = nameArray.filter(name => name != null);

    this.cpuService.getCpu(
      name_Operator,
      name_Values,
      sortProperty,
      sortDirection,
      pageIndex,
      pageSize
    ).subscribe({
      next: (response: PagedResponseOfListOfCpuDTO) => {
        if (response?.data) {
          this.dataSource.data = response?.data ?? [];
          this.cpu = response;
          this.cdr.detectChanges();
        }
      },
      error: (err) => {
        let apiError: ApiError | undefined;
        if (SwaggerException.isSwaggerException(err) && err.response) {
          try {
            apiError = JSON.parse(err.response);
          } catch {}
        }

        const errorResult = getErrorMessage(apiError ?? err);
        this.dialog.open(ErrorDialogComponent, {
          width: '800px',
          data: {
            title: 'Unable to load cpu',
            message: errorResult.message,
            details: errorResult.details
          }
        });
      }
    });
  }

  override ngOnDestroy(): void {
    this._sortSub?.unsubscribe();
  }

  protected pageChange(event: PageEvent) {
    this.getCPU();
  }

}
*/
