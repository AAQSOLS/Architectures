import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { ThemeProvider } from "@/components/theme-provider";
import { TooltipProvider } from "@/components/ui/tooltip";
import { SiteHeader } from "@/components/layout/site-header";
import { SiteFooter } from "@/components/layout/site-footer";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
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
        className={`${geistSans.variable} ${geistMono.variable} flex min-h-screen flex-col bg-background text-foreground antialiased`}
      >
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <TooltipProvider>
            <SiteHeader />
            <main className="flex-1">{children}</main>
            <SiteFooter />
          </TooltipProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
