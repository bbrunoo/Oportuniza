import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { CompanyListDto } from '../models/company-list-dto-model';

export interface CompanyCreatePayload {
  name: string;
  description: string;
  imageUrl:string
}

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private companyApiUrl = 'https://localhost:5000/api/v1/Company';
  private uploadApiUrl = 'https://localhost:5000/api/Upload/upload-company-picture';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  uploadCompanyImage(file: File): Observable<{ imageUrl: string }> {
    const token = this.authService.getToken();
    if (!token) {
      return throwError(() => new Error('Token de autenticação não encontrado.'));
    }

    const formData = new FormData();
    formData.append('file', file, file.name);

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.post<{ imageUrl: string }>(this.uploadApiUrl, formData, { headers }).pipe(
      catchError(this.handleError)
    );
  }

  createCompany(companyData: CompanyCreatePayload): Observable<any> {
    const token = this.authService.getToken();
    if (!token) {
      return throwError(() => new Error('Token de autenticação não encontrado.'));
    }

    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });

    return this.http.post<any>(this.companyApiUrl, companyData, { headers }).pipe(
      catchError(this.handleError)
    );
  }

  getUserCompanies(): Observable<CompanyListDto[]> {
    return this.http.get<CompanyListDto[]>(`${this.companyApiUrl}/user-companies`);
  }

  private handleError(error: HttpErrorResponse) {
    let errorMessage = 'Ocorreu um erro desconhecido.';
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Erro: ${error.error.message}`;
    } else {
      const errorBody = error.error;
      errorMessage = `Erro código ${error.status}: ${errorBody.message || error.statusText}`;
    }
    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
