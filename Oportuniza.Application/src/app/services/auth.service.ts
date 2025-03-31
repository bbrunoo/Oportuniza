import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { catchError, map, Observable, switchMap } from 'rxjs';
import { User } from '../models/User.model';
import { loggedUser } from '../models/loggedUser.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  apiUrl = 'https://localhost:5000/api/v1/Auth';

  constructor(private http: HttpClient) {}

  login(credentials: { email: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, credentials);
  }

  register(user: {
    name:string,
    email: string;
    password: string;
  }): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  // logout() {
  //   localStorage.removeItem('token');
  //   localStorage.removeItem('refreshToken');
  // }

  // public getToken() {
  //   return localStorage.getItem('token');
  // }

  // private getRefreshToken() {
  //   return localStorage.getItem('refreshToken');
  // }

  // isLoggedIn(): boolean {
  //   return !!this.getToken();
  // }

  // getUserProfile(): Observable<loggedUser> {
  //   const token = this.getToken();
  //   if (!token) {
  //     console.error('Token JWT não encontrado no localStorage');
  //     return new Observable<loggedUser>();
  //   }

  //   const headers = new HttpHeaders({
  //     'Content-Type': 'application/json',
  //     Authorization: `Bearer ${token}`,
  //   });

  //   return this.http
  //     .get<loggedUser>(`${this.apiUrl}/user-infos`, { headers })
  //     .pipe(
  //       catchError((error) => {
  //         if (error.status === 401) {
  //           return this.refreshToken().pipe(
  //             switchMap(() => this.getUserProfile())
  //           );
  //         }
  //         throw error;
  //       })
  //     );
  // }

  // public refreshToken(): Observable<any> {
  //   const refreshToken = this.getRefreshToken();
  //   if (!refreshToken) {
  //     console.error('Refresh token não encontrado');
  //     return new Observable();
  //   }

  //   const headers = new HttpHeaders({
  //     'Content-Type': 'application/json',
  //     Authorization: `Bearer ${refreshToken}`,
  //   });

  //   return this.http
  //     .post<any>(`${this.apiUrl}/refresh-token`, {}, { headers })
  //     .pipe(
  //       map((response: any) => {
  //         localStorage.setItem('token', response.token);
  //         localStorage.setItem('refreshToken', response.refreshToken);
  //         return response;
  //       })
  //     );
  // }

  // isTokenExpired(): boolean {
  //   const token = this.getToken();
  //   if (!token) return true;

  //   const decodedToken: any = jwtDecode(token);
  //   const expirationTime = decodedToken.exp * 1000;
  //   return Date.now() > expirationTime;
  // }

  // getUserIdFromToken(): string | null {
  //   const token = this.getToken();
  //   if (!token) {
  //     console.error('Token não encontrado no localStorage');
  //     return null;
  //   }

  //   try {
  //     const decodedToken: any = jwtDecode(token);
  //     return decodedToken['id'];
  //   } catch (error) {
  //     console.error('Erro ao decodificar o token', error);
  //     return null;
  //   }
  // }
}
