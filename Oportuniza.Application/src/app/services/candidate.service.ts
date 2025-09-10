import { HttpClient } from '@angular/common/http';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Candidato, Candidatura, HasAppliedResponse, PublicationWithCandidates } from '../models/candidatos.model';

@Injectable({
  providedIn: 'root',
})
export class CandidateService {
  private apiUrl = `${environment.apiUrl}/CandidateApplication`;

  constructor(private http: HttpClient) { }

  getMyApplications(): Observable<Candidatura[]> {
    return this.http.get<Candidatura[]>(`${this.apiUrl}/MyApplications`);
  }

  getApplicantsByJob(publicationId: string): Observable<Candidato[]> {
    return this.http.get<Candidato[]>(`${this.apiUrl}/ByPublication/${publicationId}`);
  }

  cancelApplication(applicationId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${applicationId}`);
  }

  applyToJob(publicationId: string): Observable<Candidatura> {
    return this.http.post<Candidatura>(`${this.apiUrl}`, { publicationId });
  }

  getMyPublicationsWithCandidates(): Observable<PublicationWithCandidates[]> {
    return this.http.get<PublicationWithCandidates[]>(
      `${this.apiUrl}/MyPublications/Candidates`
    );
  }
}
