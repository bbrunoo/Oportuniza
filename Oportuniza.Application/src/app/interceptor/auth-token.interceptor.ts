import { KeycloakOperationService } from './../services/keycloak.service';
import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http';
import { BehaviorSubject, Observable, from, throwError } from 'rxjs';
import { switchMap, catchError, filter, take } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { isPlatformBrowser } from '@angular/common';

@Injectable()
export class AuthTokenInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(
    private keycloakService: KeycloakOperationService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const unprotectedEndpoints = ['Auth/register', 'Auth/login', 'Verification/send-verification'];
    const isUnprotected = unprotectedEndpoints.some(endpoint => request.url.endsWith(endpoint));

    if (!request.url.startsWith(environment.apiConfig.uri) || isUnprotected) {
      return next.handle(request);
    }

    let contextToken: string | null = null;
    if (isPlatformBrowser(this.platformId)) {
      contextToken = localStorage.getItem('context_access_token');
    }

    if (contextToken) {
      return next.handle(this.addToken(request, contextToken)).pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401) {
            console.warn('[AuthTokenInterceptor] Token de contexto expirado ou inválido.');
            if (!this.keycloakService.isLogoutInProgress() && isPlatformBrowser(this.platformId)) {
              localStorage.removeItem('context_access_token');
              this.keycloakService.logout();
            }
          }
          return throwError(() => error);
        })
      );
    }

    return from(this.keycloakService.selectToken()).pipe(
      switchMap((token) => {
        if (!token) {
          console.warn('[AuthTokenInterceptor] Nenhum token ativo encontrado. Fazendo logout.');
          if (!this.keycloakService.isLogoutInProgress()) {
            this.keycloakService.logout();
          }
          return throwError(() => new HttpErrorResponse({
            status: 401,
            statusText: 'Unauthorized',
            url: request.url
          }));
        }

        if (this.isRefreshing) {
          return this.refreshTokenSubject.pipe(
            filter(t => t != null),
            take(1),
            switchMap(newToken => next.handle(this.addToken(request, newToken!)))
          );
        }

        return next.handle(this.addToken(request, token)).pipe(
          catchError((error: HttpErrorResponse) => {
            if (error.status === 401) {
              console.warn('[AuthTokenInterceptor] Token expirado. Tentando renovar...');
              return this.handle401Error(request, next);
            }
            return throwError(() => error);
          })
        );
      })
    );
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.keycloakService.refreshToken().pipe(
        switchMap((tokens: any) => {
          console.info('[AuthTokenInterceptor] Token renovado com sucesso.');
          this.isRefreshing = false;
          this.refreshTokenSubject.next(tokens.access_token);
          return next.handle(this.addToken(request, tokens.access_token));
        }),
        catchError(err => {
          console.error('[AuthTokenInterceptor] Falha ao renovar o token.', err);
          this.isRefreshing = false;
          if (!this.keycloakService.isLogoutInProgress()) {
            this.keycloakService.logout();
          }
          return throwError(() => err);
        })
      );
    }

    return this.refreshTokenSubject.pipe(
      filter(token => token != null),
      take(1),
      switchMap(token => next.handle(this.addToken(request, token!)))
    );
  }

  private addToken(request: HttpRequest<any>, token: string): HttpRequest<any> {
    return request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }
}
