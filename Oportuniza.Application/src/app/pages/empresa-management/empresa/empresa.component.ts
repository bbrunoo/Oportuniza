import {
  Component,
  OnDestroy,
  OnInit,
  HostListener,
} from '@angular/core';
import {
  ActivatedRoute,
  Router,
  RouterLink,
  RouterModule,
  RouterOutlet,
  NavigationEnd,
} from '@angular/router';
import { CompanyService } from '../../../services/company.service';
import { CompanyDto } from '../../../models/company-get.model';
import { NgIf } from '@angular/common';
import { filter, Subject, Subscription, takeUntil } from 'rxjs';
import Swal from 'sweetalert2';
import { KeycloakOperationService } from '../../../services/keycloak.service';

@Component({
  selector: 'app-empresa',
  imports: [RouterOutlet, RouterLink, NgIf, RouterModule],
  templateUrl: './empresa.component.html',
  styleUrl: './empresa.component.css',
})
export class EmpresaComponent implements OnInit, OnDestroy {
  company!: CompanyDto;
  isLoading = true;
  companyId: string | null = null;
  errorMessage: string | null = null;
  userRoleInCompany: string | null = null;
  isUserAdmin = false;
  isMenuOpen = false;

  private subscription: Subscription = new Subscription();
  private destroy$ = new Subject<void>();

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private companyService: CompanyService,
    private keycloakService: KeycloakOperationService
  ) {}

  ngOnInit(): void {
    this.subscription.add(
      this.route.paramMap.subscribe((params) => {
        this.companyId = params.get('id');
        if (this.companyId) {
          this.loadUserRoleAndCompany();
        } else {
          this.isLoading = false;
          this.errorMessage = 'ID da empresa não encontrado na URL.';
        }
      })
    );

    this.subscription.add(
      this.router.events
        .pipe(filter((event) => event instanceof NavigationEnd))
        .subscribe(() => {
          if (this.companyId) {
            this.loadUserRoleAndCompany();
          }
        })
    );
  }

  toggleMenu(forceState?: boolean): void {
    if (window.innerWidth <= 1124) {
      this.isMenuOpen =
        forceState !== undefined ? forceState : !this.isMenuOpen;
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    const sidebar = document.querySelector('.sidebar');
    const trigger = document.querySelector('.menu-trigger-area');

    if (!sidebar || !trigger) return;

    const insideMenu = sidebar.contains(target);
    const clickedButton = trigger.contains(target);

    if (this.isMenuOpen && !insideMenu && !clickedButton) {
      this.isMenuOpen = false;
    }
  }

  private loadUserRoleAndCompany(): void {
    this.isLoading = true;
    this.errorMessage = null;

    if (!this.companyId) {
      this.isLoading = false;
      this.errorMessage = 'ID da empresa não encontrado.';
      return;
    }

    this.keycloakService
      .verifyUserRole(this.companyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.hasRole && res.role) {
            this.userRoleInCompany = res.role;
            this.isUserAdmin = res.role === 'Owner';
          } else {
            this.userRoleInCompany = null;
            this.isUserAdmin = false;
          }
          this.loadCompanyInformation();
        },
        error: (err) => {
          console.error('[EmpresaComponent] Erro ao verificar role:', err);
          this.errorMessage = 'Erro ao verificar o papel do usuário.';
          this.isLoading = false;
        },
      });
  }

  private loadCompanyInformation(): void {
    if (!this.companyId) return;

    this.companyService
      .getCompanyById(this.companyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.company = data;
          this.isLoading = false;
        },
        error: (err) => {
          this.isLoading = false;
          this.errorMessage = 'Erro ao carregar os dados da empresa.';
          console.error('Erro ao buscar a empresa:', err);
        },
      });
  }

  desativarEmpresa(): void {
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
      reverseButtons: true,
    }).then((result) => {
      if (result.isConfirmed && this.company?.id) {
        this.companyService
          .disableCompany(this.company.id)
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
                'Houve um erro ao desativar a empresa.',
                'error'
              );
              console.error('Erro ao desativar a empresa:', err);
            },
          });
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.destroy$.next();
    this.destroy$.complete();
  }
}
