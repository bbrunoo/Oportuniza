import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { UserProfile } from '../models/UserProfile.model';
import { Observable } from 'rxjs';
import { UserInfoModel } from '../models/UserInfo.model';
import { AllUsersInfoModel } from '../models/AllUsersInfo.model';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {

  constructor(private http: HttpClient) { }
  apiUrl = 'https://localhost:5000/api/Profile';

  getUserProfile(id: string) {
    return this.http.get<UserProfile>(`${this.apiUrl}/${id}`,);
  }

  getUserProfileData(id: string): Observable<UserInfoModel> {
    return this.http.get<UserInfoModel>(`${this.apiUrl}/profile-data/${id}`);
  }

   getAllProfiles(): Observable<AllUsersInfoModel[]> {
    return this.http.get<AllUsersInfoModel[]>(`${this.apiUrl}/all-profiles`);
  }
}
