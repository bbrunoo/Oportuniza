import { Publication } from './../models/Publications.model';
import { HttpClient } from '@angular/common/http';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CandidateDTO, Candidatura, PublicationWithCandidates, UserApplication } from '../models/candidate.model';

@Injectable({
  providedIn: 'root',
})
export class CandidateService {
  private apiUrl = `${environment.apiUrl}/CandidateApplication`;

  constructor(private http: HttpClient) { }

  cancelApplication(applicationId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${applicationId}`);
  }

  applyToJob(publicationId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}`, { publicationId });
  }

  addCandidateExtra(applicationId: string, observation?: string, resumeFile?: File) {
    const formData = new FormData();
    if (observation) formData.append('Observation', observation);
    if (resumeFile) formData.append('Resume', resumeFile);

    return this.http.post(`${this.apiUrl}/${applicationId}/extra`, formData);
  }

  getMyPublicationsWithCandidates(): Observable<PublicationWithCandidates[]> {
    return this.http.get<PublicationWithCandidates[]>(`${this.apiUrl}/MyPublications/Candidates`);
  }

  getMyApplications(): Observable<UserApplication[]> {
    return this.http.get<UserApplication[]>(`${this.apiUrl}/MyApplications`);
  }

  getApplicationsByCompany(): Observable<CandidateDTO[]> {
    return this.http.get<CandidateDTO[]>(`${this.apiUrl}/MyCompanyApplications`);
  }

  getApplicationsByUser(): Observable<UserApplication[]> {
    return this.http.get<UserApplication[]>(`${this.apiUrl}/MyApplications`);
  }
}
