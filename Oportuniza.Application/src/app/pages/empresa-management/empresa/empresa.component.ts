import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink, RouterModule, RouterOutlet } from '@angular/router';
import { CompanyService } from '../../../services/company.service';
import { CompanyDto } from '../../../models/company-get.model';
import { NgIf } from '@angular/common';
import { filter, Observable, Subject, Subscription, takeUntil } from 'rxjs';
import { NavigationEnd } from '@angular/router';
import Swal from 'sweetalert2';
import { KeycloakOperationService } from '../../../services/keycloak.service';

@Component({
  selector: 'app-empresa',
  imports: [RouterOutlet, RouterLink, NgIf, RouterModule],
  templateUrl: './empresa.component.html',
  styleUrl: './empresa.component.css'
})
export class EmpresaComponent implements OnInit, OnDestroy {
  company!: CompanyDto;
  isLoading: boolean = true;
  companyId: string | null = null;
  errorMessage: string | null = null;
  private subscription: Subscription = new Subscription();
  private destroy$ = new Subject<void>();
  userRoleInCompany: string | null = null;
  isUserAdmin: boolean = false;

  ngOnInit(): void {
    this.subscription.add(
      this.route.paramMap.subscribe(params => {
        this.companyId = params.get('id');
        if (this.companyId) {
          this.loadCompanyInformation();
        } else {
          this.isLoading = false;
          this.errorMessage = 'ID da empresa não encontrado na URL.';
        }
      })
    );

    this.subscription.add(
      this.router.events
        .pipe(filter(event => event instanceof NavigationEnd))
        .subscribe(() => {
          if (this.companyId) {
            this.loadCompanyInformation();
          }
        })
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.destroy$.next();
    this.destroy$.complete();
  }

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private companyService: CompanyService,
    private keycloakService: KeycloakOperationService
  ) { }

  loadCompanyInformation(): void {
    if (!this.companyId) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.companyService.getCompanyById(this.companyId).subscribe({
      next: (data) => {
        this.company = data;
        this.isLoading = false;
        console.log('Dados da empresa carregados:', this.company);

        this.detectUserRole();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Erro ao carregar os dados da empresa.';
        console.error('Erro ao buscar a empresa:', err);
      }
    });
  }

  detectUserRole(): void {
    const activeContext = this.keycloakService.getActiveContext();

    this.isUserAdmin = false;
    this.userRoleInCompany = null;

    if (!activeContext || !this.company) return;

    if (activeContext.Type === 'User') {
      this.userRoleInCompany = 'User';
      this.isUserAdmin = true;
      return;
    }

    if (activeContext.Type === 'Company' && activeContext.Id === this.companyId) {
      this.userRoleInCompany = activeContext.Role ?? 'Worker';

      if (activeContext.Role === 'Owner' || this.company.ownerId === activeContext.OwnerId) {
        this.isUserAdmin = true;
        this.userRoleInCompany = 'Owner';
      } else {
        this.isUserAdmin = false;
        this.userRoleInCompany = 'Worker';
      }
      return;
    }
  }

  desativarEmpresa() {
    Swal.fire({
      text: 'Tem certeza que deseja excluir a empresa?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      width: '350px',
      color: '#252525',
      confirmButtonText: 'Sim',
      cancelButtonText: 'Cancelar',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this.companyService.disableCompany(this.company.id)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: () => {
              Swal.fire(
                'Desativada!',
                'Sua empresa foi desativada com sucesso.',
                'success'
              );
            },
            error: (err) => {
              Swal.fire(
                'Erro!',
                'Houve um erro ao desativar a postagem.',
                'error'
              );
              console.error('Erro ao desativar a postagem:', err);
            }
          });
      }
    });
  }
}
