import { UserProfile } from './../models/UserProfile.model';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { map, Observable } from 'rxjs';
import { KeycloakOperationService } from './keycloak.service';
import { GetProfiles } from '../models/new-models/Profiles.model';
import { ProfileResponse } from '../models/profile-response.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(
    private http: HttpClient,
    private keycloakService: KeycloakOperationService
  ) { }

  apiUrl = 'http://localhost:5000/api/v1/User';
  uploadApi = 'http://localhost:5000/api/Upload/upload-profile-picture';

  getOwnProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(`${this.apiUrl}/profile`);
  }

  updateProfile(profileData: {
    fullName: string;
    imageUrl: string;
    phone: string;
    interests: string;
    areaOfInterestIds: string[];
  }, id: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/completar-perfil/${id}`, profileData);
  }

  uploadProfilePicture(file: File): Observable<{ imageUrl: string }> {
    if (!file) {
      throw new Error('Nenhum arquivo fornecido.');
    }

    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ imageUrl: string }>(`${this.uploadApi}`, formData);
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

  getUserId(): Observable<string> {
    return this.http.get<{ id: string }>(`${this.apiUrl}/getUserId`).pipe(
      map(response => {
        return response.id;
      })
    );
  }
}
