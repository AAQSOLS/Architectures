import { HeroSection } from "@/components/sections/hero-section";
import { FeaturesSection } from "@/components/sections/features-section";
import { PricingSection } from "@/components/sections/pricing-section";
import { CtaSection } from "@/components/sections/cta-section";

export default function HomePage() {
  return (
    <div className="flex flex-col">
      <HeroSection />
      <FeaturesSection />
      <PricingSection />
      <CtaSection />
    </div>
  );
}
