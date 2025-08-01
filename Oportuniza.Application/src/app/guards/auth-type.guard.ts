import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { MsalService } from '@azure/msal-angular';
import { KeycloakOperationService } from '../services/keycloak.service'; // Ajuste o caminho

@Injectable({
  providedIn: 'root'
})
export class AuthTypeGuard implements CanActivate {
  constructor(
    private msalService: MsalService,
    private keycloakService: KeycloakOperationService,
    private router: Router
  ) {}

  async canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Promise<boolean | UrlTree> {
    const loggedWithKeycloakFlag = sessionStorage.getItem('loginWithKeycloak') === 'true';
    const loggedWithMicrosoftFlag = sessionStorage.getItem('loginWithMicrosoft') === 'true';

    let isAuthenticated = false;

    if (loggedWithKeycloakFlag) {
      const accessToken = sessionStorage.getItem('access_token');
      if (accessToken && !this.keycloakService.isTokenExpired(accessToken)) {
        isAuthenticated = true;
      } else {
        sessionStorage.removeItem('loginWithKeycloak');
        sessionStorage.removeItem('access_token');
      }
    }

    if (!isAuthenticated && loggedWithMicrosoftFlag) {
      const accounts = this.msalService.instance.getAllAccounts();
      if (accounts.length > 0) {
        this.msalService.instance.setActiveAccount(accounts[0]);
        isAuthenticated = true;
      } else {
        sessionStorage.removeItem('loginWithMicrosoft');
      }
    }

    if (!isAuthenticated) {
        const msalAccounts = this.msalService.instance.getAllAccounts();
        if (msalAccounts.length > 0) {
            this.msalService.instance.setActiveAccount(msalAccounts[0]);
            sessionStorage.setItem('loginWithMicrosoft', 'true');
            isAuthenticated = true;
        } else {
            const keycloakLoggedIn = await this.keycloakService.isLoggedIn();
            if (keycloakLoggedIn) {
                sessionStorage.setItem('loginWithKeycloak', 'true');
                isAuthenticated = true;
            }
        }
    }


    if (isAuthenticated) {
      return true;
    } else {
      console.warn('AuthTypeGuard: Nenhuma sessão ativa detectada. Redirecionando para /login.');
      return this.router.createUrlTree(['/login']);
    }
  }
}
