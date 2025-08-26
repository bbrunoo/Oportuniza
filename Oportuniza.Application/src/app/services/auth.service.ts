import { jwtDecode } from "jwt-decode";
import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { isPlatformBrowser } from '@angular/common';
import { KeycloakOperationService } from "./keycloak.service";

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  apiUrl = 'http://localhost:5000/api/v1/Auth';

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object,
    private keycloakOperationService: KeycloakOperationService
  ) { }

  login(credentials: { email: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, credentials);
  }

  register(user: {
    name: string,
    email: string;
    password: string;
  }): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  async logout(): Promise<void> {
    await this.keycloakOperationService.logout();
    sessionStorage.clear();
    localStorage.clear();
    this.router.navigate(['/']);
  }

  async isAuthenticated(): Promise<boolean> {
    return this.keycloakOperationService.isLoggedIn();
  }

  async getUserData(): Promise<any | null> {
    const token = await this.keycloakOperationService.getToken();

    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);

      return {
        id: decoded.sub, // 'sub' é a claim padrão para o ID do usuário no Keycloak
        email: decoded.email,
        name: decoded.name,
        isACompany: decoded.isACompany
      };
    } catch {
      return null;
    }
  }
}
