import { Check } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

const TIERS = [
  {
    name: "Starter",
    id: "tier-starter",
    price: "$0",
    frequency: "/month",
    description:
      "For indie developers prototyping and testing multi-tenant apps.",
    features: [
      "Up to 3 isolated tenants",
      "Next.js App Router client",
      "Angular 19 admin portal",
      "Basic role-based permissions",
      "Community support",
    ],
    isPopular: false,
    ctaText: "Start Free",
    ctaVariant: "outline" as const,
  },
  {
    name: "Growth",
    id: "tier-growth",
    price: "$49",
    frequency: "/month",
    description:
      "For growing SaaS products with production workloads and SLA requirements.",
    features: [
      "Unlimited tenants",
      "Automated tenant database migrations",
      "Per-tenant custom CSS branding",
      "Full RFC 7807 problem details handling",
      "Priority email & chat support",
      "Audit logging & telemetry export",
    ],
    isPopular: true,
    ctaText: "Get Started",
    ctaVariant: "default" as const,
  },
  {
    name: "Enterprise",
    id: "tier-enterprise",
    price: "$199",
    frequency: "/month",
    description:
      "Dedicated infrastructure, compliance guarantees, and custom integrations.",
    features: [
      "Custom domain per tenant",
      "SSO & SAML 2.0 / OAuth2 federation",
      "Dedicated Redis cache cluster",
      "Custom SLA & 24/7 on-call engineer",
      "Architecture review consultations",
    ],
    isPopular: false,
    ctaText: "Contact Sales",
    ctaVariant: "outline" as const,
  },
];

export function PricingSection() {
  return (
    <section id="pricing" className="py-20 sm:py-28">
      <div className="container mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <Badge variant="secondary" className="mb-4">
            Transparent Pricing
          </Badge>
          <h2 className="font-heading text-3xl font-bold tracking-tight sm:text-4xl">
            Choose The Plan That Fits Your Growth
          </h2>
          <p className="mt-4 text-muted-foreground text-balance">
            Simple, transparent tiers designed to scale seamlessly alongside
            your application and tenant base.
          </p>
        </div>

        <div className="mt-16 grid grid-cols-1 gap-8 lg:grid-cols-3">
          {TIERS.map((tier) => (
            <Card
              key={tier.id}
              className={cn(
                "relative flex flex-col justify-between transition-all bg-card",
                tier.isPopular
                  ? "border-primary shadow-lg ring-1 ring-primary/20"
                  : "border-border hover:border-border/80"
              )}
            >
              {tier.isPopular && (
                <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                  <Badge
                    variant="default"
                    className="px-3 py-0.5 text-xs font-semibold"
                  >
                    Most Popular
                  </Badge>
                </div>
              )}

              <div>
                <CardHeader>
                  <CardTitle className="text-xl">{tier.name}</CardTitle>
                  <CardDescription className="min-h-10 text-xs leading-relaxed">
                    {tier.description}
                  </CardDescription>
                  <div className="mt-4 flex items-baseline gap-1">
                    <span className="font-heading text-4xl font-extrabold tracking-tight">
                      {tier.price}
                    </span>
                    <span className="text-sm font-medium text-muted-foreground">
                      {tier.frequency}
                    </span>
                  </div>
                </CardHeader>

                <CardContent>
                  <div className="space-y-3 pt-2">
                    <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
                      Included Capabilities
                    </p>
                    <ul className="space-y-2.5 text-sm">
                      {tier.features.map((feature) => (
                        <li key={feature} className="flex items-center gap-2">
                          <Check className="size-4 shrink-0 text-primary" />
                          <span>{feature}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                </CardContent>
              </div>

              <CardFooter className="pt-6">
                <Button
                  variant={tier.ctaVariant}
                  className="w-full justify-center"
                >
                  {tier.ctaText}
                </Button>
              </CardFooter>
            </Card>
          ))}
        </div>
      </div>
    </section>
  );
}
