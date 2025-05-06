import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UserProfile } from '../models/UserProfile.model';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {

  constructor(private http: HttpClient) { }
  apiUrl = 'https://localhost:5000/api/Profile';

  getUserProfile(id: string) {
    return this.http.get<UserProfile>(`${this.apiUrl}/${id}`,);
  }
}
