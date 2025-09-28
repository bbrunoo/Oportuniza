import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CompanyEmployeeDto } from '../models/company-get.model';
import { map, Observable } from 'rxjs';
import { EmployeeRegisterPayload } from '../models/employee-register-request.model';

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

  registerEmployee(payload: EmployeeRegisterPayload): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register-employee`, payload);
  }

   getUserRoles(companyId: string): Observable<string[]> {
    return this.http.get<{ roles: string }>(`${this.apiUrl}/${companyId}/roles`)
      .pipe(
        map(response => response.roles.split(',').map(r => r.trim()))
      );
  }
}
