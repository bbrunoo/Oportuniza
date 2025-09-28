import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CompanyEmployeeDto } from '../../../models/company-get.model';
import { CompanyEmployeeService } from '../../../services/company-employee.service';
import { CommonModule, NgFor, NgIf } from '@angular/common';

@Component({
  selector: 'app-funcionarios',
  imports: [CommonModule, NgFor, NgIf],
  templateUrl: './funcionarios.component.html',
  styleUrl: './funcionarios.component.css'
})
export class FuncionariosComponent {
  empresaId: string | null = null;
  companyId: string | null = null;
  employees: CompanyEmployeeDto[] = [];
  isLoading: boolean = true;
  errorMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private employeeService: CompanyEmployeeService
  ) { }

  ngOnInit(): void {
    this.companyId = this.route.parent?.snapshot.paramMap.get('id') ?? null;

    if (this.companyId) {
      this.loadEmployees(this.companyId);
    } else {
      this.isLoading = false;
      this.errorMessage = 'ID da empresa não encontrado na URL.';
    }
  }

  openSettings(employeeId: string): void {
    console.log('Abrir configurações para o funcionário ID:', employeeId);
  }

  loadEmployees(id: string): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.employeeService.getOrderedEmployees(id).subscribe({
      next: (data) => {
        this.employees = data;
        this.isLoading = false;
        console.log('Lista de funcionários ordenada:', this.employees);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Falha ao carregar a lista de funcionários.';
        console.error('Erro ao buscar funcionários:', err);
      }
    });
  }
}
