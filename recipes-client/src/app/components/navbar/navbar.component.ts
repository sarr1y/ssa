import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark shadow-sm sticky-top" style="background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);">
      <div class="container py-1">
        <a class="navbar-brand d-flex align-items-center gap-2 fw-bold fs-4" routerLink="/">
          <span class="d-inline-flex align-items-center justify-content-center bg-white text-danger rounded-circle p-2 shadow-sm" style="width: 38px; height: 38px;">
            <i class="bi bi-fire fs-5 text-warning"></i>
          </span>
          <span class="brand-title text-white">Рецепты<span class="text-warning">.ру</span></span>
        </a>

        <button class="navbar-toggler border-0" type="button" data-bs-toggle="collapse" data-bs-target="#navContent">
          <span class="navbar-toggler-icon"></span>
        </button>

        <div class="collapse navbar-collapse" id="navContent">
          <ul class="navbar-nav me-auto mb-2 mb-lg-0 ms-lg-3">
            <li class="nav-item">
              <a class="nav-link text-white-50 px-3 d-flex align-items-center gap-1" routerLink="/" routerLinkActive="active text-white fw-semibold" [routerLinkActiveOptions]="{exact: true}">
                <i class="bi bi-collection"></i> Каталог рецептов
              </a>
            </li>
          </ul>

          <div class="d-flex align-items-center gap-2 flex-wrap">
            @if (authService.isLoggedIn()) {
              <a routerLink="/recipes/new" class="btn btn-primary btn-sm rounded-pill d-inline-flex align-items-center gap-2 px-3 py-2 shadow-sm">
                <i class="bi bi-plus-circle-fill"></i>
                <span>Добавить рецепт</span>
              </a>

              <div class="d-flex align-items-center gap-2 bg-dark bg-opacity-50 px-3 py-1 rounded-pill text-white small border border-secondary border-opacity-25 ms-2">
                <i class="bi bi-person-circle text-warning fs-6"></i>
                <span class="fw-semibold">{{ authService.currentUser()?.username }}</span>
                <button (click)="logout()" class="btn btn-link text-danger p-0 ms-2 text-decoration-none" title="Выйти">
                  <i class="bi bi-box-arrow-right fs-6"></i>
                </button>
              </div>
            } @else {
              <a routerLink="/login" class="btn btn-outline-light btn-sm rounded-pill px-3 py-2">
                <i class="bi bi-box-arrow-in-right me-1"></i> Войти
              </a>
              <a routerLink="/register" class="btn btn-warning btn-sm rounded-pill px-3 py-2 text-dark fw-semibold shadow-sm">
                <i class="bi bi-person-plus-fill me-1"></i> Регистрация
              </a>
            }
          </div>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .nav-link.active {
      color: #fff !important;
      background: rgba(255, 255, 255, 0.1);
      border-radius: 8px;
    }
  `]
})
export class NavbarComponent {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
