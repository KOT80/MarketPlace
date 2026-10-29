export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface ProductListItem {
  id: string;
  sku: string;
  name: string;
  categoryId: string;
  categoryName: string;
  price: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface ProductDto {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  categoryId: string;
  categoryName: string;
  price: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface CreateProductInput {
  sku: string;
  name: string;
  description: string | null;
  categoryId: string;
  price: number;
}

export interface UpdateProductInput {
  sku: string;
  name: string;
  description: string | null;
  categoryId: string;
}

export interface PriceHistoryEntry {
  id: string;
  oldPrice: number;
  newPrice: number;
  changedAtUtc: string;
}

export interface Category {
  id: string;
  name: string;
  createdAtUtc: string;
}

export type SortField = 'name' | 'sku' | 'price' | 'createdAt';
export type SortDirection = 'asc' | 'desc';

export interface ProductQueryParams {
  search?: string;
  categoryId?: string | null;
  isActive?: boolean | null;
  page: number;
  pageSize: number;
  sortBy: SortField;
  sortDirection: SortDirection;
}

export interface AppError {
  status: number;
  title: string;
  detail: string;
  fieldErrors: Record<string, string[]>;
  isNetwork: boolean;
}
