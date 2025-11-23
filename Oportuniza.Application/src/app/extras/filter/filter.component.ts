import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { PublicationFilterDto } from '../../models/filter.model';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';

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
    latitude: null,
    longitude: null,
    radiusKm: 0,
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
      latitude: null,
      longitude: null,
      radiusKm: 0,
    };
  }

  toggleContract(contract: string) {
    this.filters.contracts = this.filters.contracts ?? [];
    const index = this.filters.contracts.indexOf(contract);
    if (index === -1) this.filters.contracts.push(contract);
    else this.filters.contracts.splice(index, 1);
  }

  toggleShift(shift: string) {
    this.filters.shifts = this.filters.shifts ?? [];
    const index = this.filters.shifts.indexOf(shift);
    if (index === -1) this.filters.shifts.push(shift);
    else this.filters.shifts.splice(index, 1);
  }

  toggleSalary(salary: string) {
    this.filters.salaryRange =
      this.filters.salaryRange === salary ? null : salary;
  }

  async onRadiusChange() {
    if (this.filters.radiusKm && this.filters.radiusKm > 0) {
      this.getUserLocation();
    } else {
      this.filters.latitude = null;
      this.filters.longitude = null;
    }
  }

  getUserLocation() {
    if (!navigator.geolocation) {
      Swal.fire(
        'Erro',
        'Geolocalização não é suportada neste navegador.',
        'error'
      );
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (pos) => {
        this.filters.latitude = pos.coords.latitude;
        this.filters.longitude = pos.coords.longitude;
      },
      (err) => {
        console.warn('Erro ao obter localização:', err);
        let msg = 'Não foi possível obter sua localização.';
        if (err.code === 1) msg = 'Permissão de localização negada.';
        if (err.code === 2) msg = 'Localização indisponível.';
        if (err.code === 3) msg = 'Tempo esgotado ao tentar obter localização.';
        Swal.fire('Erro', msg, 'error');
        this.filters.radiusKm = 0;
      },
      { enableHighAccuracy: true, timeout: 8000, maximumAge: 0 }
    );
  }

  clearFilters() {
    this.filters = {
      searchTerm: '',
      local: '',
      contracts: [],
      shifts: [],
      salaryRange: null,
      latitude: null,
      longitude: null,
      radiusKm: 0,
    };
  }

  apply() {
    const filtersToSend = { ...this.filters };
    filtersToSend.contracts = (filtersToSend.contracts ?? []).map((c) =>
      c.toLowerCase()
    );
    filtersToSend.shifts = (filtersToSend.shifts ?? []).map((s) =>
      s.toLowerCase()
    );
    filtersToSend.local = filtersToSend.local?.toLowerCase() || '';
    filtersToSend.searchTerm = filtersToSend.searchTerm?.toLowerCase() || '';
    filtersToSend.salaryRange =
      filtersToSend.salaryRange?.toLowerCase() || null;
    this.dialogRef.close(filtersToSend);
    this.clearFilters();
  }
}
