import { Component, Input, OnInit, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { AsyncPipe } from '@angular/common';
import { Observable } from 'rxjs';
import { map, startWith, debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-autocomplete-column',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    AsyncPipe
  ],
  templateUrl: './autocomplete-column-component.html',
  styleUrl: './autocomplete-column-component.css',
})
export class AutocompleteColumnComponent implements OnInit {
  @Input() label!: string;
  @Input() placeholder: string = 'Pick one';
  @Input() options: string[] | number[] = [];
  @Input() debounce: number = 2000;
  @Output() valueChanged = new EventEmitter<string | number>();

  control = new FormControl('');
  filteredOptions!: Observable<Array<string | number>>;

  ngOnInit() {
    this.filteredOptions = this.control.valueChanges.pipe(
      startWith(''),
      map(value => this._filter(value))
    );

    this.control.valueChanges.pipe(
      debounceTime(this.debounce),
      distinctUntilChanged()
    ).subscribe(value => {
      if (typeof value === 'string') {
        this.valueChanged.emit(value);
      }
    });
  }

  onOptionSelected(event: any) {
    const value = event.option && event.option.value;
    if (value !== undefined && value !== null) {
      this.valueChanged.emit(value);
    }
  }

  private _filter(value: any): Array<string | number> {
    const filterValue = value?.toString().toLowerCase() || '';
    return this.options.filter(o => o.toString().toLowerCase().includes(filterValue));
  }
}
