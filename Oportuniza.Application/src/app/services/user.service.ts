import { UserProfile } from './../models/UserProfile.model';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
// import { useAuth } from '../authConfig';
// import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http: HttpClient) { }

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

}
