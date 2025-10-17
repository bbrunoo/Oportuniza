import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, of } from 'rxjs';
import { CompanyListDto } from '../models/company-list-dto-model';
import { CompanyPaginatedResponse } from '../models/company-paginated-response.model';
import { CompanyDto } from '../models/company-get.model';
import { CompanyUpdatePayload } from '../models/company-update.model';

export interface CompanyCreatePayload {
  name: string;
  description: string;
  imageUrl: string
  cityState: string;
  phone: string;
  email: string;
  cnpj: string;
}

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private companyApiUrl = 'http://localhost:5000/api/v1/Company';
  private uploadApiUrl = 'http://localhost:5000/api/Upload/upload-company-picture';
  private readonly brasilApiUrl = 'https://brasilapi.com.br/api/cnpj/v1';

  constructor(
    private http: HttpClient,
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

  updateCompanyStatus(companyId: string, newStatus: number): Observable<void> {
    const payload = { NewStatus: newStatus };
    return this.http.patch<void>(`${this.companyApiUrl}/status/${companyId}`, payload);
  }

  getUserCompaniesPaginated(pageNumber: number, pageSize: number): Observable<CompanyPaginatedResponse> {
    const params = {
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString()
    };
    return this.http.get<CompanyPaginatedResponse>(`${this.companyApiUrl}/user-companies-paginated`, { params });
  }

  getCompanyById(id: string): Observable<CompanyDto> {
    return this.http.get<CompanyDto>(`${this.companyApiUrl}/${id}`);
  }

  updateCompany(id: string, companyData: CompanyUpdatePayload): Observable<any> {
    return this.http.put(`${this.companyApiUrl}/${id}`, companyData);
  }

  disableCompany(id: string): Observable<any> {
    return this.http.patch(`${this.companyApiUrl}/disable/${id}`, {});
  }

  consultarCnpj(cnpj: string): Observable<{ ativo: boolean }> {
    const cnpjLimpo = cnpj.replace(/\D/g, '');

    return this.http.get<any>(`${this.brasilApiUrl}/${cnpjLimpo}`).pipe(
      map((dados) => {
        const ativo = dados?.descricao_situacao_cadastral?.toUpperCase() === 'ATIVA';
        return { ativo };
      }),
      catchError((err) => {
        console.error('Erro ao consultar CNPJ:', err);
        return of({ ativo: false });
      })
    );
  }
}
