import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container py-5 text-center my-5">
      <div class="row justify-content-center">
        <div class="col-md-6 bg-white p-5 rounded-4 shadow-sm">
          <div class="display-1 fw-bold text-primary mb-3">404</div>
          <h2 class="fw-bold text-dark mb-3">Страница не найдена</h2>
          <p class="text-muted mb-4">
            К сожалению, запрашиваемая страница не существует или рецепт был удалён.
          </p>
          <a routerLink="/" class="btn btn-primary rounded-pill px-4 py-2 shadow-sm">
            <i class="bi bi-house-door-fill me-1"></i> Вернуться в каталог
          </a>
        </div>
      </div>
    </div>
  `
})
export class NotFoundComponent {}
