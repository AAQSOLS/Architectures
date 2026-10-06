/**
 * Represents a validation error item from ASP.NET Core FluentValidation or Domain Result.
 */
export interface ValidationErrorItem {
  propertyName?: string;
  errorMessage?: string;
  code?: string;
  description?: string;
  type?: number;
}

/**
 * Standard RFC 7807 / RFC 9457 Problem Details payload emitted by ASP.NET Core backend middleware.
 */
export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance?: string;
  errors?: ValidationErrorItem[];
  [key: string]: unknown;
}

/**
 * Normalized application error consumable by UI components and forms.
 */
export interface AppNormalizedError {
  status: number;
  title: string;
  message: string;
  fieldErrors: Record<string, string>;
  isValidationError: boolean;
  raw: ProblemDetails | unknown;
}
