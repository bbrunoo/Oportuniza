import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

const allowedRoutes = ['/primeira-etapa', '/segunda-etapa', '/terceira-etapa'];

export const authRedirectGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    const currentUrl = state.url;

    const isAllowedRoute = allowedRoutes.some(route => currentUrl.startsWith(route));

    if (!isAllowedRoute) {
      router.navigate(['/home']);
      return false;
    }
  }

  return true;
};
