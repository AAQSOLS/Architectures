/**
 * Formats ISO date string to human-readable date.
 */
export function formatIsoDate(
  dateString: string | Date,
  locale: string = "en-US",
  options?: Intl.DateTimeFormatOptions
): string {
  const date =
    typeof dateString === "string" ? new Date(dateString) : dateString;
  if (isNaN(date.getTime())) return "";

  const defaultOptions: Intl.DateTimeFormatOptions = {
    year: "numeric",
    month: "short",
    day: "numeric",
  };

  return new Intl.DateTimeFormat(locale, options ?? defaultOptions).format(
    date
  );
}

/**
 * Returns relative time string (e.g. "5 minutes ago", "in 2 days").
 */
export function formatRelativeTime(
  dateString: string | Date,
  locale: string = "en-US"
): string {
  const date =
    typeof dateString === "string" ? new Date(dateString) : dateString;
  if (isNaN(date.getTime())) return "";

  const now = Date.now();
  const diffInSeconds = Math.round((date.getTime() - now) / 1000);

  const rtf = new Intl.RelativeTimeFormat(locale, { numeric: "auto" });

  const units: Array<{ unit: Intl.RelativeTimeFormatUnit; seconds: number }> = [
    { unit: "year", seconds: 31536000 },
    { unit: "month", seconds: 2592000 },
    { unit: "week", seconds: 604800 },
    { unit: "day", seconds: 86400 },
    { unit: "hour", seconds: 3600 },
    { unit: "minute", seconds: 60 },
    { unit: "second", seconds: 1 },
  ];

  for (const { unit, seconds } of units) {
    if (Math.abs(diffInSeconds) >= seconds || unit === "second") {
      const count = Math.round(diffInSeconds / seconds);
      return rtf.format(count, unit);
    }
  }

  return "";
}

/**
 * Checks if a given timestamp has passed.
 */
export function isExpired(dateString: string | Date): boolean {
  const date =
    typeof dateString === "string" ? new Date(dateString) : dateString;
  return date.getTime() <= Date.now();
}

/**
 * Adds days to a date.
 */
export function addDays(date: Date, days: number): Date {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}
