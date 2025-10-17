import { isPlatformBrowser } from '@angular/common';
import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { jwtDecode } from 'jwt-decode';

export interface ActiveContext {
  Type: 'User' | 'Company';
  Id: string;
  Name?: string;
  Email?: string;
  Role?: string;
  ImageUrl?: string;
  OwnerId: string;
}

@Injectable({
  providedIn: 'root'
})


export class KeycloakOperationService {
  private isBrowser: boolean;
  private isLoggingOut: boolean = false;

  private currentTokenSubject = new BehaviorSubject<string | null>(this.getInitialToken());

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private http: HttpClient,
    private router: Router,
    private jwtHelper: JwtHelperService
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  public isLogoutInProgress(): boolean {
    return this.isLoggingOut;
  }

  private getInitialToken(): string | null {
    if (!this.isBrowser) return null;
    const activeType = localStorage.getItem('active_token');
    if (activeType === 'company') {
      return localStorage.getItem('company_token');
    } else if (activeType === 'user') {
      return localStorage.getItem('user_token');
    }
    return null;
  }

  getToken$(): Observable<string | null> {
    return this.currentTokenSubject.asObservable();
  }

  async selectToken(): Promise<string | null> {
    return this.currentTokenSubject.value;
  }

  async init(): Promise<void> {
    if (this.isBrowser) {
      console.log("keycloak initialized.");
    }
    return Promise.resolve();
  }

  loginWithCredentials(username: string, password: string): Observable<any> {
    const url = `${environment.apiUrl}/v1/Auth/login-keycloak`;
    return this.http.post(url, { email: username, password }).pipe(
      tap((res: any) => {
        this.saveTokens(res);
      }),
      catchError(error => {
        console.error('KeycloakOperationService: Erro ao autenticar via backend:', error);
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

      this.currentTokenSubject.next(tokens.access_token);

      localStorage.setItem('user_token', tokens.access_token);
      localStorage.setItem('active_token', 'user');
      localStorage.setItem('context_access_token', tokens.access_token);
    }
  }

  public getActiveContextType(): 'user' | 'company' | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('active_token') as 'user' | 'company' | null;
  }

  public getActiveCompanyId(): string | null {
    try {
      const token = this.getToken();
      if (!token) return null;
      const decoded: any = jwtDecode(token);
      return decoded['company_id'] || null;
    } catch (e) {
      console.error('Erro ao decodificar token para obter company_id:', e);
      return null;
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
    if (!this.isBrowser) return undefined;
    const token = this.currentTokenSubject.value;
    if (token) {
      try {
        const decodedToken = this.jwtHelper.decodeToken(token);
        return decodedToken.sub;
      } catch (e) {
        console.error('Erro ao decodificar o token para obter o ID do usuário:', e);
      }
    }
    return undefined;
  }

  registerUser(user: { email: string; password: string }): Observable<any> {
    return this.http.post('http://localhost:5000/api/v1/Auth/register', user);
  }

  async isLoggedIn(): Promise<boolean> {
    if (!this.isBrowser) return false;

    let token = this.currentTokenSubject.value;

    if (!token) {
      token = this.getToken();
      if (token) {
        console.log('[KeycloakOperationService] Token recuperado do localStorage.');
        this.currentTokenSubject.next(token);
      }
    }

    if (!token) {
      console.warn('[KeycloakOperationService] Nenhum token encontrado. Usuário não autenticado.');
      return false;
    }

    const isExpired = this.isTokenExpired(token);

    if (isExpired) {
      console.warn('[KeycloakOperationService] Token expirado. Tentando renovar...');
      try {
        await this.refreshToken().toPromise();
        const refreshed = this.getToken();
        if (refreshed && !this.isTokenExpired(refreshed)) {
          console.log('[KeycloakOperationService] Token renovado com sucesso.');
          return true;
        } else {
          console.error('[KeycloakOperationService] Falha ao renovar token.');
          await this.logout();
          return false;
        }
      } catch (e) {
        console.error('[KeycloakOperationService] Erro ao tentar renovar token:', e);
        await this.logout();
        return false;
      }
    }

    return true;
  }

  public getActiveContext(): ActiveContext | null {
    if (!this.isBrowser) return null;

    const token = this.getToken();
    const activeType = this.getActiveContextType();

    if (!token || !activeType) return null;

    try {
      const decoded: any = jwtDecode(token);

      const context: ActiveContext = {
        Type: activeType === 'user' ? 'User' : 'Company',
        Id: decoded.sub,
        Name: decoded.name || decoded.preferred_username,
        Email: decoded.email,
        ImageUrl: decoded.imageUrl,
        OwnerId: decoded.ownerId,
      };

      if (context.Type === 'Company') {
        context.Id = decoded['company_id'] || decoded.sub;
        context.Role = decoded['company_role'] || decoded['role'] || 'Worker';
        context.Name = decoded['company_name'] || context.Name;
      }

      return context;

    } catch (e) {
      console.error('Erro ao decodificar o token para obter o contexto ativo:', e);
      return null;
    }
  }


  isTokenExpired(token: string | undefined | null): boolean {
    if (!token) return true;
    try {
      const decodedToken = JSON.parse(atob(token.split('.')[1]));
      const expirationTime = decodedToken.exp * 1000;
      return Date.now() >= expirationTime;
    } catch (e) {
      console.error('KeycloakOperationService: Erro ao verificar expiração do token:', e);
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
      tap(tokens => this.saveTokens(tokens)),
      catchError(error => {
        console.error('KeycloakOperationService: Erro ao renovar token:', error);
        this.logout();
        return throwError(() => new Error('Falha na renovação do token.'));
      })
    );
  }

  async logout(): Promise<void> {
    if (this.isBrowser && !this.isLoggingOut) {
      this.isLoggingOut = true;
      console.log('KeycloakOperationService: Realizando logout (limpando localStorage).');

      localStorage.removeItem('user_token');
      localStorage.removeItem('company_token');
      localStorage.removeItem('active_token');
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      localStorage.removeItem('id_token');
      localStorage.removeItem('loginWithKeycloak');

      this.currentTokenSubject.next(null);

      await this.router.navigate(['/']);
      this.isLoggingOut = false;
    }
  }

  public setNewAccessToken(newToken: string): void {
    if (this.isBrowser && newToken) {
      try {
        const payload: any = JSON.parse(atob(newToken.split('.')[1]));
        const hasCompanyId = !!payload['company_id'];

        if (hasCompanyId) {
          localStorage.setItem('company_token', newToken);
          localStorage.setItem('active_token', 'company');
          localStorage.setItem('context_access_token', newToken);
          console.log('[KeycloakOperationService] Token de empresa armazenado.');
        } else {
          localStorage.setItem('user_token', newToken);
          localStorage.setItem('active_token', 'user');
          localStorage.setItem('context_access_token', newToken);
          console.log('[KeycloakOperationService] Token de usuário armazenado.');
        }

        this.currentTokenSubject.next(newToken);

      } catch (err) {
        console.error('Erro ao analisar token recebido em setNewAccessToken:', err);
      }
    }
  }

  public switchActiveToken(context: 'user' | 'company'): void {
    if (!this.isBrowser) return;

    const userToken = localStorage.getItem('user_token');
    const companyToken = localStorage.getItem('company_token');

    if (context === 'user' && userToken) {
      localStorage.setItem('active_token', 'user');
      localStorage.setItem('context_access_token', userToken);
      this.currentTokenSubject.next(userToken);
      console.log('[KeycloakOperationService] Alternado para token de usuário.');
    } else if (context === 'company' && companyToken) {
      localStorage.setItem('active_token', 'company');
      localStorage.setItem('context_access_token', companyToken);
      this.currentTokenSubject.next(companyToken);
      console.log('[KeycloakOperationService] Alternado para token de empresa.');
    } else {
      console.warn(`[KeycloakOperationService] Token de contexto "${context}" não encontrado.`);
    }
  }

  public getToken(): string | null {
    if (!this.isBrowser) {
      return null;
    }

    const activeType = localStorage.getItem('active_token');
    if (activeType === 'company') {
      return localStorage.getItem('company_token');
    } else if (activeType === 'user') {
      return localStorage.getItem('user_token');
    }

    return null;
  }
}
