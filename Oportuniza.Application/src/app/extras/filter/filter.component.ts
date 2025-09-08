import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { PublicationFilterDto } from '../../models/filter.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-filter',
  imports: [CommonModule, FormsModule],
  templateUrl: './filter.component.html',
  styleUrl: './filter.component.css',
})
export class FilterComponent {
  filters: PublicationFilterDto = {
    local: '',
    contracts: [],
    shifts: [],
    salaryRange: null,
  };

  constructor(
    public dialogRef: MatDialogRef<FilterComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PublicationFilterDto
  ) {
    this.filters = {
      searchTerm: data?.searchTerm ?? '',
      local: data?.local ?? '',
      contracts: data?.contracts ?? [],
      shifts: data?.shifts ?? [],
      salaryRange: data?.salaryRange ?? null,
    };
  }

  toggleContract(contract: string) {
    this.filters.contracts = this.filters.contracts ?? [];
    const index = this.filters.contracts.indexOf(contract);
    if (index === -1) {
      this.filters.contracts.push(contract);
    } else {
      this.filters.contracts.splice(index, 1);
    }
  }

  toggleShift(shift: string) {
    this.filters.shifts = this.filters.shifts ?? [];
    const index = this.filters.shifts.indexOf(shift);
    if (index === -1) {
      this.filters.shifts.push(shift);
    } else {
      this.filters.shifts.splice(index, 1);
    }
  }

  toggleSalary(salary: string) {
    this.filters.salaryRange =
      this.filters.salaryRange === salary ? null : salary;
  }

  clearFilters() {
    this.filters.searchTerm = '';
    this.filters.local = '';
    this.filters.contracts = [];
    this.filters.shifts = [];
    this.filters.salaryRange = null;
  }

  apply() {
    const filtersToSend = {
      ...this.filters
    };

    // Normalize os dados para envio
    filtersToSend.contracts =
      filtersToSend.contracts?.map((c) => c.toLowerCase()) || [];
    filtersToSend.shifts =
      filtersToSend.shifts?.map((s) => s.toLowerCase()) || [];
    filtersToSend.local = filtersToSend.local?.toLowerCase() || '';
    filtersToSend.searchTerm = filtersToSend.searchTerm?.toLowerCase() || '';
    filtersToSend.salaryRange = filtersToSend.salaryRange?.toLowerCase() || null;

    // Adicionado console.log para mostrar o objeto de filtros antes de fechar o dialog
    console.log('Filtros a serem enviados para a API:', filtersToSend);

    this.dialogRef.close(filtersToSend);
  }
}
