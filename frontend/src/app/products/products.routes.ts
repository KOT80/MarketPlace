import { Routes } from '@angular/router';

export const PRODUCTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./catalog/product-catalog').then((m) => m.ProductCatalog),
  },
  {
    path: 'new',
    loadComponent: () => import('./product-form/product-form').then((m) => m.ProductForm),
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./product-form/product-form').then((m) => m.ProductForm),
  },
  {
    path: ':id/history',
    loadComponent: () => import('./price-history/price-history').then((m) => m.PriceHistory),
  },
];
