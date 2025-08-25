import { UserProfile } from './../models/UserProfile.model';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { jwtDecode } from 'jwt-decode';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http: HttpClient, private msalService: MsalService) { }

  apiUrl = 'http://localhost:5000/api/v1/User';
  uploadApi = 'http://localhost:5000/api/Upload/upload-profile-picture';

  getOwnProfile() {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
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

  getLoggedInUserId(): string | undefined {
    const activeMsalAccount = this.msalService.instance.getActiveAccount();
    if (activeMsalAccount) {
      return activeMsalAccount.homeAccountId;
    }

    const token = sessionStorage.getItem('access_token');
    if (token) {
      try {
        const decodedToken: any = jwtDecode(token);
        return decodedToken.sub || decodedToken.nameid;
      } catch (e) {
        console.error('Falha ao decodificar o token:', e);
        return undefined;
      }
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
