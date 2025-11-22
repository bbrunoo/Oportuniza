import { UserProfile } from './../models/UserProfile.model';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { map, Observable } from 'rxjs';
import { KeycloakOperationService } from './keycloak.service';
import { GetProfiles } from '../models/new-models/Profiles.model';
import { ProfileResponse } from '../models/profile-response.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(
    private http: HttpClient,
    private keycloakService: KeycloakOperationService
  ) { }

  apiUrl = 'http://localhost:5000/api/v1/User';
  private verifyApi = `${environment.apiUrl}/Publication`;

  getOwnProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(`${this.apiUrl}/profile`);
  }

  validateImageSafety(formData: FormData) {
    return this.http.post<{ isSafe: boolean }>(`${this.verifyApi}/validate-image`, formData);
  }

  async getLoggedInUserId(): Promise<string | undefined> {
    try {
      const isLoggedIn = await this.keycloakService.isLoggedIn();
      if (isLoggedIn) {
        const token = await this.keycloakService.getToken();

        if (token) {
          const decodedToken: any = jwtDecode(token);
          return decodedToken.sub || decodedToken.id;
        }
      }
    } catch (e) {
      console.error('Falha ao obter o ID do usuário do Keycloak:', e);
    }
    return undefined;
  }

  getUserById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  getUserId(): Observable<string> {
    return this.http.get<{ id: string }>(`${this.apiUrl}/getUserId`).pipe(
      map(response => {
        return response.id;
      })
    );
  }

  editProfile(name: string, location: string, imageFile?: File): Observable<any> {
    const formData = new FormData();
    formData.append('name', name);
    formData.append('location', location);
    if (imageFile) formData.append('image', imageFile);

    return this.http.put(`${this.apiUrl}/editar-perfil`, formData);
  }
}
