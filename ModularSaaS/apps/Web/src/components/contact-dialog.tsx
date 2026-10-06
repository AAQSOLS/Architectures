"use client";

import * as React from "react";
import { Send, CheckCircle2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";

export function ContactDialog({
  triggerButton,
}: {
  triggerButton?: React.ReactNode;
}) {
  const [open, setOpen] = React.useState(false);
  const [submitted, setSubmitted] = React.useState(false);
  const [loading, setLoading] = React.useState(false);
  const [formData, setFormData] = React.useState({
    name: "",
    email: "",
    company: "",
    message: "",
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    // Simulate async submission
    await new Promise((resolve) => setTimeout(resolve, 600));
    setLoading(false);
    setSubmitted(true);
  };

  const handleReset = () => {
    setSubmitted(false);
    setFormData({ name: "", email: "", company: "", message: "" });
    setOpen(false);
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger
        render={
          triggerButton ? (
            (triggerButton as React.ReactElement)
          ) : (
            <Button size="lg" className="gap-2">
              <span>Contact Sales</span>
              <Send className="size-4" />
            </Button>
          )
        }
      />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>
            {submitted ? "Message Received" : "Contact the Engineering Team"}
          </DialogTitle>
          <DialogDescription>
            {submitted
              ? "We have recorded your details and will get back to you within 24 hours."
              : "Share your architecture questions or enterprise inquiries with our team."}
          </DialogDescription>
        </DialogHeader>

        {submitted ? (
          <div className="py-6 text-center space-y-3">
            <div className="mx-auto flex size-12 items-center justify-center rounded-full bg-primary/10 text-primary">
              <CheckCircle2 className="size-6" />
            </div>
            <p className="text-sm font-medium">Thank you, {formData.name}!</p>
            <p className="text-xs text-muted-foreground">
              Confirmation sent to {formData.email}.
            </p>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4 py-2">
            <div className="space-y-1.5">
              <Label htmlFor="contact-name">Full Name</Label>
              <Input
                id="contact-name"
                placeholder="Jane Doe"
                required
                value={formData.name}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, name: e.target.value }))
                }
              />
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="contact-email">Work Email</Label>
              <Input
                id="contact-email"
                type="email"
                placeholder="jane@company.com"
                required
                value={formData.email}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, email: e.target.value }))
                }
              />
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="contact-company">Organization / Company</Label>
              <Input
                id="contact-company"
                placeholder="Acme Corp"
                value={formData.company}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, company: e.target.value }))
                }
              />
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="contact-message">Inquiry Details</Label>
              <Textarea
                id="contact-message"
                placeholder="Describe your use case or timeline..."
                rows={3}
                required
                value={formData.message}
                onChange={(e) =>
                  setFormData((prev) => ({ ...prev, message: e.target.value }))
                }
              />
            </div>

            <DialogFooter className="pt-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => setOpen(false)}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={loading}>
                {loading ? "Sending..." : "Submit Inquiry"}
              </Button>
            </DialogFooter>
          </form>
        )}

        {submitted && (
          <DialogFooter>
            <Button className="w-full" onClick={handleReset}>
              Close
            </Button>
          </DialogFooter>
        )}
      </DialogContent>
    </Dialog>
  );
}
