import { PublicationCreate } from './../models/publication-create.model';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Publication } from '../models/Publications.model';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { PublicationFilterDto } from '../models/filter.model';
import { PublicationUpdateDto } from '../models/PublicationUpdate.model';

@Injectable({
  providedIn: 'root',
})
export class PublicationService {
  constructor(private http: HttpClient) { }
  private apiUrl = `${environment.apiUrl}/Publication`;
  private apiUrlNormal = `${environment.apiUrlNormal}`;

  getPublications() {
    return this.http.get<Publication[]>(`${this.apiUrl}`);
  }

  getPublicationsById(id: string) {
    return this.http.get<Publication>(`${this.apiUrl}/${id}`);
  }

  getMyPublications(
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/my`, {
      params: {
        pageNumber: pageNumber,
        pageSize: pageSize,
      },
    });
  }

  disablePublication(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/disable/${id}`, {});
  }

  sendPostVerificationCode(email: string): Observable<any> {
    return this.http.post(`${environment.apiUrl}/Verification/send-post-code`, { email });
  }

  createPublicationWithCode(dto: PublicationCreate, image: File, verificationCode: string) {
    const formData = new FormData();
    formData.append('Title', dto.title);
    formData.append('Description', dto.description);
    formData.append('Salary', dto.salary);
    formData.append('Shift', dto.shift);
    formData.append('Contract', dto.contract);
    formData.append('Local', dto.local);
    formData.append('ExpirationDate', dto.expirationDate);
    formData.append('CityId', dto.cityId);
    formData.append('verificationCode', verificationCode);

    if (dto.postAsCompanyId) formData.append('PostAsCompanyId', dto.postAsCompanyId);
    if (image) formData.append('Image', image);

    return this.http.post(`${this.apiUrl}/create`, formData);
  }

  getPublicationById(id: string): Observable<Publication> {
    return this.http.get<Publication>(`${this.apiUrl}/${id}`);
  }

  updatePublication(id: string, dto: PublicationUpdateDto, image?: File): Observable<any> {
    const formData = new FormData();
    formData.append('Title', dto.title);
    formData.append('Resumee', dto.resumee);
    formData.append('Salary', dto.salary);
    formData.append('Shift', dto.shift);
    formData.append('Contract', dto.contract);
    formData.append('Local', dto.local);
    formData.append('ExpirationDate', dto.expirationDate);
    formData.append('AuthorUserId', dto.authorUserId);

    if (image) {
      formData.append('Image', image, image.name);
    }

    return this.http.put(`${this.apiUrl}/${id}`, formData);
  }

  filterPublications(filters: PublicationFilterDto): Observable<Publication[]> {
    let params = new HttpParams();

    if (filters.searchTerm) {
      params = params.append('searchTerm', filters.searchTerm);
    }
    if (filters.local) {
      params = params.append('local', filters.local);
    }
    if (filters.contracts && filters.contracts.length > 0) {
      filters.contracts.forEach((contract) => {
        params = params.append('contracts', contract);
      });
    }
    if (filters.shifts && filters.shifts.length > 0) {
      filters.shifts.forEach((shift) => {
        params = params.append('shifts', shift);
      });
    }
    if (filters.salaryRange) {
      params = params.append('salaryRange', filters.salaryRange);
    }

    if (filters.latitude != null) {
      params = params.append('latitude', filters.latitude.toString());
    }
    if (filters.longitude != null) {
      params = params.append('longitude', filters.longitude.toString());
    }
    if (filters.radiusKm != null && filters.radiusKm > 0) {
      params = params.append('radiusKm', filters.radiusKm.toString());
    }

    console.log('➡️ Parâmetros enviados à API:', {
      searchTerm: filters.searchTerm,
      local: filters.local,
      contracts: filters.contracts,
      shifts: filters.shifts,
      salaryRange: filters.salaryRange,
      latitude: filters.latitude,
      longitude: filters.longitude,
      radiusKm: filters.radiusKm,
      finalUrl: `${this.apiUrl}/search?${params.toString()}`
    });

    return this.http.get<Publication[]>(`${this.apiUrl}/search`, { params });
  }

  validateImageSafety(formData: FormData) {
    return this.http.post<{ isSafe: boolean }>(`${this.apiUrl}/validate-image`, formData);
  }
}
