import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CompanyListDto } from '../../../models/company-list-dto-model';
import { CompanyService } from '../../../services/company.service';
import { PostActionsComponent } from '../../../extras/post-actions/post-actions.component';
import { MatDialog } from '@angular/material/dialog';
import { CompanyActionsComponent } from '../../../extras/company-actions/company-actions.component';
import { AuthService } from '../../../services/auth.service';
import { KeycloakOperationService } from '../../../services/keycloak.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-minhas-empresas',
  imports: [CommonModule],
  templateUrl: './minhas-empresas.component.html',
  styleUrl: './minhas-empresas.component.css'
})
export class MinhasEmpresasComponent {
  companies: CompanyListDto[] = [];
  isLoading = true;
  pageNumber = 1;
  pageSize = 8;
  totalPages = 0;

  isCompanyContext = false;
  activeCompanyId: string | null = null;

  constructor(
    private companyService: CompanyService,
    private dialog: MatDialog,
    private router: Router,
    private keycloakService: KeycloakOperationService
  ) { }

  ngOnInit(): void {
    this.detectContext();
    this.getCompanies();
  }

  detectContext(): void {
    const contextType = this.keycloakService.getActiveContextType();
    if (contextType === 'company') {
      this.isCompanyContext = true;
      this.activeCompanyId = this.keycloakService.getActiveCompanyId();
    }
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

  getVisiblePages(): number[] {
    const pages: number[] = [];
    const total = this.totalPages;
    const current = this.pageNumber;
    const delta = 2;

    const start = Math.max(1, current - delta);
    const end = Math.min(total, current + delta);

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  }

  goToPage(page: number): void {
    if (page !== this.pageNumber) {
      this.pageNumber = page;
      this.getCompanies();
    }
  }

  getCompanies() {
    this.isLoading = true;
    this.companyService.getUserCompaniesPaginated(this.pageNumber, this.pageSize).subscribe({
      next: (data) => {
        this.companies = data.items.map(company => ({
          ...company,
          IsDisabled: company.isActive !== 0,
          IsActiveStatus: company.isActive === 0
        })) as CompanyListDto[];

        if (this.isCompanyContext && this.activeCompanyId) {
          this.companies.sort((a, b) => {
            if (a.id === this.activeCompanyId) return -1;
            if (b.id === this.activeCompanyId) return 1;
            return 0;
          });
        }

        this.totalPages = data.totalPages;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao buscar empresas', error);
        this.isLoading = false;
      }
    });
  }

  async goToCompanyConfig(companyId: string | undefined): Promise<void> {
    if (!companyId) {
      console.error('ID da empresa é nulo ou indefinido.');
      return;
    }

    if (this.isCompanyContext && companyId !== this.activeCompanyId) {
      await Swal.fire({
        icon: 'warning',
        title: 'Ação não permitida',
        text: 'Você está no contexto da empresa atual e não pode editar outras empresas.',
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    this.router.navigate(['/empresa', companyId, 'informacoes']);
  }

  async openDialog(event: MouseEvent, company: CompanyListDto) {
    const rect = (event.target as HTMLElement).getBoundingClientRect();

    if (this.isCompanyContext && company.id !== this.activeCompanyId) {
      await Swal.fire({
        icon: 'info',
        title: 'Ação bloqueada',
        text: 'Você não pode editar ou excluir outras empresas enquanto estiver no contexto da empresa atual.',
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'Ok'
      });
      return;
    }

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

  async toggleCompanyStatus(company: CompanyListDto): Promise<void> {

    if (this.isCompanyContext && company.id !== this.activeCompanyId) {
      await Swal.fire({
        icon: 'warning',
        title: 'Ação não permitida',
        text: 'Você só pode gerenciar a empresa que está no seu contexto atual.',
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    const currentNumericStatus = company.isActive;
    const newStatus = currentNumericStatus === 0 ? 1 : 0;

    const actionText = newStatus === 0 ? 'Reativar' : 'Desativar';
    const statusText = newStatus === 0 ? 'ATIVA' : 'INATIVA';

    const result = await Swal.fire({
      title: `${actionText} Empresa?`,
      html: `Você está prestes a tornar a empresa <b>${company.name}</b> <b>${statusText}</b>.<br>Esta ação afeta o acesso de todos os funcionários. Confirma?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: newStatus === 0 ? '#28a745' : '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: `Sim, ${actionText}!`
    });

    if (result.isConfirmed) {
      this.isLoading = true;
      this.companyService.updateCompanyStatus(company.id, newStatus).subscribe({
        next: () => {
          Swal.fire('Sucesso!', `A empresa <b>${company.name}</b> foi ${actionText.toLowerCase()} com sucesso.`, 'success');
          this.getCompanies();
        },
        error: (err) => {
          console.error(`Erro ao tentar ${actionText.toLowerCase()} a empresa:`, err);
          this.isLoading = false;
          Swal.fire('Erro!', `Falha ao ${actionText.toLowerCase()} a empresa.`, 'error');
        }
      });
    }
  }

  async createCompany(): Promise<void> {
    if (this.isCompanyContext) {
      await Swal.fire({
        icon: 'warning',
        title: 'Criação não permitida',
        html: `
          <p>Você está acessando como <strong>empresa</strong>.</p>
          <p>Para criar uma nova empresa, troque para o acesso de <strong>usuário</strong>.</p>
        `,
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'Entendido'
      });
      return;
    }

    this.router.navigate(['/inicio/criar-empresa']);
  }
}
