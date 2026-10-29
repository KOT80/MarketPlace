import { DatePipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin, of, switchMap, tap, catchError, startWith, Subject, firstValueFrom } from 'rxjs';

import { CatalogApiService } from '../../core/api/catalog-api.service';
import { AppError, PriceHistoryEntry, ProductDto } from '../../core/api/api.models';
import { validPrice } from '../product-form/product-validators';

type ViewState = 'loading' | 'ready' | 'error';

@Component({
  selector: 'app-price-history',
  templateUrl: './price-history.html',
  styleUrl: './price-history.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink, DatePipe, DecimalPipe],
})
export class PriceHistory implements OnInit {
  private readonly api = inject(CatalogApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  protected readonly product = signal<ProductDto | null>(null);
  protected readonly history = signal<PriceHistoryEntry[]>([]);
  protected readonly viewState = signal<ViewState>('loading');
  protected readonly appError = signal<AppError | null>(null);
  protected readonly priceError = signal<AppError | null>(null);
  protected readonly changedMessage = signal<string | null>(null);
  protected readonly submitting = signal(false);

  protected readonly productId = this.route.snapshot.paramMap.get('id') ?? '';

  protected readonly priceForm = this.fb.group({
    price: this.fb.nonNullable.control('', [Validators.required, validPrice]),
  });

  private readonly reload$ = new Subject<void>();

  ngOnInit(): void {
    this.reload$
      .pipe(
        startWith(undefined),
        switchMap(() =>
          forkJoin({
            product: this.api.getProduct(this.productId),
            history: this.api.getPriceHistory(this.productId),
          }),
        ),
        tap(({ product, history }) => {
          this.product.set(product);
          this.history.set(history);
          this.appError.set(null);
          this.viewState.set('ready');
        }),
        catchError((err) => {
          this.appError.set(CatalogApiService.toAppError(err));
          this.viewState.set('error');
          return of(null);
        }),
      )
      .subscribe();
  }

  protected async onChangePrice(): Promise<void> {
    if (this.priceForm.invalid) {
      this.priceForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.priceError.set(null);
    this.changedMessage.set(null);
    const price = Number(this.priceForm.getRawValue().price);

    try {
      const updated = await firstValueFrom(this.api.changePrice(this.productId, price));
      this.reload$.next();
      this.priceForm.patchValue({ price: '' });
      this.priceForm.markAsPristine();
      this.changedMessage.set(`Цена обновлена: ${updated.price.toFixed(2)}`);
    } catch (err) {
      const appError = CatalogApiService.toAppError(err);
      this.priceError.set(appError);
      if (appError.status === 409) {
        this.reload$.next();
      }
    } finally {
      this.submitting.set(false);
    }
  }

  protected reload(): void {
    this.reload$.next();
  }

  protected entryTrack(_: number, entry: PriceHistoryEntry): string {
    return entry.id;
  }
}
