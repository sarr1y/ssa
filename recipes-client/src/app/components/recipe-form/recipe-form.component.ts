import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RecipeService } from '../../services/recipe.service';
import { CuisineService } from '../../services/cuisine.service';
import { AuthService } from '../../services/auth.service';
import { Cuisine } from '../../models/cuisine.model';
import { Recipe } from '../../models/recipe.model';

// Особое правило вашего варианта: «Если время больше 240 минут, шаги не короче 200 символов.»
export function specialCookingTimeRuleValidator(group: AbstractControl): ValidationErrors | null {
  const time = group.get('cookingTimeMinutes')?.value;
  const steps = (group.get('steps')?.value || '').trim();

  if (time && Number(time) > 240) {
    if (steps.length < 200) {
      return {
        specialRuleViolation: {
          time: Number(time),
          actualLength: steps.length,
          requiredLength: 200
        }
      };
    }
  }
  return null;
}

@Component({
  selector: 'app-recipe-form',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './recipe-form.component.html',
  styleUrls: ['./recipe-form.component.css']
})
export class RecipeFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly recipeService = inject(RecipeService);
  private readonly cuisineService = inject(CuisineService);
  readonly authService = inject(AuthService);

  // 4 состояния экрана: loading, error, empty, data
  readonly state = signal<'loading' | 'error' | 'empty' | 'data'>('data');
  readonly generalError = signal<string | null>(null);

  // Ошибки с сервера в формате ProblemDetails { "Title": ["..."], "Steps": ["..."] }
  readonly serverErrors = signal<Record<string, string[]>>({});

  recipeForm!: FormGroup;
  isEditMode = signal<boolean>(false);
  recipeId = signal<number | null>(null);
  cuisines = signal<Cuisine[]>([]);
  submitting = signal<boolean>(false);

  selectedFile: File | null = null;
  imagePreviewUrl = signal<string | null>(null);
  existingImageUrl = signal<string | null>(null);

  // Реактивный счетчик символов шагов
  readonly stepsLength = signal<number>(0);
  readonly cookingTime = signal<number>(0);
  readonly isSpecialRuleActive = computed(() => this.cookingTime() > 240);

  ngOnInit(): void {
    this.initForm();
    this.loadCuisines();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode.set(true);
      const id = Number(idParam);
      this.recipeId.set(id);
      this.loadRecipeForEdit(id);
    }
  }

  private initForm(): void {
    this.recipeForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      cuisineId: [null, [Validators.required, Validators.min(1)]],
      cookingTimeMinutes: [null, [Validators.required, Validators.min(1), Validators.max(10000)]],
      servings: [null, [Validators.required, Validators.min(1), Validators.max(100)]],
      ingredients: ['', [Validators.required]],
      steps: ['', [Validators.required]]
    }, {
      validators: [specialCookingTimeRuleValidator]
    });

    this.recipeForm.get('steps')?.valueChanges.subscribe(val => {
      this.stepsLength.set((val || '').trim().length);
      this.clearFieldError('Steps');
      this.clearFieldError('steps');
    });

    this.recipeForm.get('cookingTimeMinutes')?.valueChanges.subscribe(val => {
      this.cookingTime.set(Number(val) || 0);
      this.clearFieldError('CookingTimeMinutes');
      this.clearFieldError('cookingTimeMinutes');
    });
  }

  loadCuisines(): void {
    this.cuisineService.getCuisines().subscribe({
      next: (data) => this.cuisines.set(data),
      error: (err) => console.error('Ошибка загрузки кухонь:', err)
    });
  }

  loadRecipeForEdit(id: number): void {
    this.state.set('loading');
    this.recipeService.getRecipe(id).subscribe({
      next: (recipe: Recipe) => {
        // Проверка: автор ли текущий пользователь
        if (!this.authService.isAuthor(recipe.authorId)) {
          alert('Отказано в доступе (403): редактировать рецепт может только его автор.');
          this.router.navigate(['/recipes', id]);
          return;
        }

        this.recipeForm.patchValue({
          title: recipe.title,
          cuisineId: recipe.cuisineId,
          cookingTimeMinutes: recipe.cookingTimeMinutes,
          servings: recipe.servings,
          ingredients: recipe.ingredients,
          steps: recipe.steps
        });

        this.stepsLength.set((recipe.steps || '').trim().length);
        this.cookingTime.set(recipe.cookingTimeMinutes);
        this.existingImageUrl.set(recipe.imageUrl || null);
        this.state.set('data');
      },
      error: (err) => {
        if (err.status === 404) {
          this.state.set('empty');
        } else {
          this.generalError.set('Не удалось загрузить данные рецепта для редактирования.');
          this.state.set('error');
        }
      }
    });
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];

      // Валидация файла до 5 МБ и расширений
      const allowed = ['image/jpeg', 'image/png', 'image/webp'];
      if (!allowed.includes(file.type)) {
        alert('Разрешены только форматы JPG, PNG, WEBP.');
        input.value = '';
        return;
      }
      if (file.size > 5 * 1024 * 1024) {
        alert('Файл превышает лимит в 5 МБ.');
        input.value = '';
        return;
      }

      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewUrl.set(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  removeSelectedFile(): void {
    this.selectedFile = null;
    this.imagePreviewUrl.set(null);
  }

  // Получение ошибки для конкретного поля (клиентской или серверной ProblemDetails)
  getFieldError(fieldName: string): string | null {
    const control = this.recipeForm.get(fieldName);

    // 1. Клиентская валидация
    if (control && control.touched && control.errors) {
      if (control.errors['required']) return 'Это поле обязательно для заполнения.';
      if (control.errors['maxlength']) return `Максимальная длина: ${control.errors['maxlength'].requiredLength} символов.`;
      if (control.errors['min']) return `Минимальное значение: ${control.errors['min'].min}.`;
      if (control.errors['max']) return `Максимальное значение: ${control.errors['max'].max}.`;
    }

    // Особое бизнес-правило
    if (fieldName === 'steps' && this.recipeForm.hasError('specialRuleViolation')) {
      const info = this.recipeForm.getError('specialRuleViolation');
      return `Особое правило: при времени более 240 минут шаги не короче 200 символов (сейчас: ${info.actualLength} / 200).`;
    }

    // 2. Серверная ошибка ProblemDetails
    const errors = this.serverErrors();
    const capitalized = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);
    if (errors[capitalized] && errors[capitalized].length > 0) {
      return errors[capitalized][0];
    }
    if (errors[fieldName] && errors[fieldName].length > 0) {
      return errors[fieldName][0];
    }

    return null;
  }

  clearFieldError(fieldName: string): void {
    const errs = { ...this.serverErrors() };
    const capitalized = fieldName.charAt(0).toUpperCase() + fieldName.slice(1);
    delete errs[capitalized];
    delete errs[fieldName];
    this.serverErrors.set(errs);
  }

  onSubmit(): void {
    this.serverErrors.set({});
    this.generalError.set(null);

    if (this.recipeForm.invalid) {
      this.recipeForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const formValue = this.recipeForm.value;
    const dto = {
      title: formValue.title,
      cookingTimeMinutes: Number(formValue.cookingTimeMinutes),
      servings: Number(formValue.servings),
      ingredients: formValue.ingredients,
      steps: formValue.steps,
      cuisineId: Number(formValue.cuisineId)
    };

    if (this.isEditMode() && this.recipeId()) {
      const id = this.recipeId()!;
      this.recipeService.updateRecipe(id, dto).subscribe({
        next: (updated) => {
          if (this.selectedFile) {
            this.uploadFileAndNavigate(id);
          } else {
            this.router.navigate(['/recipes', id]);
          }
        },
        error: (err) => this.handleServerError(err)
      });
    } else {
      this.recipeService.createRecipe(dto).subscribe({
        next: (created) => {
          if (this.selectedFile) {
            this.uploadFileAndNavigate(created.id);
          } else {
            this.router.navigate(['/recipes', created.id]);
          }
        },
        error: (err) => this.handleServerError(err)
      });
    }
  }

  private uploadFileAndNavigate(recipeId: number): void {
    if (!this.selectedFile) {
      this.router.navigate(['/recipes', recipeId]);
      return;
    }
    this.recipeService.uploadImage(recipeId, this.selectedFile).subscribe({
      next: () => this.router.navigate(['/recipes', recipeId]),
      error: (err) => {
        console.error('Ошибка загрузки фото:', err);
        this.router.navigate(['/recipes', recipeId]);
      }
    });
  }

  private handleServerError(err: any): void {
    this.submitting.set(false);
    if (err.status === 400 && err.error?.errors) {
      // ProblemDetails с объектом errors
      this.serverErrors.set(err.error.errors);
      this.generalError.set('Пожалуйста, исправьте ошибки валидации.');
    } else if (err.status === 401) {
      this.generalError.set('Ошибка авторизации (401). Пожалуйста, войдите снова.');
    } else if (err.status === 403) {
      this.generalError.set('Отказано в доступе (403): редактировать рецепт может только его автор.');
    } else {
      this.generalError.set(err.error?.detail || err.error?.title || 'Произошла непредвиденная ошибка на сервере.');
    }
  }
}
