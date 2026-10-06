import { FormGroup } from '@angular/forms';
import { AppNormalizedError } from '../../core/http/models/problem-details.model';

/**
 * Maps backend RFC 7807 validation errors to Angular FormGroup controls automatically.
 * Handles case differences (e.g. PascalCase "Email" from C# mapped to camelCase "email" in Angular).
 *
 * @param form The target FormGroup
 * @param error The normalized error from error.interceptor.ts
 * @returns An array of error messages that could not be matched to any form control
 */
export function applyServerValidationErrors(
  form: FormGroup,
  error: AppNormalizedError
): string[] {
  if (!error.isValidationError || !error.fieldErrors) {
    return [error.message];
  }

  const unmappedErrors: string[] = [];
  const controls = form.controls;

  // Build a lookup map of lowercased control names
  const controlLookup = new Map<string, string>();
  for (const controlName of Object.keys(controls)) {
    controlLookup.set(controlName.toLowerCase(), controlName);
  }

  for (const [field, errorMessage] of Object.entries(error.fieldErrors)) {
    const matchedControlName = controlLookup.get(field.toLowerCase());

    if (matchedControlName) {
      const targetControl = form.get(matchedControlName);
      if (targetControl) {
        targetControl.setErrors({
          ...(targetControl.errors || {}),
          serverError: errorMessage
        });
        targetControl.markAsTouched();
        targetControl.markAsDirty();
      }
    } else {
      unmappedErrors.push(`${field}: ${errorMessage}`);
    }
  }

  return unmappedErrors;
}
