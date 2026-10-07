"use client";

import * as React from "react";

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  React.useEffect(() => {
    console.error("Root layout crash caught by GlobalError:", error);
  }, [error]);

  return (
    <html lang="en">
      <body className="flex min-h-screen flex-col items-center justify-center bg-zinc-950 p-6 text-zinc-100 antialiased font-sans">
        <div className="mx-auto max-w-md text-center space-y-4">
          <div className="mx-auto flex size-12 items-center justify-center rounded-full bg-red-500/20 text-red-400">
            !
          </div>
          <h1 className="text-2xl font-bold tracking-tight">
            Application Shell Error
          </h1>
          <p className="text-sm text-zinc-400">
            A critical failure occurred within the application layout shell.
          </p>
          {error.digest && (
            <p className="font-mono text-xs text-zinc-500">
              Digest: {error.digest}
            </p>
          )}
          <div className="pt-2">
            <button
              onClick={() => reset()}
              type="button"
              className="inline-flex h-9 items-center justify-center rounded-md bg-zinc-100 px-4 text-sm font-medium text-zinc-900 transition-colors hover:bg-zinc-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-zinc-400"
            >
              Reload Application
            </button>
          </div>
        </div>
      </body>
    </html>
  );
}
