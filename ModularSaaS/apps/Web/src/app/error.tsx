"use client";

import * as React from "react";
import { AlertTriangle, RotateCcw } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function ErrorBoundary({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  React.useEffect(() => {
    // Log unexpected errors
    console.error("Route error caught by boundary:", error);
  }, [error]);

  return (
    <div className="container mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center px-4 text-center">
      <div className="flex size-14 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
        <AlertTriangle className="size-7" />
      </div>
      <h1 className="mt-6 font-heading text-3xl font-bold tracking-tight">
        Something Went Wrong
      </h1>
      <p className="mt-2 text-sm text-muted-foreground">
        An unexpected runtime error occurred while rendering this view.
      </p>
      {error.digest && (
        <p className="mt-1 font-mono text-xs text-muted-foreground">
          Error Digest: {error.digest}
        </p>
      )}
      <div className="mt-6 flex gap-3">
        <Button onClick={() => reset()} className="gap-2">
          <RotateCcw className="size-4" />
          <span>Try Again</span>
        </Button>
      </div>
    </div>
  );
}
