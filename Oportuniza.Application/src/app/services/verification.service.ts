import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class VerificationService {
  apiUrl = `${environment.apiUrl}/Verification`;

  constructor(private http: HttpClient) { }

  sendVerificationCode(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/send`, { email });
  }

  validateVerificationCode(email: string, code: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/validate`, { email, code });
  }
}
