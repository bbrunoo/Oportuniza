import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class VerificationService {
  apiUrl = `${environment.apiUrl}/Verification`;

  constructor(private http: HttpClient) { }

  sendVerificationCode(email: string): Observable<any> {
    const body = { toEmail: email };
    return this.http.post(`${this.apiUrl}/send-verification`, body, { responseType: 'text' });
  }

    validateVerificationCode(email: string, code: string): Observable<any> {
    const body = { toEmail: email, code: code };
    return this.http.post(`${this.apiUrl}/validate-code`, body, { responseType: 'text' });
  }
}
