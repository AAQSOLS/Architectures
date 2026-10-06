import { Layers, Palette, Shield, Gauge, Cpu, GitBranch } from "lucide-react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

const FEATURES = [
  {
    icon: Palette,
    title: "Completely Token-Driven",
    description:
      "All styling resolves through semantic CSS variables. Changing root tokens updates the entire application theme instantly without editing components.",
    tag: "Design System",
  },
  {
    icon: Shield,
    title: "Multi-Tenant Isolation",
    description:
      "Built-in support for tenant headers, scoped local storage, and per-tenant dynamic brand color schemes matching the backend tenant resolution.",
    tag: "Security",
  },
  {
    icon: Gauge,
    title: "Server Components First",
    description:
      "Leverages React 19 Server Components for instant page rendering, zero client bundle overhead for static UI, and fine-grained client interactivity.",
    tag: "Performance",
  },
  {
    icon: Layers,
    title: "Official shadcn Primitives",
    description:
      "Built on top of Base UI and Radix accessibility primitives. Fully compliant with WCAG keyboard navigation and screen reader semantics.",
    tag: "Accessibility",
  },
  {
    icon: Cpu,
    title: "End-to-End Type Safety",
    description:
      "Strict TypeScript compiler flags, exact optional property types, and typed contracts matching ASP.NET Core backend endpoints.",
    tag: "Architecture",
  },
  {
    icon: GitBranch,
    title: "Zero-Tolerance Guardrails",
    description:
      "Automated ESLint flat configuration, SonarJS static analysis, Prettier formatting, and husky commit verification in the CI pipeline.",
    tag: "Quality",
  },
];

export function FeaturesSection() {
  return (
    <section
      id="features"
      className="py-20 sm:py-28 bg-muted/20 border-y border-border"
    >
      <div className="container mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <Badge variant="secondary" className="mb-4">
            Architecture Highlights
          </Badge>
          <h2 className="font-heading text-3xl font-bold tracking-tight sm:text-4xl">
            Engineered For Enterprise SaaS Workloads
          </h2>
          <p className="mt-4 text-muted-foreground text-balance">
            Every architectural decision prioritizes maintainability, strict
            theming isolation, and seamless full-stack cohesion.
          </p>
        </div>

        <div className="mt-16 grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {FEATURES.map((feature) => {
            const Icon = feature.icon;
            return (
              <Card
                key={feature.title}
                className="transition-all hover:shadow-md hover:border-primary/40 bg-card"
              >
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
                      <Icon className="size-5" />
                    </div>
                    <Badge variant="outline" className="text-[10px]">
                      {feature.tag}
                    </Badge>
                  </div>
                  <CardTitle className="mt-4 text-lg">
                    {feature.title}
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <CardDescription className="leading-relaxed">
                    {feature.description}
                  </CardDescription>
                </CardContent>
              </Card>
            );
          })}
        </div>
      </div>
    </section>
  );
}
