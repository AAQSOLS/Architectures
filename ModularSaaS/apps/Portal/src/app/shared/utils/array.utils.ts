/**
 * Array and collection utilities for client-side list operations.
 */

/**
 * Groups an array of objects by a selected key property.
 * Example: groupBy(properties, p => p.society)
 */
export function groupBy<T, K extends string | number | symbol>(
  array: T[],
  keySelector: (item: T) => K
): Record<K, T[]> {
  return array.reduce((accumulator, currentItem) => {
    const key = keySelector(currentItem);
    if (!accumulator[key]) {
      accumulator[key] = [];
    }
    accumulator[key]!.push(currentItem);
    return accumulator;
  }, {} as Record<K, T[]>);
}

/**
 * Deduplicates an array based on a unique property key.
 * Example: distinctBy(users, u => u.id)
 */
export function distinctBy<T, K>(array: T[], keySelector: (item: T) => K): T[] {
  const seen = new Set<K>();
  return array.filter(item => {
    const key = keySelector(item);
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

/**
 * Sorts an array immutably by a selected key in ascending or descending order.
 */
export function sortBy<T>(
  array: T[],
  keySelector: (item: T) => string | number | Date,
  direction: 'asc' | 'desc' = 'asc'
): T[] {
  return [...array].sort((a, b) => {
    const valA = keySelector(a);
    const valB = keySelector(b);

    if (valA === valB) return 0;
    const comparison = valA > valB ? 1 : -1;
    return direction === 'asc' ? comparison : -comparison;
  });
}

/**
 * Splits an array into chunks of a given maximum size.
 */
export function chunk<T>(array: T[], size: number): T[][] {
  if (size <= 0) return [array];
  const chunks: T[][] = [];
  for (let i = 0; i < array.length; i += size) {
    chunks.push(array.slice(i, i + size));
  }
  return chunks;
}

/**
 * Splits an array into two arrays based on a predicate: [matches, nonMatches].
 */
export function partition<T>(array: T[], predicate: (item: T) => boolean): [T[], T[]] {
  const pass: T[] = [];
  const fail: T[] = [];
  for (const item of array) {
    if (predicate(item)) {
      pass.push(item);
    } else {
      fail.push(item);
    }
  }
  return [pass, fail];
}

/**
 * Performs client-side pagination on an in-memory array.
 */
export function paginate<T>(array: T[], pageNumber: number, pageSize: number): T[] {
  const startIndex = (pageNumber - 1) * pageSize;
  return array.slice(startIndex, startIndex + pageSize);
}
