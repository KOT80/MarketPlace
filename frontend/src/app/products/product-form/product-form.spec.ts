import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { CatalogApiService } from '../../core/api/catalog-api.service';
import { Category, ProductDto } from '../../core/api/api.models';
import { ProductForm } from './product-form';

describe('ProductForm', () => {
  let fixture: ComponentFixture<ProductForm>;
  let api: {
    getCategories: ReturnType<typeof vi.fn>;
    getProduct: ReturnType<typeof vi.fn>;
    createProduct: ReturnType<typeof vi.fn>;
    updateProduct: ReturnType<typeof vi.fn>;
  };
  let navigateSpy: ReturnType<typeof vi.fn>;

  const product = (overrides: Partial<ProductDto> = {}): ProductDto => ({
    id: 'p-1',
    sku: 'SKU-1',
    name: 'Товар',
    description: null,
    categoryId: 'c-1',
    categoryName: 'Категория',
    price: 10,
    isActive: true,
    createdAtUtc: '2026-01-01T00:00:00Z',
    updatedAtUtc: null,
    ...overrides,
  });

  function setup(productId: string | null) {
    api = {
      getCategories: vi.fn().mockReturnValue(of<Category[]>([])),
      getProduct: vi.fn().mockReturnValue(of(product())),
      createProduct: vi.fn().mockReturnValue(of(product())),
      updateProduct: vi.fn().mockReturnValue(of(product())),
    };

    TestBed.configureTestingModule({
      imports: [ProductForm],
      providers: [
        provideRouter([]),
        { provide: CatalogApiService, useValue: api },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => productId } } } },
      ],
    });

    fixture = TestBed.createComponent(ProductForm);
    navigateSpy = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture.detectChanges();
  }

  it('create mode: valid form submits POST and navigates to history', async () => {
    setup(null);
    await fixture.whenStable();

    const form = fixture.componentInstance['form'];
    form.patchValue({ sku: 'NEW-1', name: 'Новый', categoryId: 'c-1', price: '12.34' });
    await fixture.componentInstance['onSubmit']();

    expect(api.createProduct).toHaveBeenCalledTimes(1);
    expect(api.createProduct.mock.calls[0][0]).toMatchObject({ sku: 'NEW-1', price: 12.34 });
    expect(navigateSpy).toHaveBeenCalledWith(['/products', 'p-1', 'history']);
  });

  it('create mode: invalid fields block submit', async () => {
    setup(null);
    const form = fixture.componentInstance['form'];
    form.patchValue({ sku: '', name: 'Новый', categoryId: 'c-1', price: '12.34' });

    await fixture.componentInstance['onSubmit']();

    expect(api.createProduct).not.toHaveBeenCalled();
    expect(form.controls.sku.invalid).toBe(true);
  });

  it('create mode: rejects price with 3 decimals', () => {
    setup(null);
    const form = fixture.componentInstance['form'];
    form.patchValue({ sku: 'A', name: 'B', categoryId: 'c-1', price: '1.234' });

    expect(form.controls.price.invalid).toBe(true);
  });

  it('create mode: rejects negative price', () => {
    setup(null);
    const form = fixture.componentInstance['form'];
    form.patchValue({ sku: 'A', name: 'B', categoryId: 'c-1', price: '-5' });

    expect(form.controls.price.invalid).toBe(true);
  });

  it('edit mode: loads product, price disabled, submit calls PUT', async () => {
    setup('p-1');
    await fixture.whenStable();
    fixture.detectChanges();

    expect(api.getProduct).toHaveBeenCalledWith('p-1');
    const form = fixture.componentInstance['form'];
    expect(form.controls.sku.value).toBe('SKU-1');
    expect(form.controls.price.disabled).toBe(true);

    form.patchValue({ name: 'Обновлённый' });
    await fixture.componentInstance['onSubmit']();

    expect(api.updateProduct).toHaveBeenCalledTimes(1);
    expect(api.updateProduct.mock.calls[0][1].name).toBe('Обновлённый');
    expect(api.createProduct).not.toHaveBeenCalled();
  });

  it('maps server validation errors onto the sku field', async () => {
    setup(null);
    api.createProduct.mockReturnValue(
      throwError(() => new HttpErrorResponse({
        status: 400,
        error: {
          title: 'Проверка входных данных не пройдена',
          detail: '',
          errors: { 'Input.Sku': ['Артикул уже существует.'] },
        },
      })),
    );

    const form = fixture.componentInstance['form'];
    form.patchValue({ sku: 'DUP', name: 'B', categoryId: 'c-1', price: '1' });
    await fixture.componentInstance['onSubmit']();
    form.controls.sku.markAsTouched();
    fixture.detectChanges();

    expect(form.controls.sku.errors?.['server']).toEqual(['Артикул уже существует.']);
    const text: string = fixture.nativeElement.textContent;
    expect(text).toContain('Артикул уже существует.');
  });

  it('shows conflict banner on 409', async () => {
    setup(null);
    api.createProduct.mockReturnValue(
      throwError(() => new HttpErrorResponse({
        status: 409,
        error: { title: 'Конфликт данных', detail: 'Артикул занят.' },
      })),
    );

    const form = fixture.componentInstance['form'];
    form.patchValue({ sku: 'DUP', name: 'B', categoryId: 'c-1', price: '1' });
    await fixture.componentInstance['onSubmit']();
    fixture.detectChanges();

    const banner = fixture.nativeElement.querySelector('.banner-error');
    expect(banner).toBeTruthy();
    expect(banner.textContent).toContain('Конфликт данных');
    expect(navigateSpy).not.toHaveBeenCalled();
  });
});
