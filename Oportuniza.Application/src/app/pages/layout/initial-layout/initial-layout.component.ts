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

  userProfile: UserProfile = {
    id: '',
    name: '',
    email: '',
    phone: '',
    imageUrl: '',
    isProfileCompleted: false,
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
  ) {}

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

  /** Alterna o menu com clique */
  toggleMenu(forceState?: boolean): void {
    if (window.innerWidth <= 1124) {
      this.isMenuOpen = forceState !== undefined ? forceState : !this.isMenuOpen;
    }
  }

  /** Fecha menu se rolar a página */
  closeMenuOnScroll(): void {
    if (window.innerWidth <= 1024 && this.isMenuOpen) {
      setTimeout(() => (this.isMenuOpen = false), 50);
    }
  }

  /** Fecha o menu ao clicar fora */
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

  private async handleSession(): Promise<void> {
    const loggedIn = await this.keycloakService.isLoggedIn();
    if (loggedIn) {
      this.loginMethod = 'keycloak';
      this.afterLoginSetup();
    } else {
      this.closeLoadingAndRedirect();
    }
  }

  private closeLoadingAndRedirect(): void {
    this.loadingDialogRef?.close();
    this.isInitializing = false;
    sessionStorage.clear();
    this.loginMethod = null;
    this.router.navigate(['/login']);
  }

  afterLoginSetup() {
    this.loginDisplay = true;
    this.getLoggedUserProfile();
  }

  getLoggedUserProfile() {
    this.userService
      .getOwnProfile()
      .pipe(take(1))
      .subscribe({
        next: (profile: UserProfile) => {
          this.userProfile = profile;
          if (!profile.isCompany) {
            this.showCompleteProfileicon = !profile.isProfileCompleted;
          }
          this.loadingDialogRef?.close();
          this.isInitializing = false;
        },
        error: (error: any) => {
          console.error('Erro ao carregar perfil do usuário:', error);
          this.closeLoadingAndRedirect();
        },
      });
  }

  ngOnDestroy(): void {
    window.removeEventListener('scroll', this.closeMenuOnScroll.bind(this), true);
    this._destroying$.next(undefined);
    this._destroying$.complete();
  }

  openDialog() {
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
