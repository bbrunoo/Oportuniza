import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router, UrlTree } from '@angular/router';
import { KeycloakOperationService } from '../services/keycloak.service';

@Injectable({
  providedIn: 'root'
})
export class AuthTypeGuard implements CanActivate {
  constructor(
    private keycloakService: KeycloakOperationService,
    private router: Router
  ) { }

  async canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Promise<boolean | UrlTree> {
    const isAuthenticated = await this.keycloakService.isLoggedIn();

    if (isAuthenticated) {
      return true;
    } else {
      return this.router.createUrlTree(['/login']);
    }
  }
}
