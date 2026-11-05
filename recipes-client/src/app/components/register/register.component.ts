import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-5 col-sm-8 col-11">
          <div class="card shadow border-0 rounded-4 overflow-hidden bg-white">
            <div class="card-header bg-warning bg-gradient text-dark p-4 text-center">
              <i class="bi bi-person-plus-fill display-4 mb-2 d-inline-block text-dark"></i>
              <h3 class="fw-bold mb-1">Регистрация шеф-повара</h3>
              <p class="mb-0 text-dark-50 small">Создайте аккаунт и делитесь своими рецептами</p>
            </div>

            <div class="card-body p-4 p-md-5">

              @if (errorMessage()) {
                <div class="alert alert-danger rounded-3 py-2 px-3 small d-flex align-items-center gap-2 mb-4" role="alert">
                  <i class="bi bi-exclamation-octagon-fill text-danger fs-5"></i>
                  <div>{{ errorMessage() }}</div>
                </div>
              }

              @if (loading()) {
                <div class="text-center py-4">
                  <div class="spinner-border text-warning mb-2" role="status"></div>
                  <div class="text-muted small">Регистрация в системе...</div>
                </div>
              } @else {
                <form (ngSubmit)="onSubmit()" novalidate>
                  <div class="mb-3">
                    <label class="form-label fw-semibold text-secondary small">
                      Имя пользователя (Логин)
                    </label>
                    <div class="input-group">
                      <span class="input-group-text bg-light text-muted border-end-0 rounded-pill-start">
                        <i class="bi bi-person"></i>
                      </span>
                      <input type="text"
                             class="form-control border-start-0 rounded-pill-end"
                             name="username"
                             [(ngModel)]="username"
                             required
                             minlength="3"
                             placeholder="Минимум 3 символа">
                    </div>
                  </div>

                  <div class="mb-4">
                    <label class="form-label fw-semibold text-secondary small">Пароль</label>
                    <div class="input-group">
                      <span class="input-group-text bg-light text-muted border-end-0 rounded-pill-start">
                        <i class="bi bi-lock"></i>
                      </span>
                      <input type="password"
                             class="form-control border-start-0 rounded-pill-end"
                             name="password"
                             [(ngModel)]="password"
                             required
                             minlength="6"
                             placeholder="Минимум 6 символов">
                    </div>
                  </div>

                  <button type="submit"
                          class="btn btn-warning w-100 rounded-pill py-2 fw-semibold shadow-sm mb-3 text-dark">
                    Зарегистрироваться
                  </button>

                  <div class="text-center small text-secondary">
                    Уже зарегистрированы?
                    <a routerLink="/login" class="fw-semibold text-primary text-decoration-none">
                      Войти
                    </a>
                  </div>
                </form>
              }

            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .rounded-pill-start {
      border-top-left-radius: 50rem !important;
      border-bottom-left-radius: 50rem !important;
    }
    .rounded-pill-end {
      border-top-right-radius: 50rem !important;
      border-bottom-right-radius: 50rem !important;
    }
  `]
})
export class RegisterComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  username = '';
  password = '';
  readonly loading = signal<boolean>(false);
  readonly errorMessage = signal<string | null>(null);

  onSubmit(): void {
    this.errorMessage.set(null);

    if (!this.username.trim() || !this.password) {
      this.errorMessage.set('Заполните логин и пароль.');
      return;
    }

    if (this.username.trim().length < 3) {
      this.errorMessage.set('Логин должен содержать не менее 3 символов.');
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage.set('Пароль должен содержать не менее 6 символов.');
      return;
    }

    this.loading.set(true);
    this.authService.register(this.username.trim(), this.password).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.loading.set(false);
        const msg = err.error?.errors?.Username?.[0]
          || err.error?.errors?.Password?.[0]
          || err.error?.detail
          || 'Ошибка регистрации на сервере.';
        this.errorMessage.set(msg);
      }
    });
  }
}
