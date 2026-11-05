import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Recipe } from '../../models/recipe.model';
import { RecipeService } from '../../services/recipe.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-recipe-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './recipe-detail.component.html',
  styleUrls: ['./recipe-detail.component.css']
})
export class RecipeDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly recipeService = inject(RecipeService);
  readonly authService = inject(AuthService);

  // 4 состояния: loading, error, empty, data
  readonly state = signal<'loading' | 'error' | 'empty' | 'data'>('loading');
  readonly errorMessage = signal<string>('');
  readonly recipe = signal<Recipe | null>(null);

  // Загрузка фото
  readonly isUploading = signal<boolean>(false);
  readonly uploadMessage = signal<string | null>(null);
  readonly uploadError = signal<string | null>(null);

  // Проверка: автор ли текущий пользователь
  readonly isAuthor = computed(() => {
    const r = this.recipe();
    if (!r) return false;
    return this.authService.isAuthor(r.authorId);
  });

  checkedIngredients = signal<Record<number, boolean>>({});

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id && !isNaN(id)) {
      this.loadRecipe(id);
    } else {
      this.state.set('empty');
    }
  }

  loadRecipe(id: number): void {
    this.state.set('loading');
    this.errorMessage.set('');

    this.recipeService.getRecipe(id).subscribe({
      next: (data) => {
        if (!data) {
          this.state.set('empty');
        } else {
          this.recipe.set(data);
          this.state.set('data');
        }
      },
      error: (err) => {
        if (err.status === 404) {
          this.state.set('empty');
        } else {
          this.errorMessage.set('Ошибка соединения с сервером при получении рецепта.');
          this.state.set('error');
        }
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;

    const file = input.files[0];
    const current = this.recipe();
    if (!current) return;

    // Клиентская валидация файла (до 5 МБ, jpg/png/webp)
    const allowed = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowed.includes(file.type)) {
      this.uploadError.set('Допустимы только файлы форматов JPG, PNG или WEBP.');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.uploadError.set('Размер файла не должен превышать 5 МБ.');
      return;
    }

    this.isUploading.set(true);
    this.uploadError.set(null);
    this.uploadMessage.set(null);

    this.recipeService.uploadImage(current.id, file).subscribe({
      next: (res) => {
        this.recipe.update(r => r ? { ...r, imageUrl: res.imageUrl } : null);
        this.isUploading.set(false);
        this.uploadMessage.set('Фото блюда успешно загружено и обновлено!');
        input.value = '';
      },
      error: (err) => {
        this.isUploading.set(false);
        if (err.status === 403) {
          this.uploadError.set('Отказано в доступе (403): загружать фото может только автор записи.');
        } else {
          const msg = err.error?.errors?.File?.[0] || err.error?.detail || 'Ошибка загрузки фото на сервер.';
          this.uploadError.set(msg);
        }
      }
    });
  }

  deleteRecipe(): void {
    const current = this.recipe();
    if (!current) return;

    if (confirm(`Вы уверены, что хотите удалить рецепт «${current.title}»?`)) {
      this.recipeService.deleteRecipe(current.id).subscribe({
        next: () => {
          this.router.navigate(['/']);
        },
        error: (err) => {
          if (err.status === 403) {
            alert('Отказано в доступе (403): удалять рецепт может только его автор.');
          } else {
            alert('Ошибка удаления: ' + (err.error?.detail || err.message));
          }
        }
      });
    }
  }

  toggleIngredient(index: number): void {
    this.checkedIngredients.update(map => ({
      ...map,
      [index]: !map[index]
    }));
  }

  getIngredientsList(): string[] {
    const ingredients = this.recipe()?.ingredients;
    if (!ingredients) return [];
    return ingredients
      .split('\n')
      .map(line => line.trim())
      .filter(line => line.length > 0);
  }

  getStepsList(): string[] {
    const steps = this.recipe()?.steps;
    if (!steps) return [];
    return steps
      .split('\n')
      .map(line => line.trim())
      .filter(line => line.length > 0);
  }

  formatTime(minutes: number): string {
    if (minutes < 60) return `${minutes} минут`;
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m === 0 ? `${h} ч.` : `${h} ч. ${m} мин.`;
  }
}
