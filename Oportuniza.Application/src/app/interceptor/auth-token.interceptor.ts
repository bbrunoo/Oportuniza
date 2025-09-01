import { KeycloakOperationService } from './../services/keycloak.service';
import { Injectable } from '@angular/core';
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

@Injectable()
export class AuthTokenInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(private keycloakService: KeycloakOperationService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const unprotectedEndpoints = ['Auth/register', 'Auth/login', 'Verification/send-verification'];
    const isUnprotectedEndpoint = unprotectedEndpoints.some(endpoint => request.url.endsWith(endpoint));

    if (!request.url.startsWith(environment.apiConfig.uri) || isUnprotectedEndpoint) {
      console.log(`[AuthTokenInterceptor] URL não é protegida. Pulando: ${request.url}`);
      return next.handle(request);
    }

    return from(this.keycloakService.getToken()).pipe(
      switchMap((token) => {
        if (this.isRefreshing) {
          return this.refreshTokenSubject.pipe(
            filter(token => token != null),
            take(1),
            switchMap(newToken => {
              return next.handle(this.addToken(request, newToken));
            })
          );
        }

        if (token) {
          return next.handle(this.addToken(request, token)).pipe(
            catchError((error: HttpErrorResponse) => {
              if (error.status === 401) {
                return this.handle401Error(request, next);
              }
              return throwError(() => error);
            })
          );
        } else {
          console.warn('AuthTokenInterceptor: Nenhum token disponível. Redirecionando para login.');
          this.keycloakService.logout();
          return throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Unauthorized', url: request.url }));
        }
      }),
      catchError((error) => {
        console.error('AuthTokenInterceptor: Erro ao obter token ou na requisição:', error);
        return throwError(() => error);
      })
    );
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler) {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null); 

      return this.keycloakService.refreshToken().pipe(
        switchMap((tokens: any) => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(tokens.access_token);
          return next.handle(this.addToken(request, tokens.access_token));
        }),
        catchError((err) => {
          this.isRefreshing = false;
          this.keycloakService.logout();
          return throwError(() => err);
        })
      );
    }
    return this.refreshTokenSubject.pipe(
      filter(token => token != null),
      take(1),
      switchMap(token => {
        return next.handle(this.addToken(request, token));
      })
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
