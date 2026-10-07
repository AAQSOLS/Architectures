"use client";

import { Languages } from "lucide-react";
import { useI18n, Locale } from "@/lib/i18n/i18n-context";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

export function LanguageSwitcher() {
  const { locale, setLocale } = useI18n();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button
            variant="ghost"
            size="sm"
            className="h-9 gap-1.5 px-2.5 text-xs font-semibold"
            aria-label="Switch language"
          />
        }
      >
        <Languages className="size-4" />
        <span>{locale === "ur" ? "اردو" : "EN"}</span>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem
          onClick={() => setLocale("en" as Locale)}
          className="cursor-pointer font-medium"
        >
          <span>English (LTR)</span>
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => setLocale("ur" as Locale)}
          className="cursor-pointer font-medium font-urdu"
        >
          <span>اردو (RTL)</span>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
