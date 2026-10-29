import { AbstractControl, ValidationErrors } from '@angular/forms';

export function normalizedSkuRequired(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string | null;
  return value && value.trim().length > 0 ? null : { normalizedSkuRequired: true };
}

export function validPrice(control: AbstractControl): ValidationErrors | null {
  const value = control.value as string;
  if (!/^\d+(\.\d{1,2})?$/.test(value)) {
    return { validPrice: true };
  }
  return Number(value) <= 999_999_999_999.99 ? null : { validPrice: true };
}
