import { PublicationCreate } from './../models/publication-create.model';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Publication } from '../models/Publications.model';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PublicationService {
  constructor(private http: HttpClient) { }
  private apiUrl = `${environment.apiUrl}/Publication`;
  private apiUrlNormal = `${environment.apiUrlNormal}`;

  getPublications() {
    return this.http.get<Publication[]>(`${this.apiUrl}`);
  }

  getMyPublications(pageNumber: number = 1, pageSize: number = 10): Observable<any> {
    return this.http.get<any>(`${this.apiUrlNormal}/my`, {
      params: {
        pageNumber: pageNumber,
        pageSize: pageSize
      }
    });
  }

  disablePublication(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/disable/${id}`, {});
  }

  createPublication(dto: PublicationCreate, image: File) {

    const formData = new FormData();
    formData.append('Title', dto.title);
    formData.append('Description', dto.content);
    formData.append('Salary', dto.salary);

    formData.append('Shift', dto.shift);
    formData.append('Contract', dto.contract);
    formData.append('Local', dto.local);
    formData.append('ExpirationDate', dto.expirationDate);

    formData.append('Tags', JSON.stringify(dto.tags));


    if (dto.postAsCompanyId) {
      formData.append('PostAsCompanyId', dto.postAsCompanyId);
    }

    if (image) {
      formData.append('Image', image);
    }

    return this.http.post(this.apiUrl, formData);
  }
}
