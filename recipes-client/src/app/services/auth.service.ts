import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, UserInfo } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5000/api/auth';
  private readonly tokenKey = 'recipes_auth_token';
  private readonly userKey = 'recipes_auth_user';

  // Реактивные сигналы для пользователя и состояния авторизации
  readonly currentUser = signal<UserInfo | null>(this.loadUserFromStorage());
  readonly isLoggedIn = computed(() => this.currentUser() !== null);
  readonly currentUserId = computed(() => this.currentUser()?.id ?? null);

  private loadUserFromStorage(): UserInfo | null {
    try {
      const stored = localStorage.getItem(this.userKey);
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  register(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { username, password })
      .pipe(
        tap(res => this.handleAuthSuccess(res))
      );
  }

  login(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { username, password })
      .pipe(
        tap(res => this.handleAuthSuccess(res))
      );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUser.set(null);
  }

  private handleAuthSuccess(res: AuthResponse): void {
    localStorage.setItem(this.tokenKey, res.token);
    const userInfo: UserInfo = { id: res.userId, username: res.username };
    localStorage.setItem(this.userKey, JSON.stringify(userInfo));
    this.currentUser.set(userInfo);
  }

  // Проверка: является ли текущий пользователь автором рецепта
  isAuthor(authorId: number): boolean {
    const user = this.currentUser();
    return !!user && user.id === authorId;
  }
}
