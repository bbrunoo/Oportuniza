import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { PublicationFilterDto } from '../../models/filter.model';
import { MatDialog } from '@angular/material/dialog';
import { FilterComponent } from '../../extras/filter/filter.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-searchbar',
  imports: [FormsModule],
  templateUrl: './searchbar.component.html',
  styleUrl: './searchbar.component.css'
})
export class SearchbarComponent {
  @Output() search = new EventEmitter<PublicationFilterDto>();
  @Input() currentFilters: PublicationFilterDto = {};

  searchTerm: string = '';

  constructor(private dialog: MatDialog) { }

  onSearch() {
    this.currentFilters.searchTerm = this.searchTerm;
    this.search.emit(this.currentFilters);
  }

  openFiltersDialog() {
    const dialogRef = this.dialog.open(FilterComponent, {
      data: this.currentFilters,
    });

    dialogRef.afterClosed().subscribe((result: PublicationFilterDto) => {
      if (result) {
        this.currentFilters = result;
        this.search.emit(this.currentFilters);
      }
    });
  }
}
