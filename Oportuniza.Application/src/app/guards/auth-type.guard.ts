import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { KeycloakOperationService } from '../services/keycloak.service';
import { firstValueFrom } from 'rxjs';

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
        isAuthenticated = await this.keycloakService.isLoggedIn();
    }

    if (!isAuthenticated && loggedWithMicrosoftFlag) {
        isAuthenticated = this.msalService.instance.getAllAccounts().length > 0;
    }

    if (isAuthenticated) {
      return true;
    } else {
      console.warn('AuthTypeGuard: Nenhuma sessão ativa detectada. Redirecionando para /login.');
      return this.router.createUrlTree(['/login']);
    }
  }
}
