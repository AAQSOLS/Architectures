import { HttpParams } from '@angular/common/http';

/**
 * Converts a plain JavaScript object/dictionary into clean Angular HttpParams.
 * - Strips null, undefined, and empty string properties.
 * - Formats Date instances into ISO UTC strings.
 * - Appends arrays with repeated parameter keys (e.g. `status=1&status=2`) for ASP.NET Core binding.
 */
export function buildHttpParams(filters: Record<string, unknown>): HttpParams {
  let params = new HttpParams();

  for (const [key, value] of Object.entries(filters)) {
    if (value === null || value === undefined || value === '') {
      continue;
    }

    if (value instanceof Date) {
      params = params.set(key, value.toISOString());
    } else if (Array.isArray(value)) {
      for (const item of value) {
        if (item !== null && item !== undefined && item !== '') {
          params = params.append(key, String(item));
        }
      }
    } else {
      params = params.set(key, String(value));
    }
  }

  return params;
}

/**
 * Serializes an object to a URL query string without Angular HttpParams dependency.
 * Useful for building window.open or download URLs.
 * Example: { page: 1, search: 'Gulberg' } -> "page=1&search=Gulberg"
 */
export function buildQueryString(params: Record<string, unknown>): string {
  const parts: string[] = [];

  for (const [key, value] of Object.entries(params)) {
    if (value === null || value === undefined || value === '') {
      continue;
    }

    if (Array.isArray(value)) {
      for (const item of value) {
        if (item !== null && item !== undefined && item !== '') {
          parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(String(item))}`);
        }
      }
    } else if (value instanceof Date) {
      parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(value.toISOString())}`);
    } else {
      parts.push(`${encodeURIComponent(key)}=${encodeURIComponent(String(value))}`);
    }
  }

  return parts.join('&');
}
