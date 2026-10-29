import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { PagedResult, ProductListItem } from '../../core/api/api.models';
import { ProductCatalog } from './product-catalog';

describe('ProductCatalog', () => {
  let fixture: ComponentFixture<ProductCatalog>;
  let httpMock: HttpTestingController;

  const item = (sku: string, price: number, isActive = true): ProductListItem => ({
    id: crypto.randomUUID(),
    sku,
    name: `Товар ${sku}`,
    categoryId: 'cat-1',
    categoryName: 'Категория',
    price,
    isActive,
    createdAtUtc: '2026-01-01T00:00:00Z',
    updatedAtUtc: null,
  });

  const paged = (items: ProductListItem[]): PagedResult<ProductListItem> => ({
    items,
    page: 1,
    pageSize: 20,
    totalCount: items.length,
    totalPages: 1,
    hasPrevious: false,
    hasNext: false,
  });

  const pendingProducts = () =>
    httpMock.match((r) => r.urlWithParams.includes('/api/products'))[0];
  const flushCategories = () =>
    httpMock.match((r) => r.urlWithParams.includes('/api/categories')).forEach((r) => r.flush([]));

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductCatalog],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductCatalog);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function initialLoad() {
    fixture.detectChanges();
    flushCategories();
    pendingProducts().flush(paged([item('SKU-1', 10), item('SKU-2', 20)]));
    fixture.detectChanges();
  }

  it('loads catalog with server-side page params', () => {
    fixture.detectChanges();
    flushCategories();

    const req = pendingProducts();
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('20');
    expect(req.request.params.get('sortBy')).toBe('createdAt');
    expect(req.request.params.get('sortDirection')).toBe('desc');
    req.flush(paged([]));
  });

  it('renders rows and pagination info after load', () => {
    initialLoad();

    const rows = fixture.nativeElement.querySelectorAll('table.grid tbody tr');
    expect(rows.length).toBe(2);
    expect(fixture.nativeElement.querySelector('.pager span').textContent).toContain('всего 2');
  });

  it('shows empty state when no results', () => {
    fixture.detectChanges();
    flushCategories();
    pendingProducts().flush(paged([]));
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.state-empty')).toBeTruthy();
  });

  it('shows error state when API fails', () => {
    initialLoad();

    fixture.componentInstance['onActiveChange']('true');
    pendingProducts().flush(null, { status: 503, statusText: 'Unavailable' });
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.state-error')).toBeTruthy();
  });

  it('debounces search and passes search param on page 1', () => {
    initialLoad();

    vi.useFakeTimers();
    try {
      fixture.componentInstance['searchControl'].setValue('ноут');
      vi.advanceTimersByTime(350);

      const req = pendingProducts();
      expect(req.request.params.get('search')).toBe('ноут');
      expect(req.request.params.get('page')).toBe('1');
      req.flush(paged([]));
    } finally {
      vi.useRealTimers();
    }
  });

  it('requests sort change on header click', () => {
    initialLoad();

    const buttons = Array.from<HTMLElement>(
      fixture.nativeElement.querySelectorAll('th button.sort'),
    );
    const priceButton = buttons.find((b) => b.textContent?.includes('Цена'))!;
    priceButton.click();

    const req = pendingProducts();
    expect(req.request.params.get('sortBy')).toBe('price');
    expect(req.request.params.get('sortDirection')).toBe('asc');
    req.flush(paged([]));
  });
});
