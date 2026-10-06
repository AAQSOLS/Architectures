/**
 * Number formatting, math, and compact abbreviation utilities.
 */

/**
 * Constrains a number between a minimum and maximum boundary.
 */
export function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

/**
 * Rounds a number to a specified number of decimal places without floating point drift.
 */
export function roundTo(value: number, decimals = 2): number {
  const factor = Math.pow(10, decimals);
  return Math.round((value + Number.EPSILON) * factor) / factor;
}

/**
 * Formats large integers into compact notation (e.g. 1200 -> "1.2K", 2500000 -> "2.5M").
 */
export function formatCompact(value: number | null | undefined): string {
  if (value === null || value === undefined || isNaN(value)) return '0';

  if (Math.abs(value) >= 1_000_000_000) {
    return `${(value / 1_000_000_000).toFixed(1).replace(/\.0$/, '')}B`;
  }
  if (Math.abs(value) >= 1_000_000) {
    return `${(value / 1_000_000).toFixed(1).replace(/\.0$/, '')}M`;
  }
  if (Math.abs(value) >= 1_000) {
    return `${(value / 1_000).toFixed(1).replace(/\.0$/, '')}K`;
  }

  return value.toLocaleString();
}

/**
 * Calculates a percentage safely, avoiding division-by-zero exceptions.
 */
export function calculatePercentage(part: number, total: number): number {
  if (!total || total === 0) return 0;
  return roundTo((part / total) * 100, 1);
}

/**
 * Formats a ratio as a percentage string (e.g. 0.155 -> "15.5%").
 */
export function formatPercentage(ratio: number, decimals = 1): string {
  return `${(ratio * 100).toFixed(decimals)}%`;
}

/**
 * Validates if a string or unknown value represents a valid finite number.
 */
export function isNumeric(value: unknown): boolean {
  if (typeof value === 'number') return !isNaN(value) && isFinite(value);
  if (typeof value !== 'string') return false;
  return !isNaN(parseFloat(value)) && isFinite(Number(value));
}
