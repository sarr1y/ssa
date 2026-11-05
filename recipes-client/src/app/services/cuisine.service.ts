import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cuisine } from '../models/cuisine.model';

@Injectable({
  providedIn: 'root'
})
export class CuisineService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5000/api/cuisines';

  getCuisines(): Observable<Cuisine[]> {
    return this.http.get<Cuisine[]>(this.apiUrl);
  }
}
