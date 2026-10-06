"use client";

import * as React from "react";
import Link from "next/link";
import { Layers, Menu } from "lucide-react";
import { Button, buttonVariants } from "@/components/ui/button";
import { ThemeToggle } from "@/components/theme-toggle";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { cn } from "@/lib/utils";

const NAV_ITEMS = [
  { label: "Features", href: "#features" },
  { label: "Architecture", href: "#architecture" },
  { label: "Pricing", href: "#pricing" },
  { label: "FAQ", href: "#faq" },
];

export function SiteHeader() {
  const [mobileOpen, setMobileOpen] = React.useState(false);

  return (
    <header className="sticky top-0 z-40 w-full border-b border-border bg-background/95 backdrop-blur-md supports-backdrop-filter:bg-background/80">
      <div className="container mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        {/* Brand */}
        <Link
          href="/"
          className="flex items-center gap-2.5 font-heading text-lg font-bold tracking-tight text-foreground transition-opacity hover:opacity-90"
        >
          <div className="flex size-9 items-center justify-center rounded-lg bg-primary text-primary-foreground shadow-xs">
            <Layers className="size-5" />
          </div>
          <span>
            Modular
            <span className="text-muted-foreground font-normal">SaaS</span>
          </span>
        </Link>

        {/* Desktop Navigation */}
        <nav className="hidden md:flex items-center gap-6 text-sm font-medium">
          {NAV_ITEMS.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="text-muted-foreground transition-colors hover:text-foreground"
            >
              {item.label}
            </Link>
          ))}
        </nav>

        {/* Action Controls */}
        <div className="flex items-center gap-2.5">
          <ThemeToggle />

          <div className="hidden sm:flex items-center gap-2">
            <a
              href="http://localhost:4200"
              target="_blank"
              rel="noopener noreferrer"
              className={buttonVariants({ variant: "ghost", size: "sm" })}
            >
              Admin Portal
            </a>
            <Link href="#pricing" className={buttonVariants({ size: "sm" })}>
              Get Started
            </Link>
          </div>

          {/* Mobile Menu Trigger */}
          <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
            <SheetTrigger
              render={
                <Button
                  variant="outline"
                  size="icon"
                  className="size-9 md:hidden"
                  aria-label="Open navigation menu"
                />
              }
            >
              <Menu className="size-4" />
            </SheetTrigger>
            <SheetContent side="right" className="w-72 sm:w-80">
              <SheetHeader>
                <SheetTitle className="flex items-center gap-2">
                  <div className="flex size-7 items-center justify-center rounded-md bg-primary text-primary-foreground">
                    <Layers className="size-4" />
                  </div>
                  <span>ModularSaaS</span>
                </SheetTitle>
              </SheetHeader>
              <div className="flex flex-col gap-4 p-4 text-sm font-medium">
                {NAV_ITEMS.map((item) => (
                  <Link
                    key={item.href}
                    href={item.href}
                    onClick={() => setMobileOpen(false)}
                    className="rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
                  >
                    {item.label}
                  </Link>
                ))}
                <div className="my-2 h-px bg-border" />
                <a
                  href="http://localhost:4200"
                  target="_blank"
                  rel="noopener noreferrer"
                  className={cn(
                    buttonVariants({ variant: "outline" }),
                    "w-full justify-center"
                  )}
                >
                  Admin Portal
                </a>
                <Link
                  href="#pricing"
                  onClick={() => setMobileOpen(false)}
                  className={cn(buttonVariants(), "w-full justify-center")}
                >
                  Get Started
                </Link>
              </div>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </header>
  );
}
