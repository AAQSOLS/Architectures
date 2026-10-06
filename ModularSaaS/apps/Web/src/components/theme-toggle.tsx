"use client";

import * as React from "react";
import { Moon, Sun, Laptop } from "lucide-react";
import { useTheme } from "next-themes";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

const subscribe = () => () => {};

function getThemeIcon(theme?: string) {
  if (theme === "dark") {
    return <Moon className="size-4" />;
  }
  if (theme === "system") {
    return <Laptop className="size-4" />;
  }
  return <Sun className="size-4" />;
}

export function ThemeToggle() {
  const { setTheme, theme } = useTheme();
  const mounted = React.useSyncExternalStore(
    subscribe,
    () => true,
    () => false
  );

  if (!mounted) {
    return (
      <Button
        variant="outline"
        size="icon"
        className="size-9"
        aria-label="Toggle theme"
      >
        <Sun className="size-4" />
      </Button>
    );
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button
            variant="outline"
            size="icon"
            className="size-9"
            aria-label="Toggle theme"
          />
        }
      >
        {getThemeIcon(theme)}
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem
          onClick={() => setTheme("light")}
          className="cursor-pointer"
        >
          <Sun className="mr-2 size-4" />
          <span>Light</span>
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => setTheme("dark")}
          className="cursor-pointer"
        >
          <Moon className="mr-2 size-4" />
          <span>Dark</span>
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => setTheme("system")}
          className="cursor-pointer"
        >
          <Laptop className="mr-2 size-4" />
          <span>System</span>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
