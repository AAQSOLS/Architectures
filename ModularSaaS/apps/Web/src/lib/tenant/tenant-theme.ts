export interface TenantThemeConfig {
  primaryColor?: string;
  primaryForeground?: string;
  radius?: string;
}

const TENANT_THEME_REGISTRY: Record<string, TenantThemeConfig> = {
  default: {
    primaryColor: "oklch(0.205 0 0)",
    primaryForeground: "oklch(0.985 0 0)",
    radius: "0.625rem",
  },
  "tenant-alpha": {
    primaryColor: "oklch(0.55 0.22 260)", // Modern Indigo/Blue
    primaryForeground: "oklch(0.99 0 0)",
    radius: "0.5rem",
  },
  "tenant-beta": {
    primaryColor: "oklch(0.58 0.18 150)", // Emerald Green
    primaryForeground: "oklch(0.99 0 0)",
    radius: "0.75rem",
  },
};

/**
 * Applies tenant-specific CSS variables to document root.
 */
export function applyTenantTheme(config?: TenantThemeConfig): void {
  if (typeof document === "undefined" || !config) return;

  const root = document.documentElement;

  if (config.primaryColor) {
    root.style.setProperty("--primary", config.primaryColor);
  }

  if (config.primaryForeground) {
    root.style.setProperty("--primary-foreground", config.primaryForeground);
  }

  if (config.radius) {
    root.style.setProperty("--radius", config.radius);
  }
}

/**
 * Resolves theme configuration for a given tenant identifier.
 */
export function getTenantTheme(tenantId: string): TenantThemeConfig {
  return TENANT_THEME_REGISTRY[tenantId] ?? TENANT_THEME_REGISTRY["default"]!;
}
