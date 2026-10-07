"use client";

import * as React from "react";
import enDictionary from "./dictionaries/en.json";
import urDictionary from "./dictionaries/ur.json";

export type Locale = "en" | "ur";
export type Direction = "ltr" | "rtl";

type Dictionary = typeof enDictionary;

const DICTIONARIES: Record<Locale, Dictionary> = {
  en: enDictionary,
  ur: urDictionary,
};

interface I18nContextValue {
  locale: Locale;
  direction: Direction;
  setLocale: (locale: Locale) => void;
  t: (key: string) => string;
}

const I18nContext = React.createContext<I18nContextValue | null>(null);

function getNestedValue(obj: Record<string, unknown>, path: string): string {
  const parts = path.split(".");
  let current: unknown = obj;

  for (const part of parts) {
    if (typeof current === "object" && current !== null && part in current) {
      current = (current as Record<string, unknown>)[part];
    } else {
      return path;
    }
  }

  return typeof current === "string" ? current : path;
}

export function I18nProvider({
  initialLocale = "en",
  children,
}: {
  initialLocale?: Locale;
  children: React.ReactNode;
}) {
  const [locale, setLocaleState] = React.useState<Locale>(initialLocale);
  const direction: Direction = locale === "ur" ? "rtl" : "ltr";

  const setLocale = React.useCallback((newLocale: Locale) => {
    setLocaleState(newLocale);
    document.cookie = `NEXT_LOCALE=${newLocale}; path=/; max-age=31536000; SameSite=Lax`;
    document.documentElement.lang = newLocale;
    document.documentElement.dir = newLocale === "ur" ? "rtl" : "ltr";
  }, []);

  React.useEffect(() => {
    // Sync document attributes on mount
    document.documentElement.lang = locale;
    document.documentElement.dir = direction;
  }, [locale, direction]);

  const t = React.useCallback(
    (key: string): string => {
      const dict = DICTIONARIES[locale] as unknown as Record<string, unknown>;
      return getNestedValue(dict, key);
    },
    [locale]
  );

  const value = React.useMemo(
    () => ({
      locale,
      direction,
      setLocale,
      t,
    }),
    [locale, direction, setLocale, t]
  );

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n(): I18nContextValue {
  const context = React.useContext(I18nContext);
  if (!context) {
    throw new Error("useI18n must be used within an I18nProvider");
  }
  return context;
}
