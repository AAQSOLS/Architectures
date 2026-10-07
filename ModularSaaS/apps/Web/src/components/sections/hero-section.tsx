"use client";

import Link from "next/link";
import {
  ArrowRight,
  Sparkles,
  CheckCircle2,
  ShieldCheck,
  Zap,
} from "lucide-react";
import { buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { useI18n } from "@/lib/i18n/i18n-context";

export function HeroSection() {
  const { t } = useI18n();

  return (
    <section className="relative overflow-hidden py-20 sm:py-28 lg:py-32">
      {/* Background radial gradient using semantic tokens */}
      <div className="pointer-events-none absolute inset-0 -z-10 flex items-center justify-center opacity-30 dark:opacity-20">
        <div className="h-[40rem] w-[40rem] rounded-full bg-primary/20 blur-3xl" />
      </div>

      <div className="container mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl text-center">
          {/* Release Badge */}
          <div className="mb-6 inline-flex items-center gap-2">
            <Badge variant="outline" className="gap-1.5 py-1 px-3 text-xs">
              <Sparkles className="size-3 text-primary" />
              <span>{t("hero.badge")}</span>
            </Badge>
          </div>

          {/* Headline */}
          <h1 className="font-heading text-4xl font-extrabold tracking-tight sm:text-5xl lg:text-6xl text-balance">
            {t("hero.title")}
          </h1>

          {/* Subtitle */}
          <p className="mt-6 text-lg leading-relaxed text-muted-foreground text-balance">
            {t("hero.subtitle")}
          </p>

          {/* Action CTAs */}
          <div className="mt-8 flex flex-wrap items-center justify-center gap-4">
            <Link
              href="#pricing"
              className={buttonVariants({ size: "lg", className: "gap-2" })}
            >
              <span>{t("hero.ctaPrimary")}</span>
              <ArrowRight className="size-4 rtl:rotate-180" />
            </Link>
            <a
              href="http://localhost:4200"
              target="_blank"
              rel="noopener noreferrer"
              className={buttonVariants({ variant: "outline", size: "lg" })}
            >
              {t("hero.ctaSecondary")}
            </a>
          </div>

          {/* Social Proof / Pillars */}
          <div className="mt-10 flex flex-wrap items-center justify-center gap-6 text-sm text-muted-foreground">
            <div className="flex items-center gap-1.5">
              <CheckCircle2 className="size-4 text-primary" />
              <span>{t("hero.tokenDriven")}</span>
            </div>
            <div className="flex items-center gap-1.5">
              <ShieldCheck className="size-4 text-primary" />
              <span>{t("hero.a11y")}</span>
            </div>
            <div className="flex items-center gap-1.5">
              <Zap className="size-4 text-primary" />
              <span>{t("hero.turbopack")}</span>
            </div>
          </div>
        </div>

        {/* Dashboard Preview Card */}
        <div className="mt-16 sm:mt-20">
          <Card className="mx-auto max-w-4xl border-border bg-card/60 shadow-xl backdrop-blur-sm">
            <CardHeader className="border-b border-border/60 pb-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="size-3 rounded-full bg-destructive/60" />
                  <div className="size-3 rounded-full bg-muted-foreground/30" />
                  <div className="size-3 rounded-full bg-primary/40" />
                  <span className="ml-2 font-mono text-xs text-muted-foreground">
                    saas.modular.local / dashboard
                  </span>
                </div>
                <Badge variant="secondary" className="text-xs">
                  Tenant: default
                </Badge>
              </div>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
                <div className="rounded-lg border border-border bg-background p-4">
                  <p className="text-xs font-medium text-muted-foreground">
                    Active Tenants
                  </p>
                  <p className="mt-1 font-heading text-2xl font-bold">1,248</p>
                  <p className="mt-1 text-xs text-primary">
                    +12% from last month
                  </p>
                </div>
                <div className="rounded-lg border border-border bg-background p-4">
                  <p className="text-xs font-medium text-muted-foreground">
                    Monthly Recurring
                  </p>
                  <p className="mt-1 font-heading text-2xl font-bold">
                    $48,250
                  </p>
                  <p className="mt-1 text-xs text-primary">+8.4% growth</p>
                </div>
                <div className="rounded-lg border border-border bg-background p-4">
                  <p className="text-xs font-medium text-muted-foreground">
                    API Latency (p99)
                  </p>
                  <p className="mt-1 font-heading text-2xl font-bold">14ms</p>
                  <p className="mt-1 text-xs text-muted-foreground">
                    Global Edge Cache
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </section>
  );
}
