import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private http: HttpClient) { }

  apiUrl = 'https://localhost:5000/api/v1/User';

  updateProfile(profileData: {
    fullName: string;
    isACompany: boolean;
    interests: string;
  }, id: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/completar-perfil/${id}`, profileData);
  }
}
