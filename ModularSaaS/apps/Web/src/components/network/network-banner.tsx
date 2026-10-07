"use client";

import { WifiOff } from "lucide-react";
import { useNetworkStatus } from "@/hooks/use-network-status";
import { useI18n } from "@/lib/i18n/i18n-context";

export function NetworkBanner() {
  const isOnline = useNetworkStatus();
  const { t } = useI18n();

  if (isOnline) {
    return null;
  }

  return (
    <div
      role="alert"
      className="fixed bottom-4 left-1/2 z-50 flex -translate-x-1/2 items-center gap-2.5 rounded-lg border border-destructive/30 bg-destructive/90 px-4 py-2.5 text-xs font-medium text-destructive-foreground shadow-lg backdrop-blur-md animate-in fade-in slide-in-from-bottom-2 duration-200"
    >
      <WifiOff className="size-4 shrink-0" />
      <span>{t("common.offlineNotice")}</span>
    </div>
  );
}
