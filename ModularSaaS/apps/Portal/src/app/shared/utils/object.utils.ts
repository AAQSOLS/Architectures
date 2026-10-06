/**
 * Object manipulation, cloning, and diffing utilities.
 */

/**
 * Creates a deep clone of a serializable object.
 * Uses structuredClone when available with a JSON parse/stringify fallback.
 */
export function deepClone<T>(source: T): T {
  if (source === null || typeof source !== 'object') {
    return source;
  }
  if (typeof structuredClone === 'function') {
    try {
      return structuredClone(source);
    } catch {
      // Fallback on objects with functions or non-cloneable symbols
    }
  }
  return JSON.parse(JSON.stringify(source)) as T;
}

/**
 * Creates an object composed of the selected object properties.
 * Example: pick(user, ['id', 'email'])
 */
export function pick<T extends object, K extends keyof T>(source: T, keys: K[]): Pick<T, K> {
  const result = {} as Pick<T, K>;
  for (const key of keys) {
    if (key in source) {
      result[key] = source[key];
    }
  }
  return result;
}

/**
 * Creates an object composed of the object properties with specific keys omitted.
 * Example: omit(user, ['passwordHash'])
 */
export function omit<T extends object, K extends keyof T>(source: T, keys: K[]): Omit<T, K> {
  const result = { ...source };
  for (const key of keys) {
    delete result[key];
  }
  return result;
}

/**
 * Removes null, undefined, and empty string properties from an object recursively.
 * Ideal for cleaning up form payloads before submitting to backend APIs.
 */
export function removeEmpty<T extends Record<string, unknown>>(obj: T): Partial<T> {
  const clean = {} as Partial<T>;

  for (const [key, value] of Object.entries(obj)) {
    if (value === null || value === undefined || value === '') {
      continue;
    }
    if (typeof value === 'object' && !Array.isArray(value) && !(value instanceof Date)) {
      const nested = removeEmpty(value as Record<string, unknown>);
      if (Object.keys(nested).length > 0) {
        clean[key as keyof T] = nested as unknown as T[keyof T];
      }
    } else {
      clean[key as keyof T] = value as T[keyof T];
    }
  }

  return clean;
}

/**
 * Performs a deep equality check between two objects.
 * Useful for determining if a form is dirty compared to its initial state.
 */
export function isDeepEqual(a: unknown, b: unknown): boolean {
  if (a === b) return true;
  if (a === null || b === null || typeof a !== 'object' || typeof b !== 'object') {
    return false;
  }

  const keysA = Object.keys(a as object);
  const keysB = Object.keys(b as object);

  if (keysA.length !== keysB.length) return false;

  for (const key of keysA) {
    if (!keysB.includes(key)) return false;
    if (!isDeepEqual((a as Record<string, unknown>)[key], (b as Record<string, unknown>)[key])) {
      return false;
    }
  }

  return true;
}
