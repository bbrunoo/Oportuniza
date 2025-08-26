import { KeycloakOperationService } from './../services/keycloak.service';
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, from, throwError } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';

@Injectable()
export class AuthTokenInterceptor implements HttpInterceptor {
  constructor(private keycloakService: KeycloakOperationService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Definir as URLs não protegidas como strings parciais para evitar problemas com barras finais
    const unprotectedEndpoints = [
      'Auth/register-keycloak',
      'Auth/login-keycloak' // Adicionar login também como não protegido
    ];

    // Checar se a URL da requisição termina com algum dos endpoints não protegidos
    const isUnprotectedEndpoint = unprotectedEndpoints.some(endpoint => request.url.endsWith(endpoint));

    // Se a URL não é uma API protegida ou se é um endpoint não protegido,
    // pule a lógica de anexo de token.
    if (!request.url.startsWith(environment.apiConfig.uri) || isUnprotectedEndpoint) {
      console.log(`[AuthTokenInterceptor] URL não é protegida. Pulando: ${request.url}`);
      return next.handle(request);
    }

    console.log(`[AuthTokenInterceptor] URL protegida detectada: ${request.url}`);

    return from(this.keycloakService.getToken()).pipe(
      switchMap((keycloakToken) => {
        if (keycloakToken) {
          console.log('AuthTokenInterceptor: Anexando token Keycloak.');
          return next.handle(
            request.clone({
              setHeaders: {
                Authorization: `Bearer ${keycloakToken}`,
              },
            })
          );
        } else {
          console.warn('AuthTokenInterceptor: Nenhum token Keycloak disponível. A requisição será bloqueada.');
          return throwError(() =>
            new HttpErrorResponse({
              status: 401,
              statusText: 'Unauthorized',
              url: request.url,
              error: 'No Keycloak token available',
            })
          );
        }
      }),
      catchError((error) => {
        console.error('AuthTokenInterceptor: Erro ao obter token Keycloak:', error);
        return throwError(() => error);
      })
    );
  }
}
