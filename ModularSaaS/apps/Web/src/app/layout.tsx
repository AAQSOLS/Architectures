import type { Metadata } from "next";
import { Geist, Geist_Mono, Noto_Nastaliq_Urdu } from "next/font/google";
import "./globals.css";
import { ThemeProvider } from "@/components/theme-provider";
import { TooltipProvider } from "@/components/ui/tooltip";
import { TenantProvider } from "@/lib/tenant/tenant-context";
import { TenantThemeInjector } from "@/components/tenant/tenant-theme-injector";
import { I18nProvider } from "@/lib/i18n/i18n-context";
import { SessionProvider } from "@/lib/auth/session-context";
import { SiteHeader } from "@/components/layout/site-header";
import { SiteFooter } from "@/components/layout/site-footer";
import { NetworkBanner } from "@/components/network/network-banner";

const geistSans = Geist({
  variable: "--font-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

const notoNastaliq = Noto_Nastaliq_Urdu({
  variable: "--font-urdu",
  subsets: ["arabic"],
  weight: ["400", "700"],
});

export const metadata: Metadata = {
  title: "ModularSaaS | Enterprise Multi-Tenant Frontend Template",
  description:
    "Production-grade token-driven SaaS web portal built with Next.js App Router, Tailwind CSS, and shadcn/ui.",
  keywords: [
    "Next.js",
    "React",
    "Tailwind CSS",
    "shadcn/ui",
    "Multi-Tenant",
    "SaaS Template",
  ],
  authors: [{ name: "ModularSaaS Team" }],
  openGraph: {
    title: "ModularSaaS - Enterprise Multi-Tenant Frontend Template",
    description:
      "Production-grade token-driven SaaS web portal built with Next.js App Router, Tailwind CSS, and shadcn/ui.",
    type: "website",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body
        className={`${geistSans.variable} ${geistMono.variable} ${notoNastaliq.variable} flex min-h-screen flex-col bg-background text-foreground antialiased`}
      >
        <TenantProvider>
          <TenantThemeInjector />
          <I18nProvider>
            <SessionProvider>
              <ThemeProvider
                attribute="class"
                defaultTheme="system"
                enableSystem
                disableTransitionOnChange
              >
                <TooltipProvider>
                  <NetworkBanner />
                  <SiteHeader />
                  <main className="flex-1">{children}</main>
                  <SiteFooter />
                </TooltipProvider>
              </ThemeProvider>
            </SessionProvider>
          </I18nProvider>
        </TenantProvider>
      </body>
    </html>
  );
}
