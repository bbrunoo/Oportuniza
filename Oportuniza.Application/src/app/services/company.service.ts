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
  private companyApiUrl = 'http://localhost:5000/api/v1/Company';
  private uploadApiUrl = 'http://localhost:5000/api/Upload/upload-company-picture';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  uploadCompanyImage(file: File): Observable<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<{ imageUrl: string }>(this.uploadApiUrl, formData);
  }

  createCompany(companyData: CompanyCreatePayload): Observable<any> {
    return this.http.post<any>(this.companyApiUrl, companyData);
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
