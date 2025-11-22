import { environment } from './../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CityService {
  private apiUrl = `${environment.apiUrl}/City`;

  constructor(private http: HttpClient) { }

  getCities(page: number = 1, pageSize: number = 20) {
    return this.http.get<any>(`${this.apiUrl}`, {
      params: { page, pageSize }
    }).pipe(
      map(res => res.data)
    );
  }

  searchCities(name: string, page: number = 1, pageSize: number = 20) {
    return this.http.get<any>(`${this.apiUrl}/search`, {
      params: { name, page, pageSize }
    }).pipe(
      map(res => res.data || res)
    );
  }

}
