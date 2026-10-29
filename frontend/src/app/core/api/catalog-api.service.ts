import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  AppError,
  Category,
  CreateProductInput,
  PagedResult,
  PriceHistoryEntry,
  ProductDto,
  ProductListItem,
  ProductQueryParams,
  UpdateProductInput,
} from './api.models';

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api';

  getProducts(params: ProductQueryParams): Observable<PagedResult<ProductListItem>> {
    let httpParams = new HttpParams()
      .set('page', params.page)
      .set('pageSize', params.pageSize)
      .set('sortBy', params.sortBy)
      .set('sortDirection', params.sortDirection);

    if (params.search) {
      httpParams = httpParams.set('search', params.search);
    }
    if (params.categoryId) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }
    if (params.isActive !== null && params.isActive !== undefined) {
      httpParams = httpParams.set('isActive', params.isActive);
    }

    return this.http.get<PagedResult<ProductListItem>>(`${this.base}/products`, { params: httpParams });
  }

  getProduct(id: string): Observable<ProductDto> {
    return this.http.get<ProductDto>(`${this.base}/products/${id}`);
  }

  createProduct(input: CreateProductInput): Observable<ProductDto> {
    return this.http.post<ProductDto>(`${this.base}/products`, input);
  }

  updateProduct(id: string, input: UpdateProductInput): Observable<ProductDto> {
    return this.http.put<ProductDto>(`${this.base}/products/${id}`, input);
  }

  changePrice(id: string, price: number): Observable<ProductDto> {
    return this.http.patch<ProductDto>(`${this.base}/products/${id}/price`, { price });
  }

  archive(id: string): Observable<ProductDto> {
    return this.http.post<ProductDto>(`${this.base}/products/${id}/archive`, null);
  }

  restore(id: string): Observable<ProductDto> {
    return this.http.post<ProductDto>(`${this.base}/products/${id}/restore`, null);
  }

  getPriceHistory(id: string): Observable<PriceHistoryEntry[]> {
    return this.http.get<PriceHistoryEntry[]>(`${this.base}/products/${id}/price-history`);
  }

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.base}/categories`);
  }

  static toAppError(err: unknown): AppError {
    if (err instanceof HttpErrorResponse) {
      if (err.status === 0) {
        return {
          status: 0,
          title: 'Сервер недоступен',
          detail: 'Не удалось выполнить запрос к API. Проверьте, что backend запущен.',
          fieldErrors: {},
          isNetwork: true,
        };
      }

      const body = err.error as Record<string, unknown> | null;
      const errors = (body?.['errors'] ?? {}) as Record<string, string[]>;
      return {
        status: err.status,
        title: typeof body?.['title'] === 'string' ? (body['title'] as string) : err.statusText,
        detail: typeof body?.['detail'] === 'string' ? (body['detail'] as string) : err.message,
        fieldErrors: errors,
        isNetwork: false,
      };
    }

    return {
      status: -1,
      title: 'Неизвестная ошибка',
      detail: err instanceof Error ? err.message : String(err),
      fieldErrors: {},
      isNetwork: false,
    };
  }

  static mapFieldErrors(errors: Record<string, string[]>): Record<string, string[]> {
    const result: Record<string, string[]> = {};
    for (const [key, messages] of Object.entries(errors)) {
      const last = key.split('.').pop() ?? key;
      const field = last.charAt(0).toLowerCase() + last.slice(1);
      result[field] = [...(result[field] ?? []), ...messages];
    }
    return result;
  }
}
