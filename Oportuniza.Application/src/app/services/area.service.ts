import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AreaInteresse } from '../models/AreaInteresse.model';

@Injectable({
  providedIn: 'root'
})
export class AreaService {
  private apiUrl = "http://localhost:5000/api/AreaOfInterest";

  constructor(private http: HttpClient) { }

  getFilteredAreas(search: string): Observable<AreaInteresse[]> {
    return this.http.get<AreaInteresse[]>(`${this.apiUrl}/search?areaName=${search}`);
  }

  getAllAreas(): Observable<AreaInteresse[]> {
    return this.http.get<AreaInteresse[]>(`${this.apiUrl}`);
  }
}
