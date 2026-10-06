/**
 * String manipulation, search matching, and privacy utilities.
 */

/**
 * Performs case-insensitive and whitespace-trimmed search matching.
 */
export function containsIgnoreCase(
  source: string | null | undefined,
  query: string | null | undefined
): boolean {
  if (!source || !query) return false;
  return source.toLowerCase().trim().includes(query.toLowerCase().trim());
}

/**
 * Masks phone numbers to prevent scraping while keeping the prefix and suffix recognizable.
 * Example: "03001234567" -> "0300-***4567"
 */
export function maskPhoneNumber(phone: string | null | undefined): string {
  if (!phone || phone.length < 7) return phone ?? '';
  const clean = phone.replace(/\D/g, '');
  if (clean.length < 7) return phone;
  const prefix = clean.slice(0, 4);
  const suffix = clean.slice(-4);
  return `${prefix}-***${suffix}`;
}

/**
 * Masks email addresses for privacy display.
 * Example: "john.doe@example.com" -> "j***e@example.com"
 */
export function maskEmail(email: string | null | undefined): string {
  if (!email || !email.includes('@')) return email ?? '';
  const [local, domain] = email.split('@');
  if (!local || !domain) return email;
  if (local.length <= 2) return `${local}***@${domain}`;
  return `${local[0]}***${local[local.length - 1]}@${domain}`;
}

/**
 * Truncates text cleanly without breaking words in the middle.
 */
export function truncate(text: string | null | undefined, maxLength: number, suffix = '...'): string {
  if (!text || text.length <= maxLength) return text ?? '';
  const truncated = text.slice(0, maxLength);
  const lastSpace = truncated.lastIndexOf(' ');
  const clean = lastSpace > 0 ? truncated.slice(0, lastSpace) : truncated;
  return `${clean.trimEnd()}${suffix}`;
}

/**
 * Generates URL-friendly slugs for societies, projects, and articles.
 * Example: "Gulberg Greens Phase 1" -> "gulberg-greens-phase-1"
 */
export function slugify(text: string | null | undefined): string {
  if (!text) return '';
  return text
    .toString()
    .toLowerCase()
    .trim()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '') // Remove diacritics
    .replace(/[^a-z0-9 -]/g, '')     // Remove invalid chars
    .replace(/\s+/g, '-')            // Replace spaces with hyphens
    .replace(/-+/g, '-');            // Collapse multiple hyphens
}

/**
 * Extracts initials from a user's full name for avatars.
 * Example: "Ghulam Mohiuddin" -> "GM"
 */
export function getInitials(name: string | null | undefined, maxChars = 2): string {
  if (!name) return '';
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return '';
  return parts
    .slice(0, maxChars)
    .map(part => part[0]?.toUpperCase() ?? '')
    .join('');
}

/**
 * Capitalizes the first letter of each word in a string.
 */
export function capitalizeWords(text: string | null | undefined): string {
  if (!text) return '';
  return text
    .toLowerCase()
    .replace(/(?:^|\s|-)\S/g, char => char.toUpperCase());
}

/**
 * Returns singular or plural form based on item count.
 * Example: pluralize(1, 'listing', 'listings') -> "1 listing"
 */
export function pluralize(count: number, singular: string, plural?: string): string {
  const word = count === 1 ? singular : (plural ?? `${singular}s`);
  return `${count} ${word}`;
}

/**
 * Strips HTML tags safely without triggering DOM sinks or innerHTML execution.
 */
export function stripHtml(html: string | null | undefined): string {
  if (!html) return '';
  return html.replace(/<[^>]*>?/gm, '').trim();
}
