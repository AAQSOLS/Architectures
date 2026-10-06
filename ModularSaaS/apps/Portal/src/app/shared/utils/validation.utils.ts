/**
 * Common format validators and regular expressions.
 */

const EMAIL_REGEX = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$/;
const GUID_REGEX = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const PK_PHONE_REGEX = /^(?:(?:\+92)|(?:0092)|(?:92)|0)?(3[0-9]{9})$/;
const CNIC_REGEX = /^[0-9]{5}-?[0-9]{7}-?[0-9]$/;

/**
 * Validates if an email address conforms to RFC format.
 */
export function isValidEmail(email: string | null | undefined): boolean {
  if (!email) return false;
  return EMAIL_REGEX.test(email.trim());
}

/**
 * Validates if a string is a valid UUID/GUID (matching C# Guid).
 */
export function isValidGuid(guid: string | null | undefined): boolean {
  if (!guid) return false;
  return GUID_REGEX.test(guid.trim());
}

/**
 * Validates Pakistani mobile numbers (e.g. 03001234567, +923001234567).
 */
export function isValidPkPhone(phone: string | null | undefined): boolean {
  if (!phone) return false;
  const clean = phone.replace(/[\s-]/g, '');
  return PK_PHONE_REGEX.test(clean);
}

/**
 * Normalizes a Pakistani phone number to standard international format (+923001234567).
 */
export function normalizePkPhone(phone: string | null | undefined): string | null {
  if (!phone) return null;
  const clean = phone.replace(/[\s-]/g, '');
  const match = clean.match(PK_PHONE_REGEX);
  if (!match || !match[1]) return null;
  return `+92${match[1]}`;
}

/**
 * Validates Pakistani CNIC format (13 digits, optional hyphens: 12345-1234567-1).
 */
export function isValidCnic(cnic: string | null | undefined): boolean {
  if (!cnic) return false;
  return CNIC_REGEX.test(cnic.trim());
}

/**
 * Validates if a string is a secure web URL (http or https).
 */
export function isValidUrl(url: string | null | undefined): boolean {
  if (!url) return false;
  try {
    const parsed = new URL(url);
    return parsed.protocol === 'http:' || parsed.protocol === 'https:';
  } catch {
    return false;
  }
}
