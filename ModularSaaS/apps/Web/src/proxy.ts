import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const SUPPORTED_LOCALES = ["en", "ur"] as const;
type SupportedLocale = (typeof SUPPORTED_LOCALES)[number];
const DEFAULT_LOCALE: SupportedLocale = "en";

/**
 * Resolves the tenant ID from custom subdomain, request headers, query parameters, or cookies.
 */
function resolveTenant(request: NextRequest): string {
  // 1. Check for explicit query parameter
  const queryTenant = request.nextUrl.searchParams.get("tenant");
  if (queryTenant) return queryTenant;

  // 2. Check for cookie
  const cookieTenant = request.cookies.get("modular_tenant")?.value;
  if (cookieTenant) return cookieTenant;

  // 3. Check for subdomain from host header
  const host = request.headers.get("host") ?? "";
  const parts = host.split(".");
  // e.g. tenant-a.domain.com (3+ parts) or tenant-a.localhost:3000
  if (
    parts.length >= 2 &&
    parts[0] &&
    parts[0] !== "www" &&
    parts[0] !== "localhost"
  ) {
    return parts[0];
  }

  // 4. Fallback to default tenant
  return "default";
}

/**
 * Resolves preferred locale (English or Urdu) from cookie or Accept-Language header.
 */
function resolveLocale(request: NextRequest): SupportedLocale {
  const cookieLocale = request.cookies.get("NEXT_LOCALE")
    ?.value as SupportedLocale;
  if (cookieLocale && SUPPORTED_LOCALES.includes(cookieLocale)) {
    return cookieLocale;
  }

  const acceptLanguage = request.headers.get("accept-language") ?? "";
  if (acceptLanguage.toLowerCase().includes("ur")) {
    return "ur";
  }

  return DEFAULT_LOCALE;
}

export function proxy(request: NextRequest): NextResponse {
  const tenantId = resolveTenant(request);
  const locale = resolveLocale(request);
  const direction = locale === "ur" ? "rtl" : "ltr";

  // Clone headers to forward context downstream
  const requestHeaders = new Headers(request.headers);
  requestHeaders.set("x-tenant-id", tenantId);
  requestHeaders.set("x-locale", locale);
  requestHeaders.set("x-direction", direction);

  const response = NextResponse.next({
    request: {
      headers: requestHeaders,
    },
  });

  // Enterprise Security Headers
  response.headers.set("X-Frame-Options", "DENY");
  response.headers.set("X-Content-Type-Options", "nosniff");
  response.headers.set("Referrer-Policy", "strict-origin-when-cross-origin");
  response.headers.set(
    "Permissions-Policy",
    "camera=(), microphone=(), geolocation=(), browsing-topics=()"
  );

  // Set tenant and locale tracking cookies if not already present
  if (!request.cookies.has("modular_tenant")) {
    response.cookies.set("modular_tenant", tenantId, {
      path: "/",
      sameSite: "lax",
      httpOnly: false,
    });
  }

  if (!request.cookies.has("NEXT_LOCALE")) {
    response.cookies.set("NEXT_LOCALE", locale, {
      path: "/",
      sameSite: "lax",
      httpOnly: false,
    });
  }

  return response;
}

export const config = {
  matcher: [
    /*
     * Match all request paths except:
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     * - public asset extensions (svg, png, jpg, etc.)
     */
    "/((?!_next/static|_next/image|favicon.ico|.*\\.(?:svg|png|jpg|jpeg|gif|webp)$).*)",
  ],
};
