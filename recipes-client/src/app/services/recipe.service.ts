import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Recipe, RecipeFilterParams, PagedResult } from '../models/recipe.model';

@Injectable({
  providedIn: 'root'
})
export class RecipeService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5000/api/recipes';
  private readonly baseUrl = 'http://localhost:5000';

  getRecipes(filters?: RecipeFilterParams): Observable<PagedResult<Recipe>> {
    let params = new HttpParams();

    if (filters) {
      if (filters.page && filters.page > 0) {
        params = params.set('page', filters.page.toString());
      }
      if (filters.pageSize && filters.pageSize > 0) {
        params = params.set('pageSize', filters.pageSize.toString());
      }
      if (filters.cuisineId && filters.cuisineId > 0) {
        params = params.set('cuisineId', filters.cuisineId.toString());
      }
      if (filters.maxTime && filters.maxTime > 0) {
        params = params.set('maxTime', filters.maxTime.toString());
      }
      if (filters.search && filters.search.trim().length > 0) {
        params = params.set('search', filters.search.trim());
      }
      if (filters.sortBy) {
        params = params.set('sortBy', filters.sortBy);
      }
    }

    return this.http.get<PagedResult<Recipe>>(this.apiUrl, { params });
  }

  getRecipe(id: number): Observable<Recipe> {
    return this.http.get<Recipe>(`${this.apiUrl}/${id}`);
  }

  createRecipe(dto: {
    title: string;
    cookingTimeMinutes: number;
    servings: number;
    ingredients: string;
    steps: string;
    cuisineId: number;
  }): Observable<Recipe> {
    return this.http.post<Recipe>(this.apiUrl, dto);
  }

  updateRecipe(id: number, dto: {
    title: string;
    cookingTimeMinutes: number;
    servings: number;
    ingredients: string;
    steps: string;
    cuisineId: number;
  }): Observable<Recipe> {
    return this.http.put<Recipe>(`${this.apiUrl}/${id}`, dto);
  }

  deleteRecipe(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  uploadImage(id: number, file: File): Observable<{ imageUrl: string; message: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ imageUrl: string; message: string }>(`${this.apiUrl}/${id}/image`, formData);
  }

  getImageUrl(relativePath?: string): string {
    if (!relativePath) {
      return 'assets/placeholder-food.svg';
    }
    if (relativePath.startsWith('http://') || relativePath.startsWith('https://')) {
      return relativePath;
    }
    return `${this.baseUrl}${relativePath.startsWith('/') ? '' : '/'}${relativePath}`;
  }
}
