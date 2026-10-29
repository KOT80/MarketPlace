import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { CatalogApiService } from '../../core/api/catalog-api.service';
import { AppError, Category } from '../../core/api/api.models';
import { normalizedSkuRequired, validPrice } from './product-validators';

@Component({
  selector: 'app-product-form',
  templateUrl: './product-form.html',
  styleUrl: './product-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
})
export class ProductForm implements OnInit {
  private readonly api = inject(CatalogApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  protected readonly editMode = signal(false);
  protected readonly loading = signal(true);
  protected readonly submitting = signal(false);
  protected readonly loadError = signal<AppError | null>(null);
  protected readonly submitError = signal<AppError | null>(null);
  protected readonly categories = signal<Category[]>([]);

  protected readonly form = this.fb.group({
    sku: this.fb.nonNullable.control('', [
      Validators.required,
      Validators.maxLength(64),
      normalizedSkuRequired,
    ]),
    name: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(200)]),
    description: this.fb.control<string | null>(null, [Validators.maxLength(4000)]),
    categoryId: this.fb.nonNullable.control('', [Validators.required]),
    price: this.fb.nonNullable.control('0', [Validators.required, validPrice]),
  });

  async ngOnInit(): Promise<void> {
    this.api.getCategories().subscribe({
      next: (categories) => this.categories.set(categories),
      error: (err) => this.loadError.set(CatalogApiService.toAppError(err)),
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      return;
    }

    this.editMode.set(true);
    this.form.controls.price.disable();

    try {
      const product = await firstValueFrom(this.api.getProduct(id));
      this.form.patchValue({
        sku: product.sku,
        name: product.name,
        description: product.description,
        categoryId: product.categoryId,
        price: String(product.price),
      });
    } catch (err) {
      this.loadError.set(CatalogApiService.toAppError(err));
    } finally {
      this.loading.set(false);
    }
  }

  protected async onSubmit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.submitError.set(null);

    const value = this.form.getRawValue();
    const id = this.route.snapshot.paramMap.get('id');

    try {
      const saved =
        this.editMode() && id
          ? await firstValueFrom(
              this.api.updateProduct(id, {
                sku: value.sku,
                name: value.name,
                description: value.description,
                categoryId: value.categoryId,
              }),
            )
          : await firstValueFrom(
              this.api.createProduct({
                sku: value.sku,
                name: value.name,
                description: value.description,
                categoryId: value.categoryId,
                price: Number(value.price),
              }),
            );

      await this.router.navigate(['/products', saved.id, 'history']);
    } catch (err) {
      const appError = CatalogApiService.toAppError(err);
      this.submitError.set(appError);
      this.applyFieldErrors(appError);
      this.submitting.set(false);
    }
  }

  protected fieldErrorMessages(controlName: string): string[] {
    const errors = this.submitError()?.fieldErrors ?? {};
    const mapped = CatalogApiService.mapFieldErrors(errors);
    return mapped[controlName] ?? [];
  }

  private applyFieldErrors(error: AppError): void {
    const mapped = CatalogApiService.mapFieldErrors(error.fieldErrors);
    for (const controlName of ['sku', 'name', 'description', 'categoryId', 'price']) {
      const control = this.form.get(controlName);
      if (control && mapped[controlName]?.length) {
        control.setErrors({ ...(control.errors ?? {}), server: mapped[controlName] });
      }
    }
  }
}
