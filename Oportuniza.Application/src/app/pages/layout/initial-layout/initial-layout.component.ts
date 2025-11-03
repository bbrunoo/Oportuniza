import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, HostListener } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { UserService } from '../../../services/user.service';
import { UserProfile } from '../../../models/UserProfile.model';
import { ConfigsComponent } from '../../../extras/configs/configs.component';
import { Subject, take } from 'rxjs';
import { KeycloakOperationService } from '../../../services/keycloak.service';
import { LoadingComponent } from '../../../extras/loading/loading.component';

type UserRole = 'Owner' | 'Worker' | 'Administrator' | null;

@Component({
  selector: 'app-initial-layout',
  standalone: true,
  imports: [RouterOutlet, CommonModule, RouterLink],
  templateUrl: './initial-layout.component.html',
  styleUrl: './initial-layout.component.css',
})
export class InitialLayoutComponent implements OnInit, OnDestroy {
  isInitializing = true;
  private loadingDialogRef: MatDialogRef<LoadingComponent> | null = null;
  isCompany = false;
  userRole: UserRole = null;

  userProfile: UserProfile = {
    id: '',
    name: '',
    email: '',
    phone: '',
    imageUrl: '',
    local: '',
    interestArea: [],
    isCompany: false,
  };

  isMenuOpen = false;
  loginDisplay = false;
  showCompleteProfileicon = true;
  loginMethod: 'keycloak' | null = null;

  private readonly _destroying$ = new Subject<void>();

  constructor(
    private userService: UserService,
    private dialog: MatDialog,
    private keycloakService: KeycloakOperationService,
    private router: Router
  ) { }

  ngOnInit(): void {
    window.addEventListener('scroll', this.closeMenuOnScroll.bind(this), true);

    this.loadingDialogRef = this.dialog.open(LoadingComponent, {
      height: '300px',
      width: '450px',
      disableClose: true,
      panelClass: 'loading-dialog-panel',
      backdropClass: 'loading-dialog-backdrop',
    });

    this.handleSession();
  }

  ngOnDestroy(): void {
    window.removeEventListener('scroll', this.closeMenuOnScroll.bind(this), true);
    this._destroying$.next(undefined);
    this._destroying$.complete();
  }

  // 🔹 Sessão e login
  private async handleSession(): Promise<void> {
    const loggedIn = await this.keycloakService.isLoggedIn();
    if (loggedIn) {
      this.loginMethod = 'keycloak';
      this.afterLoginSetup();
    } else {
      this.closeLoadingAndRedirect();
    }
  }

  private afterLoginSetup(): void {
    this.loginDisplay = true;
    this.getLoggedUserProfile();
  }

  private closeLoadingAndRedirect(): void {
    this.loadingDialogRef?.close();
    this.isInitializing = false;
    sessionStorage.clear();
    this.loginMethod = null;
    this.router.navigate(['/login']);
  }

  // 🔹 Perfil e papel do usuário
  private getLoggedUserProfile(): void {
    this.userService
      .getOwnProfile()
      .pipe(take(1))
      .subscribe({
        next: (profile: UserProfile) => {
          this.userProfile = profile;
          this.loadUserCompanyRole();
        },
        error: (error: any) => {
          console.error('Erro ao carregar perfil do usuário:', error);
          this.closeLoadingAndRedirect();
        },
      });
  }

  private loadUserCompanyRole(): void {
    const companyId = this.keycloakService.getActiveCompanyId();

    if (!companyId) {
      this.isCompany = false;
      this.userRole = null;
      this.finishLoading();
      return;
    }

    this.keycloakService.verifyUserRole(companyId).pipe(take(1)).subscribe({
      next: (res) => {
        const normalizedRole = (res.role || '').trim().toLowerCase();

        if (res.hasRole && normalizedRole) {
          if (normalizedRole === 'owner') {
            this.userRole = 'Owner';
            this.isCompany = true;
          } else if (normalizedRole === 'worker' || normalizedRole === 'employee') {
            this.userRole = 'Worker';
            this.isCompany = true;
          } else if (normalizedRole === 'administrator' || normalizedRole === 'admin') {
            this.userRole = 'Administrator';
            this.isCompany = true;
          } else {
            console.warn(`🔸 Papel desconhecido recebido: ${normalizedRole}`);
            this.userRole = null;
            this.isCompany = false;
          }
        } else {
          this.userRole = null;
          this.isCompany = false;
        }

        console.log('🧭 Papel do usuário na empresa:', this.userRole);
        this.finishLoading();
      },
      error: (err) => {
        console.error('Erro ao verificar papel do usuário:', err);
        this.userRole = null;
        this.isCompany = false;
        this.finishLoading();
      },
    });
  }

  // 🔹 Verificação manual opcional
  verifyCompanyRole(): void {
    this.keycloakService.verifyUserRole().pipe(take(1)).subscribe({
      next: (res) => {
        const normalizedRole = (res.role || '').trim().toLowerCase();

        if (res.hasRole && normalizedRole) {
          if (normalizedRole === 'owner' || normalizedRole === 'companyowner') {
            this.userRole = 'Owner';
          } else if (normalizedRole === 'worker' || normalizedRole === 'employee') {
            this.userRole = 'Worker';
          } else if (normalizedRole === 'administrator' || normalizedRole === 'admin') {
            this.userRole = 'Administrator';
          } else {
            this.userRole = null;
          }

          this.isCompany = this.userRole !== null;
        } else {
          this.userRole = null;
          this.isCompany = false;
        }

        console.log('🧭 Papel do usuário na empresa (verificação manual):', this.userRole);
      },
      error: (err) => {
        console.error('Erro ao verificar papel do usuário:', err);
        this.userRole = null;
      },
    });
  }

  // 🔹 Funções de UI
  private finishLoading(): void {
    this.loadingDialogRef?.close();
    this.isInitializing = false;
  }

  toggleMenu(forceState?: boolean): void {
    if (window.innerWidth <= 1124) {
      this.isMenuOpen = forceState !== undefined ? forceState : !this.isMenuOpen;
    }
  }

  closeMenuOnScroll(): void {
    if (window.innerWidth <= 1024 && this.isMenuOpen) {
      setTimeout(() => (this.isMenuOpen = false), 50);
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    const sidebar = document.getElementById('sidebar');
    const trigger = document.querySelector('.menu-trigger-area');
    if (!sidebar || !trigger) return;

    const insideMenu = sidebar.contains(target);
    const clickedButton = trigger.contains(target);

    if (this.isMenuOpen && !insideMenu && !clickedButton) {
      this.isMenuOpen = false;
    }
  }

  openDialog(): void {
    if (this.isInitializing) return;

    this.isMenuOpen = false;

    this.dialog.open(ConfigsComponent, {
      minWidth: '230px',
      minHeight: '130px',
      position: { bottom: '80px', left: '130px' },
      panelClass: 'custom-dialog',
    });
  }
}
