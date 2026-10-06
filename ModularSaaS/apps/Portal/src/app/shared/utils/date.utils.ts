/**
 * Date and time manipulation utilities.
 */

/**
 * Returns a human-friendly relative time string ("just now", "5m ago", "2h ago", "yesterday", "5d ago").
 */
export function getRelativeTime(dateInput: string | Date | null | undefined): string {
  if (!dateInput) return '-';
  const date = typeof dateInput === 'string' ? new Date(dateInput) : dateInput;
  if (isNaN(date.getTime())) return '-';

  const now = new Date();
  const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

  if (diffInSeconds < 30) return 'just now';
  if (diffInSeconds < 60) return `${diffInSeconds}s ago`;

  const diffInMinutes = Math.floor(diffInSeconds / 60);
  if (diffInMinutes < 60) return `${diffInMinutes}m ago`;

  const diffInHours = Math.floor(diffInMinutes / 60);
  if (diffInHours < 24) return `${diffInHours}h ago`;

  const diffInDays = Math.floor(diffInHours / 24);
  if (diffInDays === 1) return 'yesterday';
  if (diffInDays < 30) return `${diffInDays}d ago`;

  return date.toLocaleDateString();
}

/**
 * Ensures a date is formatted to a standard UTC ISO 8601 string.
 */
export function toUtcIsoString(dateInput: string | Date | null | undefined): string | null {
  if (!dateInput) return null;
  const date = typeof dateInput === 'string' ? new Date(dateInput) : dateInput;
  if (isNaN(date.getTime())) return null;
  return date.toISOString();
}

/**
 * Formats a date using Intl.DateTimeFormat with safe fallback.
 * Example: formatDate('2026-10-06T15:00:00Z') -> "Oct 6, 2026"
 */
export function formatDate(
  dateInput: string | Date | null | undefined,
  options: Intl.DateTimeFormatOptions = { year: 'numeric', month: 'short', day: 'numeric' },
  locale = 'en-US'
): string {
  if (!dateInput) return '-';
  const date = typeof dateInput === 'string' ? new Date(dateInput) : dateInput;
  if (isNaN(date.getTime())) return '-';
  return new Intl.DateTimeFormat(locale, options).format(date);
}

/**
 * Checks if a given date falls on the current calendar day.
 */
export function isToday(dateInput: string | Date | null | undefined): boolean {
  if (!dateInput) return false;
  const date = typeof dateInput === 'string' ? new Date(dateInput) : dateInput;
  const today = new Date();
  return (
    date.getDate() === today.getDate() &&
    date.getMonth() === today.getMonth() &&
    date.getFullYear() === today.getFullYear()
  );
}

/**
 * Calculates calendar day difference between two dates.
 */
export function diffInDays(start: Date | string, end: Date | string): number {
  const d1 = typeof start === 'string' ? new Date(start) : start;
  const d2 = typeof end === 'string' ? new Date(end) : end;
  const diffTime = Math.abs(d2.getTime() - d1.getTime());
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
}

/**
 * Adds an integer number of days to a date.
 */
export function addDays(date: Date, days: number): Date {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

/**
 * Sets time to beginning of the day (00:00:00.000).
 */
export function startOfDay(date: Date): Date {
  const result = new Date(date);
  result.setHours(0, 0, 0, 0);
  return result;
}

/**
 * Sets time to end of the day (23:59:59.999).
 */
export function endOfDay(date: Date): Date {
  const result = new Date(date);
  result.setHours(23, 59, 59, 999);
  return result;
}
