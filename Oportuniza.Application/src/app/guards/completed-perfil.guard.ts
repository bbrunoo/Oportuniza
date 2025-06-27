import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { catchError, map, of } from 'rxjs';

export const completedPerfilGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const userService = inject(UserService);

  return userService.getOwnProfile().pipe(
    map((profile: { isProfileCompleted: any; }) => {
      if (profile.isProfileCompleted) {
        return true;
      } else {
        router.navigate(['/primeira-etapa']);
        return false;
      }
    }),
    catchError((error) => {
      console.error('Erro ao buscar perfil:', error);
      router.navigate(['/primeira-etapa'])
      return of(false);
    })
  );
};
