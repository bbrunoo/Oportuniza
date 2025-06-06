import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http: HttpClient) { }

  apiUrl = 'https://localhost:5000/api/v1/User';
  uploadApi = 'https://localhost:5000/api/Upload/upload-profile-picture';

  updateProfile(profileData: {
    fullName: string;
    isACompany: boolean;
    interests: string;
    imageUrl: string;
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
