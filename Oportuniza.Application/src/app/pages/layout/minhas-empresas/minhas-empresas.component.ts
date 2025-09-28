import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CompanyListDto } from '../../../models/company-list-dto-model';
import { CompanyService } from '../../../services/company.service';
import { PostActionsComponent } from '../../../extras/post-actions/post-actions.component';
import { MatDialog } from '@angular/material/dialog';
import { CompanyActionsComponent } from '../../../extras/company-actions/company-actions.component';

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

  constructor(
    private companyService: CompanyService,
    private dialog: MatDialog,
    private router: Router) { }

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

  goToCompanyConfig(companyId: string | undefined): void {
    if (companyId) {
      this.router.navigate(['/empresa', companyId, 'informacoes']);
    } else {
      console.error('ID da empresa é nulo ou indefinido. Não é possível navegar.');
    }
  }

  openDialog(event: MouseEvent, company: CompanyListDto) {
    const rect = (event.target as HTMLElement).getBoundingClientRect();

    const dialogRef = this.dialog.open(CompanyActionsComponent, {
      minWidth: '130px',
      minHeight: '80px',
      position: {
        top: `${rect.top}px`,
        left: `${rect.left}px`,
      },
      data: { company: company },
      panelClass: 'custom-dialog',
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.companies = this.companies.filter((p) => p.id !== company.id);
      }
    });
  }
}
