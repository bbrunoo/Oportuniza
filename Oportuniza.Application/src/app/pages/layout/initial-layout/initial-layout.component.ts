import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { UserService } from '../../../services/user.service';
import { UserProfile } from '../../../models/UserProfile.model';
import { ConfigsComponent } from '../../../extras/configs/configs.component';
import { Subject, take, takeUntil } from 'rxjs';
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
  };

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
    this.loadingDialogRef = this.dialog.open(LoadingComponent, {
      height: '300px',
      width: '450px',
      disableClose: true,
      panelClass: 'loading-dialog-panel',
      backdropClass: 'loading-dialog-backdrop',
    });

    this.handleSession();
  }

  private async handleSession(): Promise<void> {
    const loggedIn = await this.keycloakService.isLoggedIn();
    if (loggedIn) {
      console.log('Sessão Keycloak válida. Carregando perfil do usuário...');
      this.loginMethod = 'keycloak';
      this.afterLoginSetup();
    } else {
      console.warn('Nenhuma sessão ativa detectada. Redirecionando para login.');
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
          this.showCompleteProfileicon = !profile.isProfileCompleted;
          console.log('Perfil do usuário carregado:', this.userProfile);
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
    this._destroying$.next(undefined);
    this._destroying$.complete();
  }

  openDialog() {
    if (this.isInitializing) {
      console.log('Aguardando a inicialização do perfil do usuário...');
      return;
    }

    const dialogRef = this.dialog.open(ConfigsComponent, {
      minWidth: '230px',
      minHeight: '130px',
      position: {
        bottom: '80px',
        left: '130px',
      },
      panelClass: 'custom-dialog',
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        console.log('fechou');
      }
    });
  }
}
