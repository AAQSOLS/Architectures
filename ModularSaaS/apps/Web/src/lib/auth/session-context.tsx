"use client";

import * as React from "react";
import { tabSyncService } from "@/lib/sync/tab-sync.service";
import { storageService } from "@/lib/storage/storage.service";

export interface UserSession {
  userId: string;
  email: string;
  name: string;
  role?: string;
  tenantId?: string;
}

interface SessionContextValue {
  isAuthenticated: boolean;
  user: UserSession | null;
  login: (user: UserSession, token: string) => void;
  logout: () => void;
}

const SessionContext = React.createContext<SessionContextValue | null>(null);

export function SessionProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = React.useState<UserSession | null>(() => {
    return storageService.getItem<UserSession>("auth_user");
  });

  React.useEffect(() => {
    // Listen for cross-tab logout events
    const cleanup = tabSyncService.on("AUTH_LOGOUT", () => {
      setUser(null);
    });

    return cleanup;
  }, []);

  const login = React.useCallback((userData: UserSession, token: string) => {
    setUser(userData);
    storageService.setItem("auth_user", userData);
    document.cookie = `auth_token=${token}; path=/; max-age=86400; SameSite=Lax`;
  }, []);

  const logout = React.useCallback(() => {
    setUser(null);
    storageService.removeItem("auth_user");
    document.cookie = "auth_token=; path=/; max-age=0; SameSite=Lax";
    tabSyncService.broadcast("AUTH_LOGOUT");
  }, []);

  const value = React.useMemo(
    () => ({
      isAuthenticated: user !== null,
      user,
      login,
      logout,
    }),
    [user, login, logout]
  );

  return (
    <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
  );
}

export function useSession(): SessionContextValue {
  const context = React.useContext(SessionContext);
  if (!context) {
    throw new Error("useSession must be used within a SessionProvider");
  }
  return context;
}
