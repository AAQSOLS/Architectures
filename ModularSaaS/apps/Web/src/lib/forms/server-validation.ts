import { ProblemDetails, HttpError } from "@/lib/http/models/problem-details";

export interface FieldValidationErrors {
  [fieldName: string]: string[];
}

/**
 * Normalizes field names from backend PascalCase (e.g., 'UserEmail') to camelCase ('userEmail').
 */
function toCamelCaseKey(key: string): string {
  if (!key) return "";
  return key.charAt(0).toLowerCase() + key.slice(1);
}

/**
 * Extracts and maps RFC 7807 problem details errors into a normalized field error dictionary.
 */
export function extractServerValidationErrors(
  error: unknown
): FieldValidationErrors {
  if (!error) return {};

  let problem: ProblemDetails | undefined;

  if (error instanceof HttpError) {
    problem = error.problemDetails;
  } else if (
    typeof error === "object" &&
    error !== null &&
    "errors" in error &&
    typeof (error as ProblemDetails).errors === "object"
  ) {
    problem = error as ProblemDetails;
  }

  if (!problem?.errors) {
    return {};
  }

  const result: FieldValidationErrors = {};

  Object.entries(problem.errors).forEach(([field, messages]) => {
    const camelKey = toCamelCaseKey(field);
    result[camelKey] = Array.isArray(messages) ? messages : [String(messages)];
  });

  return result;
}

/**
 * Retrieves the first error message for a given form field.
 */
export function getFieldError(
  errors: FieldValidationErrors,
  fieldName: string
): string | undefined {
  const camelKey = toCamelCaseKey(fieldName);
  const fieldErrors = errors[camelKey] ?? errors[fieldName];
  return fieldErrors?.[0];
}

/**
 * Checks whether any validation errors exist.
 */
export function hasValidationErrors(errors: FieldValidationErrors): boolean {
  return Object.keys(errors).length > 0;
}
