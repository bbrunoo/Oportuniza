import { PublicationCreate } from './../models/publication-create.model';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Publication } from '../models/Publications.model';

@Injectable({
  providedIn: 'root'
})
export class PublicationService {
  constructor(private http: HttpClient) { }
  apiUrl = 'https://localhost:5000/api/Publication';

  getPublications() {
    return this.http.get<Publication[]>(`${this.apiUrl}`,);
  }

 createPublication(dto: PublicationCreate, image: File) {
    const formData = new FormData();
    formData.append('Title', dto.title);
    formData.append('Description', dto.content);
    formData.append('Salary', dto.salary);

    if (dto.postAsCompanyId) {
      formData.append('PostAsCompanyId', dto.postAsCompanyId);
    }

    if (image) {
      formData.append('Image', image);
    }

    return this.http.post(this.apiUrl, formData);
  }
}
