/**
 * Serializes an object into a safe URL query string, omitting null/undefined values.
 */
export function buildQueryString(
  params: Record<
    string,
    string | number | boolean | null | undefined | Array<string | number>
  >
): string {
  const searchParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value === null || value === undefined || value === "") {
      return;
    }

    if (Array.isArray(value)) {
      value.forEach((item) => {
        if (item !== null && item !== undefined) {
          searchParams.append(key, String(item));
        }
      });
    } else {
      searchParams.set(key, String(value));
    }
  });

  const queryString = searchParams.toString();
  return queryString ? `?${queryString}` : "";
}

/**
 * Parses query string into an object.
 */
export function parseQueryString(queryString: string): Record<string, string> {
  const clean = queryString.startsWith("?")
    ? queryString.slice(1)
    : queryString;
  const searchParams = new URLSearchParams(clean);
  const result: Record<string, string> = {};

  searchParams.forEach((val, key) => {
    result[key] = val;
  });

  return result;
}
