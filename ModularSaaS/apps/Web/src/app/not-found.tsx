import Link from "next/link";
import { ArrowLeft, FileQuestion } from "lucide-react";
import { buttonVariants } from "@/components/ui/button";

export default function NotFound() {
  return (
    <div className="container mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center px-4 text-center">
      <div className="flex size-14 items-center justify-center rounded-2xl bg-muted text-muted-foreground">
        <FileQuestion className="size-7" />
      </div>
      <h1 className="mt-6 font-heading text-3xl font-bold tracking-tight">
        Page Not Found
      </h1>
      <p className="mt-2 text-sm text-muted-foreground">
        The requested resource or page could not be located in this deployment.
      </p>
      <div className="mt-6">
        <Link href="/" className={buttonVariants({ className: "gap-2" })}>
          <ArrowLeft className="size-4" />
          <span>Return Home</span>
        </Link>
      </div>
    </div>
  );
}
