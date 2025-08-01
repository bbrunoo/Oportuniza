// import { authConfig, useAuth } from './../../../authConfig';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../../services/user.service';
import { UserProfile } from '../../../models/UserProfile.model';
import { ConfigsComponent } from '../../../extras/configs/configs.component';
import { AuthenticationResult, EventMessage, EventType, InteractionStatus } from '@azure/msal-browser';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { filter, Subject, take, takeUntil } from 'rxjs';
import { KeycloakOperationService } from '../../../services/keycloak.service';

@Component({
  selector: 'app-initial-layout',
  imports: [RouterOutlet, CommonModule, RouterLink],
  templateUrl: './initial-layout.component.html',
  styleUrl: './initial-layout.component.css'
})
export class InitialLayoutComponent implements OnInit, OnDestroy {
  userProfile: UserProfile = {
    id: '',
    name: '',
    email: '',
    phone: '',
    imageUrl: '',
    isProfileCompleted: false
  };

  loginDisplay = false;
  showCompleteProfileicon = true;
  loginMethod: 'keycloak' | 'msal' | null = null;

  private readonly _destroying$ = new Subject<void>();

  constructor(
    private userService: UserService,
    private dialog: MatDialog,
    private msalService: MsalService,
    private msalBroadcastService: MsalBroadcastService,
    private keycloakService: KeycloakOperationService,
    private router: Router
  ) { }

  ngOnInit(): void {
    const loggedWithKeycloakFlag = sessionStorage.getItem('loginWithKeycloak') === 'true';
    const loggedWithMicrosoftFlag = sessionStorage.getItem('loginWithMicrosoft') === 'true';

    this.msalBroadcastService.msalSubject$
      .pipe(
        filter((msg: EventMessage) =>
          msg.eventType === EventType.LOGIN_SUCCESS ||
          msg.eventType === EventType.ACQUIRE_TOKEN_SUCCESS
        ),
        takeUntil(this._destroying$)
      )
      .subscribe((result: EventMessage) => {
        const payload = result.payload as AuthenticationResult;
        this.msalService.instance.setActiveAccount(payload.account);
        sessionStorage.setItem('loginWithMicrosoft', 'true');
        this.loginMethod = 'msal';
        console.log('Evento MSAL LOGIN/ACQUIRE_TOKEN_SUCCESS detectado. Realizando setup.');
        this.afterLoginSetup();
      });

    if (loggedWithKeycloakFlag) {
      this.loginMethod = 'keycloak';
      this.validateKeycloakSession();
      this.afterLoginSetup();
    } else if (loggedWithMicrosoftFlag) {
      this.loginMethod = 'msal';
      this.validateMsalSession();
    } else {
      this.checkExistingSessions();

      if (!this.loginMethod) {
        console.warn('Nenhuma sessão ativa detectada. Redirecionando para /login.');
        this.router.navigate(['/login']);
        return;
      }

      if (this.loginMethod === 'keycloak' && !loggedWithKeycloakFlag) {
        this.afterLoginSetup();
      }
    }
  }

  async validateKeycloakSession(): Promise<void> {
    const accessToken = sessionStorage.getItem('access_token');
    if (accessToken && !this.keycloakService.isTokenExpired(accessToken)) {
      console.log('Sessão Keycloak válida.');
    } else {
      console.warn('Sessão Keycloak inválida ou expirada. Limpando flags e redirecionando.');
      sessionStorage.removeItem('loginWithKeycloak');
      sessionStorage.removeItem('access_token');
      this.loginMethod = null;
      this.router.navigate(['/login']);
    }
  }

  afterLoginSetup() {
    this.loginDisplay = true;
    this.getLoggedUserProfile();
  }

  getLoggedUserProfile() {
    this.userService.getOwnProfile().pipe(take(1)).subscribe({
      next: (profile: UserProfile) => {
        this.userProfile = profile;
        this.showCompleteProfileicon = !profile.isProfileCompleted;
        console.log("Perfil do usuário carregado:", this.userProfile);
      },
      error: (error: any) => {
        console.error('Erro ao carregar perfil do usuário:', error);
        console.warn('Erro ao obter perfil. Redirecionando para login.');
        this.router.navigate(['/login']);
        sessionStorage.removeItem('loginWithKeycloak');
        sessionStorage.removeItem('loginWithMicrosoft');
        sessionStorage.removeItem('access_token');
        this.loginMethod = null;
      }
    })
  }

  ngOnDestroy(): void {
    this._destroying$.next(undefined);
    this._destroying$.complete();
  }

  validateMsalSession(): void {
    const accounts = this.msalService.instance.getAllAccounts();
    if (accounts.length > 0) {
      this.msalService.instance.setActiveAccount(accounts[0]);
      console.log('Sessão MSAL ativa.');
    } else {
      console.warn('Nenhuma conta MSAL ativa. Limpando flags e esperando por LOGIN_SUCCESS.');
      sessionStorage.removeItem('loginWithMicrosoft');
      this.loginMethod = null;
    }

    this.msalBroadcastService.msalSubject$
      .pipe(filter((msg: EventMessage) => msg.eventType === EventType.LOGIN_SUCCESS || msg.eventType === EventType.ACQUIRE_TOKEN_SUCCESS))
      .subscribe((result: EventMessage) => {
        const payload = result.payload as AuthenticationResult;
        this.msalService.instance.setActiveAccount(payload.account);
        sessionStorage.setItem('loginWithMicrosoft', 'true');
        this.loginMethod = 'msal';
        this.afterLoginSetup();
      });

    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None)
      )
      .subscribe(() => {

        if (sessionStorage.getItem('loginWithMicrosoft') === 'true' &&
          this.msalService.instance.getAllAccounts().length === 0 &&
          this.loginMethod !== 'msal') {
          console.warn('MSAL finalizou interação sem conta ativa. Redirecionando para /login.');
          sessionStorage.removeItem('loginWithMicrosoft');
          this.router.navigate(['/login']);
        }
      });
  }

  async checkExistingSessions(): Promise<void> {
    const accounts = this.msalService.instance.getAllAccounts();
    if (accounts.length > 0) {
      this.msalService.instance.setActiveAccount(accounts[0]);
      sessionStorage.setItem('loginWithMicrosoft', 'true');
      this.loginMethod = 'msal';
      console.log('Sessão MSAL existente detectada.');
      return;
    }

    const keycloakLoggedIn = await this.keycloakService.isLoggedIn();
    const accessToken = sessionStorage.getItem('access_token');

    if (keycloakLoggedIn && accessToken && !this.keycloakService.isTokenExpired(accessToken)) {
      sessionStorage.setItem('loginWithKeycloak', 'true');
      this.loginMethod = 'keycloak';
      console.log('Sessão Keycloak existente detectada.');
      return;
    }
  }


  async checkKeycloakLogin(): Promise<boolean> {
    const loggedIn = await this.keycloakService.isLoggedIn();
    if (loggedIn) {
      this.loginMethod = 'keycloak';
      this.setLoginDisplay();
      this.getLoggedUserProfile();
      return true;
    }
    return false;
  }

  setLoginDisplay() {
    this.loginDisplay = this.msalService.instance.getAllAccounts().length > 0;
  }

  openDialog() {
    const dialogRef = this.dialog.open(ConfigsComponent, {
      minWidth: '230px',
      minHeight: '130px',
      position: {
        bottom: '80px',
        left: '130px'
      },
      panelClass: 'custom-dialog'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('fechou');
      }
    });
  }
}
