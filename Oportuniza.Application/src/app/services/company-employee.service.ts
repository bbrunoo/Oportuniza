import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CompanyEmployeeDto } from '../models/company-get.model';
import { map, Observable } from 'rxjs';
import { UserSearchResult } from '../models/user-serach.model';

@Injectable({
  providedIn: 'root'
})
export class CompanyEmployeeService {
  private apiUrl = 'http://localhost:5000/api/v1/CompanyEmployee';

  constructor(private http: HttpClient) { }

  getOrderedEmployees(companyId: string): Observable<CompanyEmployeeDto[]> {
    return this.http.get<CompanyEmployeeDto[]>(`${this.apiUrl}/${companyId}/employees-ordered`);
  }

  updateEmployeeStatus(employeeId: string, newStatus: string): Observable<any> {
    const payload = { NewStatus: newStatus };
    return this.http.patch(`${this.apiUrl}/status/${employeeId}`, payload);
  }

  getUserRoles(companyId: string): Observable<string[]> {
    return this.http.get<{ roles: string }>(`${this.apiUrl}/${companyId}/roles`)
      .pipe(
        map(response => response.roles.split(',').map(r => r.trim()))
      );
  }

  toggleEmployeeStatus(id: string, newStatus: number) {
    return this.http.patch(`${this.apiUrl}/status/${id}`, { newStatus });
  }

  searchUserByEmail(email: string): Observable<UserSearchResult> {
    return this.http.get<UserSearchResult>(`${this.apiUrl}/search-user`, { params: { email } });
  }

  linkEmployee(email: string, companyId: string): Observable<any> {
    const payload = { email, companyId };
    return this.http.post(`${this.apiUrl}/register-employee`, payload);
  }

  updateEmployeeRoles(employeeId: string, updateData: any): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/roles/${employeeId}`, updateData);
  }

  unlinkCompany(companyId: string) {
    return this.http.delete<any>(`${this.apiUrl}/unlink/${companyId}`);
  }
}
