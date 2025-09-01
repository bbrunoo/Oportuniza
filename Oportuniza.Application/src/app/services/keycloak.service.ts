import { isPlatformBrowser } from '@angular/common';
import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';

@Injectable({
  providedIn: 'root'
})
export class KeycloakOperationService {
  private isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) private platformId: Object,
    private http: HttpClient,
    private router: Router,
    private jwtHelper: JwtHelperService
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  async init(): Promise<void> {
    if (this.isBrowser) {
      console.log("keycloak initialized.");
    }
    return Promise.resolve();
  }

  loginWithCredentials(username: string, password: string): Observable<any> {
    const body = new HttpParams()
      .set('grant_type', 'password')
      .set('client_id', environment.keycloak.clientId)
      .set('client_secret', environment.keycloak.secret)
      .set('username', username)
      .set('password', password);

    const tokenUrl = `${environment.keycloak.url}/realms/${environment.keycloak.realm}/protocol/openid-connect/token`;

    return this.http.post(tokenUrl, body, {
      headers: new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' })
    }).pipe(
      tap(tokens => {
        this.saveTokens(tokens);
      }),
      catchError(error => {
        console.error('KeycloakOperationService: Erro ao autenticar com credenciais:', error);
        return throwError(() => new Error('Falha na autenticação.'));
      })
    );
  }

  saveTokens(tokens: any): void {
    if (this.isBrowser && tokens.access_token) {
      localStorage.setItem('access_token', tokens.access_token);
      localStorage.setItem('refresh_token', tokens.refresh_token);
      localStorage.setItem('id_token', tokens.id_token);
      localStorage.setItem('loginWithKeycloak', 'true');
    }
  }

  getAdminToken(): Observable<any> {
    const body = new HttpParams()
      .set('grant_type', 'password')
      .set('client_id', 'admin-cli')
      .set('username', 'admin')
      .set('password', 'admin');

    const adminTokenUrl = `${environment.keycloak.url}/realms/master/protocol/openid-connect/token`;

    return this.http.post(adminTokenUrl, body, {
      headers: new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' })
    });
  }

  getUserIdFromToken(): string | undefined {
    if (!this.isBrowser) {
      return undefined;
    }
    const token = localStorage.getItem('access_token');
    if (token) {
      try {
        const decodedToken = this.jwtHelper.decodeToken(token);
        return decodedToken.sub;
      } catch (e) {
        console.error('Erro ao decodificar o token para obter o ID do usuário:', e);
        return undefined;
      }
    }
    return undefined;
  }

  registerUser(user: { email: string; password: string }): Observable<any> {
    return this.http.post('http://localhost:5000/api/v1/Auth/register', user);
  }

  async isLoggedIn(): Promise<boolean> {
    if (!this.isBrowser) {
      return false;
    }
    const token = localStorage.getItem('access_token');
    return !!token && !this.isTokenExpired(token);
  }

  isTokenExpired(token: string | undefined | null): boolean {
    if (!token) {
      return true;
    }
    try {
      const decodedToken = JSON.parse(atob(token.split('.')[1]));
      const expirationTime = decodedToken.exp * 1000;

      return Date.now() >= expirationTime;
    } catch (e) {
      console.error('KeycloakOperationService: Erro ao decodificar ou verificar token JWT:', e);
      return true;
    }
  }

  refreshToken(): Observable<any> {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!this.isBrowser || !refreshToken) {
      return throwError(() => new Error('Refresh token não encontrado.'));
    }

    const body = new HttpParams()
      .set('grant_type', 'refresh_token')
      .set('client_id', environment.keycloak.clientId)
      .set('client_secret', environment.keycloak.secret)
      .set('refresh_token', refreshToken);

    const tokenUrl = `${environment.keycloak.url}/realms/${environment.keycloak.realm}/protocol/openid-connect/token`;

    return this.http.post(tokenUrl, body, {
      headers: new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' })
    }).pipe(
      tap(tokens => {
        this.saveTokens(tokens);
      }),
      catchError(error => {
        console.error('KeycloakOperationService: Erro ao renovar o token:', error);
        this.logout();
        return throwError(() => new Error('Falha na renovação do token.'));
      })
    );
  }

  isTokenAboutToExpire(token: string | undefined | null, buffer: number = 0): boolean {
    if (!token) {
      return true;
    }
    try {
      const decodedToken = JSON.parse(atob(token.split('.')[1]));
      const expirationTimeInSeconds = decodedToken.exp;
      const nowInSeconds = Date.now() / 1000;
      return nowInSeconds >= (expirationTimeInSeconds - buffer);
    } catch (e) {
      console.error('Erro ao decodificar ou verificar token JWT:', e);
      return true;
    }
  }

  async logout(): Promise<void> {
    if (this.isBrowser) {
      console.log('KeycloakOperationService: Realizando logout (limpando localStorage).');
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      localStorage.removeItem('id_token');
      localStorage.removeItem('loginWithKeycloak');
      this.router.navigate(['/']);
    }
    return Promise.resolve();
  }

  async getToken(): Promise<string | undefined> {
    if (!this.isBrowser) {
      return undefined;
    }

    const token = localStorage.getItem('access_token') ?? undefined;
    const refreshToken = localStorage.getItem('refresh_token') ?? undefined;

    if (!token || this.isTokenAboutToExpire(token, 60)) {
      if (refreshToken && !this.isTokenExpired(refreshToken)) {
        try {
          console.log('Renovando token: o token de acesso atual está expirado ou prestes a expirar.');
          const newTokens = await this.refreshToken().toPromise();
          return newTokens.access_token;
        } catch (e) {
          console.error('Falha ao renovar o token proativamente, fazendo logout.', e);
          this.logout();
          return undefined;
        }
      } else {
        console.warn('Refresh token ausente ou expirado, fazendo logout.');
        this.logout();
        return undefined;
      }
    }

    return token;
  }
}
