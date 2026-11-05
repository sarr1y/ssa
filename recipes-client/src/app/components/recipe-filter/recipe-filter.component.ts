import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Cuisine } from '../../models/cuisine.model';
import { RecipeFilterParams } from '../../models/recipe.model';

@Component({
  selector: 'app-recipe-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card shadow-sm border-0 rounded-4 p-4 mb-4 bg-white filter-container">
      <div class="row g-3 align-items-end">
        <!-- Поиск -->
        <div class="col-md-4 col-12">
          <label class="form-label fw-semibold small text-secondary">
            <i class="bi bi-search me-1"></i> Поиск по названию или ингредиентам
          </label>
          <div class="input-group">
            <input type="text"
                   class="form-control rounded-pill-start border-end-0"
                   placeholder="Например: сыр, паста, стейк..."
                   [(ngModel)]="currentSearch"
                   (keyup.enter)="applyFilters()">
            <button class="btn btn-outline-secondary border-start-0 rounded-pill-end"
                    type="button"
                    (click)="applyFilters()">
              <i class="bi bi-search"></i>
            </button>
          </div>
        </div>

        <!-- Кухня -->
        <div class="col-md-3 col-sm-6 col-12">
          <label class="form-label fw-semibold small text-secondary">
            <i class="bi bi-globe-americas me-1"></i> Кухня мира
          </label>
          <select class="form-select rounded-pill"
                  [(ngModel)]="currentCuisineId"
                  (change)="applyFilters()">
            <option [ngValue]="undefined">Все кухни</option>
            @for (c of cuisines(); track c.id) {
              <option [ngValue]="c.id">
                {{ c.name }} ({{ c.recipeCount }})
              </option>
            }
          </select>
        </div>

        <!-- Время готовки -->
        <div class="col-md-2 col-sm-6 col-12">
          <label class="form-label fw-semibold small text-secondary">
            <i class="bi bi-stopwatch me-1"></i> Время до (мин)
          </label>
          <input type="number"
                 class="form-control rounded-pill"
                 placeholder="Напр. 60"
                 min="1"
                 [(ngModel)]="currentMaxTime"
                 (change)="applyFilters()">
        </div>

        <!-- Сортировка -->
        <div class="col-md-3 col-12">
          <label class="form-label fw-semibold small text-secondary">
            <i class="bi bi-arrow-down-up me-1"></i> Сортировка
          </label>
          <select class="form-select rounded-pill"
                  [(ngModel)]="currentSortBy"
                  (change)="applyFilters()">
            <option value="newest">Сначала новые</option>
            <option value="time_asc">Время: по возрастанию</option>
            <option value="time_desc">Время: по убыванию</option>
            <option value="oldest">Сначала старые</option>
          </select>
        </div>
      </div>

      <!-- Кнопка сброса -->
      @if (hasActiveFilters()) {
        <div class="mt-3 pt-3 border-top d-flex justify-content-end">
          <button (click)="onReset()" class="btn btn-sm btn-link text-decoration-none text-danger">
            <i class="bi bi-x-circle me-1"></i> Сбросить все фильтры
          </button>
        </div>
      }
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
    .filter-container {
      box-shadow: 0 4px 20px rgba(0, 0, 0, 0.04) !important;
    }
  `]
})
export class RecipeFilterComponent {
  // Modern Angular Signals: input() and output()
  readonly cuisines = input<Cuisine[]>([]);
  readonly selectedCuisineId = input<number | undefined>(undefined);
  readonly maxTime = input<number | undefined>(undefined);
  readonly search = input<string>('');
  readonly sortBy = input<string>('newest');

  readonly filterChange = output<RecipeFilterParams>();
  readonly resetFilters = output<void>();

  currentSearch: string = '';
  currentCuisineId: number | undefined = undefined;
  currentMaxTime: number | undefined = undefined;
  currentSortBy: string = 'newest';

  ngOnInit(): void {
    this.currentSearch = this.search();
    this.currentCuisineId = this.selectedCuisineId();
    this.currentMaxTime = this.maxTime();
    this.currentSortBy = this.sortBy();
  }

  hasActiveFilters(): boolean {
    return !!(this.currentSearch || this.currentCuisineId || this.currentMaxTime || this.currentSortBy !== 'newest');
  }

  applyFilters(): void {
    this.filterChange.emit({
      page: 1, // Сбрасываем на 1 страницу при фильтрации
      search: this.currentSearch,
      cuisineId: this.currentCuisineId,
      maxTime: this.currentMaxTime,
      sortBy: this.currentSortBy as any
    });
  }

  onReset(): void {
    this.currentSearch = '';
    this.currentCuisineId = undefined;
    this.currentMaxTime = undefined;
    this.currentSortBy = 'newest';
    this.resetFilters.emit();
  }
}
