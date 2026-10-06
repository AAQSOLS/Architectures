import Link from "next/link";
import { Layers } from "lucide-react";

export function SiteFooter() {
  return (
    <footer className="w-full border-t border-border bg-muted/30 text-muted-foreground">
      <div className="container mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 gap-8 md:grid-cols-4">
          {/* Brand */}
          <div className="space-y-3 md:col-span-1">
            <Link
              href="/"
              className="flex items-center gap-2 font-heading text-lg font-bold text-foreground"
            >
              <div className="flex size-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                <Layers className="size-4" />
              </div>
              <span>ModularSaaS</span>
            </Link>
            <p className="text-sm leading-relaxed text-muted-foreground">
              Production-ready multi-tenant SaaS frontend template powered by
              Next.js App Router, Tailwind CSS, and shadcn/ui.
            </p>
          </div>

          {/* Product */}
          <div className="space-y-3">
            <h4 className="text-sm font-semibold uppercase tracking-wider text-foreground">
              Product
            </h4>
            <ul className="space-y-2 text-sm">
              <li>
                <Link href="#features" className="hover:text-foreground">
                  Features
                </Link>
              </li>
              <li>
                <Link href="#architecture" className="hover:text-foreground">
                  Architecture
                </Link>
              </li>
              <li>
                <Link href="#pricing" className="hover:text-foreground">
                  Pricing Plans
                </Link>
              </li>
              <li>
                <Link href="#faq" className="hover:text-foreground">
                  FAQ
                </Link>
              </li>
            </ul>
          </div>

          {/* Resources */}
          <div className="space-y-3">
            <h4 className="text-sm font-semibold uppercase tracking-wider text-foreground">
              Resources
            </h4>
            <ul className="space-y-2 text-sm">
              <li>
                <a
                  href="https://ui.shadcn.com"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="hover:text-foreground"
                >
                  shadcn/ui Documentation
                </a>
              </li>
              <li>
                <a
                  href="https://nextjs.org/docs"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="hover:text-foreground"
                >
                  Next.js App Router
                </a>
              </li>
              <li>
                <a
                  href="https://tailwindcss.com"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="hover:text-foreground"
                >
                  Tailwind CSS
                </a>
              </li>
            </ul>
          </div>

          {/* Portals */}
          <div className="space-y-3">
            <h4 className="text-sm font-semibold uppercase tracking-wider text-foreground">
              Applications
            </h4>
            <ul className="space-y-2 text-sm">
              <li>
                <a
                  href="http://localhost:4200"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="hover:text-foreground"
                >
                  Admin Portal (Angular 19)
                </a>
              </li>
              <li>
                <Link href="/" className="hover:text-foreground">
                  Public Marketplace (Next.js)
                </Link>
              </li>
            </ul>
          </div>
        </div>

        <div className="mt-10 flex flex-col items-center justify-between border-t border-border pt-6 text-xs sm:flex-row">
          <p>
            &copy; {new Date().getFullYear()} ModularSaaS Template. Built with
            shadcn token theming.
          </p>
          <div className="mt-4 flex gap-6 sm:mt-0">
            <span className="text-muted-foreground">MIT License</span>
          </div>
        </div>
      </div>
    </footer>
  );
}
