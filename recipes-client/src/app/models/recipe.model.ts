export interface Recipe {
  id: number;
  title: string;
  cookingTimeMinutes: number;
  servings: number;
  ingredients: string;
  steps: string;
  cuisineId: number;
  cuisineName: string;
  imageUrl?: string;
  createdAt: string;
  authorId: number;
  authorUsername: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface RecipeFilterParams {
  page?: number;
  pageSize?: number;
  cuisineId?: number;
  maxTime?: number;
  search?: string;
  sortBy?: 'newest' | 'time_asc' | 'time_desc' | 'oldest';
}

export interface ValidationProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
