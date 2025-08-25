import { KeycloakOperationService } from './../services/keycloak.service';
import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, from, of, throwError } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';
import { MsalService } from '@azure/msal-angular';
import { InteractionType } from '@azure/msal-browser';
import { environment } from '../../environments/environment';

@Injectable()
export class AuthTokenInterceptor implements HttpInterceptor {
  constructor(
    private msalService: MsalService,
    private keycloakService: KeycloakOperationService
  ) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const isProtectedApi = request.url.startsWith(environment.apiConfig.uri);

    if (!isProtectedApi) {
      console.log(`[AuthTokenInterceptor] URL não é para uma API protegida. Pulando: ${request.url}`);
      return next.handle(request);
    }

    console.log(`[AuthTokenInterceptor] URL protegida detectada: ${request.url}`);

    const loggedWithKeycloak = sessionStorage.getItem('loginWithKeycloak') === 'true';
    const loggedWithMicrosoft = sessionStorage.getItem('loginWithMicrosoft') === 'true';

    if (loggedWithKeycloak) {
      return from(this.keycloakService.getToken()).pipe(
        switchMap(keycloakToken => {
          if (keycloakToken) {
            console.log('AuthTokenInterceptor: Anexando token Keycloak.');
            return next.handle(request.clone({
              setHeaders: {
                Authorization: `Bearer ${keycloakToken}`
              }
            }));
          } else {
            // Se o token for nulo, jogue um erro para parar a requisição
            console.warn('AuthTokenInterceptor: Flag Keycloak TRUE, mas nenhum token disponível.');
            return throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Unauthorized', url: request.url, error: 'No Keycloak token available' }));
          }
        }),
        catchError(error => {
          console.error('AuthTokenInterceptor: Erro ao obter token Keycloak:', error);
          // Jogue o erro para que a requisição pare
          return throwError(() => error);
        })
      );
    } else if (loggedWithMicrosoft) {
      const account = this.msalService.instance.getActiveAccount();
      if (account) {
        const msalRequest = {
          scopes: this.getMsalScopesForRequest(request.url),
          account: account
        };
        return from(this.msalService.acquireTokenSilent(msalRequest)).pipe(
          switchMap(response => {
            if (response.accessToken) {
              console.log('AuthTokenInterceptor: Anexando token MSAL.');
              return next.handle(request.clone({
                setHeaders: {
                  Authorization: `Bearer ${response.accessToken}`
                }
              }));
            } else {
              // Se o token for nulo, jogue um erro para parar a requisição
              console.warn('AuthTokenInterceptor: Flag MSAL TRUE, mas nenhum token disponível.');
              return throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Unauthorized', url: request.url, error: 'No MSAL token available' }));
            }
          }),
          catchError(error => {
            console.error('AuthTokenInterceptor: Erro na aquisição silenciosa de token MSAL:', error);
            // Jogue o erro para que a requisição pare
            return throwError(() => error);
          })
        );
      } else {
        // Se não houver conta ativa, jogue um erro para parar a requisição
        console.warn('AuthTokenInterceptor: Flag MSAL TRUE, mas nenhuma conta MSAL ativa.');
        return throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Unauthorized', url: request.url, error: 'No MSAL account active' }));
      }
    } else {
      console.log('AuthTokenInterceptor: Nenhuma flag de login. Prosseguindo sem token.');
      return next.handle(request);
    }
  }
  private getMsalScopesForRequest(url: string): string[] {
    if (url.startsWith(environment.apiConfig.uri)) {
      return environment.apiConfig.scopes;
    }
    return ['user.read'];
  }
}
