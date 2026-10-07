"use client";

import * as React from "react";

/**
 * Warns users before navigating or closing the tab if form state contains unsaved modifications.
 */
export function useUnsavedChanges(isDirty: boolean): void {
  React.useEffect(() => {
    if (!isDirty) return;

    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      return "";
    };

    window.addEventListener("beforeunload", handleBeforeUnload);

    return () => {
      window.removeEventListener("beforeunload", handleBeforeUnload);
    };
  }, [isDirty]);
}
