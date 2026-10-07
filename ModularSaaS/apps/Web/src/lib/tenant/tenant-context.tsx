"use client";

import * as React from "react";
import { tabSyncService } from "@/lib/sync/tab-sync.service";

interface TenantContextValue {
  tenantId: string;
  setTenantId: (tenantId: string) => void;
}

const TenantContext = React.createContext<TenantContextValue | null>(null);

export function TenantProvider({
  initialTenantId = "default",
  children,
}: {
  initialTenantId?: string;
  children: React.ReactNode;
}) {
  const [tenantId, setTenantIdState] = React.useState<string>(initialTenantId);

  const setTenantId = React.useCallback((newTenantId: string) => {
    setTenantIdState(newTenantId);
    document.cookie = `modular_tenant=${newTenantId}; path=/; max-age=31536000; SameSite=Lax`;
    tabSyncService.broadcast("TENANT_CHANGED", { tenantId: newTenantId });
  }, []);

  React.useEffect(() => {
    // Listen for cross-tab tenant changes
    const cleanup = tabSyncService.on<{ tenantId: string }>(
      "TENANT_CHANGED",
      (msg) => {
        if (msg.payload?.tenantId && msg.payload.tenantId !== tenantId) {
          setTenantIdState(msg.payload.tenantId);
        }
      }
    );

    return cleanup;
  }, [tenantId]);

  const value = React.useMemo(
    () => ({
      tenantId,
      setTenantId,
    }),
    [tenantId, setTenantId]
  );

  return (
    <TenantContext.Provider value={value}>{children}</TenantContext.Provider>
  );
}

export function useTenant(): TenantContextValue {
  const context = React.useContext(TenantContext);
  if (!context) {
    throw new Error("useTenant must be used within a TenantProvider");
  }
  return context;
}
