import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  BehaviorSubject,
  catchError,
  debounceTime,
  distinctUntilChanged,
  of,
  Subject,
  switchMap,
  takeUntil,
  tap,
} from 'rxjs';

import { CatalogApiService } from '../../core/api/catalog-api.service';
import {
  AppError,
  Category,
  PagedResult,
  ProductListItem,
  ProductQueryParams,
  SortDirection,
  SortField,
} from '../../core/api/api.models';

type ViewState = 'loading' | 'ready' | 'error';

@Component({
  selector: 'app-product-catalog',
  templateUrl: './product-catalog.html',
  styleUrl: './product-catalog.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, ReactiveFormsModule, DatePipe, DecimalPipe],
})
export class ProductCatalog implements OnInit, OnDestroy {
  private readonly api = inject(CatalogApiService);
  private readonly destroy$ = new Subject<void>();

  protected readonly categories = signal<Category[]>([]);
  protected readonly result = signal<PagedResult<ProductListItem>>({
    items: [],
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0,
    hasPrevious: false,
    hasNext: false,
  });
  protected readonly viewState = signal<ViewState>('loading');
  protected readonly appError = signal<AppError | null>(null);
  protected readonly actionError = signal<string | null>(null);
  protected readonly busyItemId = signal<string | null>(null);
  protected readonly sortField = signal<SortField>('createdAt');
  protected readonly sortDirection = signal<SortDirection>('desc');
  protected readonly page = signal(1);
  protected readonly pageSize = signal(20);

  protected readonly searchControl = new FormControl('', { nonNullable: true });

  private categoryValue: string | null = null;
  private activeValue: boolean | null = null;

  private readonly query$ = new BehaviorSubject<ProductQueryParams>({
    page: 1,
    pageSize: 20,
    sortBy: 'createdAt',
    sortDirection: 'desc',
  });

  ngOnInit(): void {
    this.api.getCategories().subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => this.categories.set([]),
    });

    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.page.set(1);
        this.emitQuery();
      });

    this.query$
      .pipe(
        switchMap((params) => {
          this.viewState.set('loading');
          this.actionError.set(null);
          return this.api.getProducts(params).pipe(
            tap((result) => {
              this.result.set(result);
              this.appError.set(null);
              this.viewState.set('ready');
            }),
            catchError((err) => {
              this.appError.set(CatalogApiService.toAppError(err));
              this.viewState.set('error');
              return of(null);
            }),
          );
        }),
        takeUntil(this.destroy$),
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  protected onCategoryChange(value: string): void {
    this.categoryValue = value || null;
    this.page.set(1);
    this.emitQuery();
  }

  protected onActiveChange(value: string): void {
    this.activeValue = value === '' ? null : value === 'true';
    this.page.set(1);
    this.emitQuery();
  }

  protected sortBy(field: SortField): void {
    if (this.sortField() === field) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortField.set(field);
      this.sortDirection.set(field === 'createdAt' ? 'desc' : 'asc');
    }
    this.page.set(1);
    this.emitQuery();
  }

  protected sortIndicator(field: SortField): string {
    if (this.sortField() !== field) {
      return '↕';
    }
    return this.sortDirection() === 'asc' ? '↑' : '↓';
  }

  protected changePage(delta: number): void {
    const target = Math.min(Math.max(1, this.page() + delta), Math.max(1, this.result().totalPages));
    if (target !== this.page()) {
      this.page.set(target);
      this.emitQuery();
    }
  }

  protected onPageSizeChange(value: string): void {
    this.pageSize.set(Number(value));
    this.page.set(1);
    this.emitQuery();
  }

  protected toggleArchive(item: ProductListItem): void {
    this.busyItemId.set(item.id);
    const request = item.isActive ? this.api.archive(item.id) : this.api.restore(item.id);
    request.subscribe({
      next: () => {
        this.busyItemId.set(null);
        this.emitQuery();
      },
      error: (err) => {
        this.busyItemId.set(null);
        this.actionError.set(CatalogApiService.toAppError(err).detail);
      },
    });
  }

  protected retry(): void {
    this.emitQuery();
  }

  protected readonly trackById = (_: number, item: ProductListItem): string => item.id;

  private emitQuery(): void {
    this.query$.next({
      search: this.searchControl.value?.trim() || undefined,
      categoryId: this.categoryValue,
      isActive: this.activeValue,
      page: this.page(),
      pageSize: this.pageSize(),
      sortBy: this.sortField(),
      sortDirection: this.sortDirection(),
    });
  }
}
