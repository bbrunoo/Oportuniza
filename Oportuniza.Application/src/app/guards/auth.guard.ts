import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { UserService } from '../services/user.service';
import { catchError, map, of } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const userService = inject(UserService);

  return userService.getOwnProfile().pipe(
    map(profile => {
      if (profile.isProfileCompleted) {
        router.navigate(['/inicio']);
        return false;
      } else {
        return true;
      }
    }),
    catchError((error) => {
      console.error('Erro ao buscar perfil:', error);
      router.navigate(['/inicio']);
      return of(false);
    })
  );
};
