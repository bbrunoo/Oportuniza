import { HttpClient } from '@angular/common/http';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CandidateService {
  private apiUrl = `${environment.apiUrl}/CandidateApplication`;

  constructor(private http: HttpClient) { }

  applyToJob(publicationId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}`, { publicationId });
  }

  getMyApplications(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/MyApplications`);
  }

  getApplicantsByJob(publicationId: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/ByPublication/${publicationId}`
    );
  }

  cancelApplication(applicationId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${applicationId}`);
  }

  hasApplied(
    publicationId: string,
    userId: string
  ): Observable<{ hasApplied: boolean }> {
    return this.http.get<{ hasApplied: boolean }>(
      `${this.apiUrl}/HasApplied?publicationId=${publicationId}&userId=${userId}`
    );
  }
}
