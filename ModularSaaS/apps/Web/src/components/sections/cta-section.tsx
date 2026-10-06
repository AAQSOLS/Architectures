import { ContactDialog } from "@/components/contact-dialog";
import { Badge } from "@/components/ui/badge";

export function CtaSection() {
  return (
    <section id="inquiry" className="py-20 bg-muted/40 border-t border-border">
      <div className="container mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="rounded-2xl border border-border bg-card p-8 sm:p-12 lg:p-16 text-center shadow-lg">
          <Badge variant="secondary" className="mb-4">
            Ready To Ship
          </Badge>
          <h2 className="font-heading text-3xl font-bold tracking-tight sm:text-4xl text-balance">
            Start Building Your Next SaaS Platform Today
          </h2>
          <p className="mx-auto mt-4 max-w-2xl text-muted-foreground text-balance">
            Designed for developers and enterprise teams demanding clean
            separation of concerns, robust design tokens, and strict type
            safety.
          </p>

          <div className="mt-8 flex justify-center">
            <ContactDialog />
          </div>
        </div>
      </div>
    </section>
  );
}
