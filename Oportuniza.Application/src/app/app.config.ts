import {
  APP_INITIALIZER,
  ApplicationConfig,
  importProvidersFrom,
  inject,
  provideZoneChangeDetection
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, provideHttpClient, withFetch, withInterceptorsFromDi } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { provideEnvironmentNgxMask, provideNgxMask } from 'ngx-mask';

import { AuthTokenInterceptor } from './interceptor/auth-token.interceptor';
import { JWT_OPTIONS, JwtHelperService } from "@auth0/angular-jwt";
import { KeycloakOperationService } from './services/keycloak.service';

export function kcFactory(kcService: KeycloakOperationService) {
  return () => kcService.init();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withInterceptorsFromDi(), withFetch()),
    provideToastr(),
    provideEnvironmentNgxMask(),
    importProvidersFrom(TranslateModule.forRoot()),
    provideNgxMask(),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthTokenInterceptor,
      multi: true,
    },
    { provide: JWT_OPTIONS, useValue: JWT_OPTIONS },
    JwtHelperService,
    {
      provide: APP_INITIALIZER,
      useFactory: kcFactory,
      multi: true,
      deps: [KeycloakOperationService],
    },
  ]
};
