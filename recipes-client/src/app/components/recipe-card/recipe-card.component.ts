import { Component, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Recipe } from '../../models/recipe.model';
import { RecipeService } from '../../services/recipe.service';

@Component({
  selector: 'app-recipe-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="card recipe-card h-100 shadow-sm border-0 rounded-4 overflow-hidden position-relative">
      <!-- Изображение блюда -->
      <div class="card-img-wrapper position-relative overflow-hidden bg-light" style="height: 220px;">
        <img [src]="recipeService.getImageUrl(recipe().imageUrl)"
             [alt]="recipe().title"
             class="card-img-top w-100 h-100 object-fit-cover recipe-img transition-zoom"
             loading="lazy">
        <span class="badge bg-primary bg-gradient position-absolute top-0 end-0 m-3 px-3 py-2 rounded-pill shadow-sm">
          {{ recipe().cuisineName }}
        </span>
      </div>

      <div class="card-body d-flex flex-column p-4">
        <h5 class="card-title fw-bold text-dark mb-2 text-truncate" [title]="recipe().title">
          {{ recipe().title }}
        </h5>

        <div class="d-flex align-items-center gap-3 text-muted small mb-3">
          <span class="d-flex align-items-center gap-1">
            <i class="bi bi-clock-history text-primary"></i>
            {{ recipe().cookingTimeMinutes }} мин.
          </span>
          <span class="d-flex align-items-center gap-1">
            <i class="bi bi-people-fill text-primary"></i>
            {{ recipe().servings }} порц.
          </span>
          <span class="d-flex align-items-center gap-1 ms-auto text-secondary fst-italic">
            <i class="bi bi-person-circle"></i>
            {{ recipe().authorUsername }}
          </span>
        </div>

        <p class="card-text text-secondary small flex-grow-1 line-clamp-3 mb-4">
          {{ recipe().steps }}
        </p>

        <div class="d-flex gap-2 align-items-center mt-auto pt-2 border-top">
          <a [routerLink]="['/recipes', recipe().id]" class="btn btn-outline-primary rounded-pill btn-sm flex-grow-1 fw-medium">
            <i class="bi bi-book-half me-1"></i> Читать
          </a>

          @if (isAuthor()) {
            <a [routerLink]="['/recipes', recipe().id, 'edit']"
               class="btn btn-outline-secondary rounded-circle btn-sm p-2 action-btn"
               title="Редактировать">
              <i class="bi bi-pencil-fill"></i>
            </a>
            <button (click)="onDelete()"
                    type="button"
                    class="btn btn-outline-danger rounded-circle btn-sm p-2 action-btn"
                    title="Удалить">
              <i class="bi bi-trash-fill"></i>
            </button>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .recipe-card {
      transition: transform 0.25s ease, box-shadow 0.25s ease;
      background: #ffffff;
    }
    .recipe-card:hover {
      transform: translateY(-5px);
      box-shadow: 0 12px 28px rgba(0, 0, 0, 0.1) !important;
    }
    .transition-zoom {
      transition: transform 0.4s ease;
    }
    .recipe-card:hover .transition-zoom {
      transform: scale(1.05);
    }
    .line-clamp-3 {
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    .action-btn {
      width: 34px;
      height: 34px;
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }
  `]
})
export class RecipeCardComponent {
  readonly recipeService = inject(RecipeService);

  // Modern Angular Signals: input() and output()
  readonly recipe = input.required<Recipe>();
  readonly isAuthor = input<boolean>(false);
  readonly delete = output<number>();

  onDelete(): void {
    if (confirm(`Вы действительно хотите удалить рецепт «${this.recipe().title}»?`)) {
      this.delete.emit(this.recipe().id);
    }
  }
}
