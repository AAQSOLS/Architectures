"use client";

import * as React from "react";
import { useTenant } from "@/lib/tenant/tenant-context";
import { applyTenantTheme, getTenantTheme } from "@/lib/tenant/tenant-theme";

export function TenantThemeInjector() {
  const { tenantId } = useTenant();

  React.useEffect(() => {
    const themeConfig = getTenantTheme(tenantId);
    applyTenantTheme(themeConfig);
  }, [tenantId]);

  return null;
}
