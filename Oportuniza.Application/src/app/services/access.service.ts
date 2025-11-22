import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { KeycloakOperationService } from './keycloak.service';
import { AccessContext } from '../pages/Authentication/context-switcher/context-switcher.component';

@Injectable({
  providedIn: 'root'
})
export class AccessService {
  private apiUrl = `${environment.apiUrl}/Account`;

  constructor(
    private http: HttpClient,
    private keycloakService: KeycloakOperationService
  ) { }

  getContexts(): Observable<AccessContext[]> {
    return this.http.get<AccessContext[]>(`${this.apiUrl}/contexts`);
  }

  switchContext(companyId: string): Observable<any> {
    return this.http.post<{ token: string }>(`${this.apiUrl}/switch-context/${companyId}`, {})
      .pipe(
        tap(response => {
          if (response?.token) {
            localStorage.setItem('company_token', response.token);
            localStorage.setItem('active_token', response.token);

            this.keycloakService.setNewAccessToken(response.token);
          }
        })
      );
  }


}
