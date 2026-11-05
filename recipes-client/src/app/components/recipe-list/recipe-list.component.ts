import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Recipe, RecipeFilterParams } from '../../models/recipe.model';
import { Cuisine } from '../../models/cuisine.model';
import { RecipeService } from '../../services/recipe.service';
import { CuisineService } from '../../services/cuisine.service';
import { AuthService } from '../../services/auth.service';
import { RecipeCardComponent } from '../recipe-card/recipe-card.component';
import { RecipeFilterComponent } from '../recipe-filter/recipe-filter.component';

@Component({
  selector: 'app-recipe-list',
  standalone: true,
  imports: [CommonModule, RouterModule, RecipeCardComponent, RecipeFilterComponent],
  templateUrl: './recipe-list.component.html',
  styleUrls: ['./recipe-list.component.css']
})
export class RecipeListComponent implements OnInit {
  private readonly recipeService = inject(RecipeService);
  private readonly cuisineService = inject(CuisineService);
  readonly authService = inject(AuthService);

  // Состояние экрана (4 состояния: loading, error, empty, data)
  readonly state = signal<'loading' | 'error' | 'empty' | 'data'>('loading');
  readonly errorMessage = signal<string>('');

  // Сигналы данных
  readonly recipes = signal<Recipe[]>([]);
  readonly cuisines = signal<Cuisine[]>([]);
  readonly total = signal<number>(0);
  readonly page = signal<number>(1);
  readonly pageSize = signal<number>(6);
  readonly totalPages = signal<number>(1);

  // Текущие фильтры
  readonly filterParams = signal<RecipeFilterParams>({
    page: 1,
    pageSize: 6,
    sortBy: 'newest'
  });

  readonly hasNextPage = computed(() => this.page() < this.totalPages());
  readonly hasPrevPage = computed(() => this.page() > 1);

  ngOnInit(): void {
    this.loadCuisines();
    this.loadRecipes();
  }

  loadCuisines(): void {
    this.cuisineService.getCuisines().subscribe({
      next: (data) => this.cuisines.set(data),
      error: (err) => console.error('Ошибка загрузки справочника кухонь:', err)
    });
  }

  loadRecipes(): void {
    this.state.set('loading');
    this.errorMessage.set('');

    const params: RecipeFilterParams = {
      ...this.filterParams(),
      page: this.page(),
      pageSize: this.pageSize()
    };

    this.recipeService.getRecipes(params).subscribe({
      next: (res) => {
        this.recipes.set(res.items);
        this.total.set(res.total);
        this.totalPages.set(res.totalPages || 1);

        if (res.items.length === 0) {
          this.state.set('empty');
        } else {
          this.state.set('data');
        }
      },
      error: (err) => {
        this.errorMessage.set('Не удалось загрузить список рецептов. Убедитесь, что сервер API запущен.');
        this.state.set('error');
        console.error(err);
      }
    });
  }

  onFilterChange(newFilters: RecipeFilterParams): void {
    this.page.set(1);
    this.filterParams.set({
      ...this.filterParams(),
      ...newFilters,
      page: 1
    });
    this.loadRecipes();
  }

  onResetFilters(): void {
    this.page.set(1);
    this.filterParams.set({
      page: 1,
      pageSize: 6,
      sortBy: 'newest'
    });
    this.loadRecipes();
  }

  nextPage(): void {
    if (this.hasNextPage()) {
      this.page.update(p => p + 1);
      this.loadRecipes();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  prevPage(): void {
    if (this.hasPrevPage()) {
      this.page.update(p => p - 1);
      this.loadRecipes();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  goToPage(p: number): void {
    if (p >= 1 && p <= this.totalPages() && p !== this.page()) {
      this.page.set(p);
      this.loadRecipes();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  onDeleteRecipe(id: number): void {
    this.recipeService.deleteRecipe(id).subscribe({
      next: () => {
        this.loadRecipes();
      },
      error: (err) => {
        alert(err.status === 403
          ? 'Отказано в доступе (403): удалять рецепт может только его автор.'
          : 'Ошибка при удалении рецепта.');
      }
    });
  }

  isRecipeAuthor(authorId: number): boolean {
    return this.authService.isAuthor(authorId);
  }
}
