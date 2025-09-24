import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CompanyListDto } from '../../../models/company-list-dto-model';
import { CompanyService } from '../../../services/company.service';

@Component({
  selector: 'app-minhas-empresas',
  imports: [CommonModule, RouterLink],
  templateUrl: './minhas-empresas.component.html',
  styleUrl: './minhas-empresas.component.css'
})
export class MinhasEmpresasComponent {
  companies: CompanyListDto[] = [];
  isLoading = true;
  pageNumber = 1;
  pageSize = 8;
  totalPages = 0;

  constructor(private companyService: CompanyService) { }

  ngOnInit(): void {
    this.getCompanies();
  }

  nextPage() {
    if (this.pageNumber < this.totalPages) {
      this.pageNumber++;
      this.getCompanies();
    }
  }

  prevPage() {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.getCompanies();
    }
  }

  getCompanies() {
    this.isLoading = true;
    this.companyService.getUserCompaniesPaginated(this.pageNumber, this.pageSize).subscribe({
      next: (data) => {
        this.companies = data.items;
        this.totalPages = data.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao buscar empresas', error);
        this.isLoading = false;
      }
    });
  }
}
