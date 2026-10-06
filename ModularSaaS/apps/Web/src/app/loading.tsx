import { Skeleton } from "@/components/ui/skeleton";

export default function Loading() {
  return (
    <div className="container mx-auto max-w-7xl px-4 py-20 space-y-12">
      <div className="mx-auto max-w-2xl text-center space-y-4">
        <Skeleton className="mx-auto h-6 w-32 rounded-full" />
        <Skeleton className="mx-auto h-12 w-3/4 rounded-xl" />
        <Skeleton className="mx-auto h-5 w-1/2 rounded-md" />
      </div>

      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {Array.from({ length: 6 }).map((_, i) => (
          <div
            key={i}
            className="rounded-xl border border-border p-6 space-y-4"
          >
            <Skeleton className="h-10 w-10 rounded-lg" />
            <Skeleton className="h-6 w-3/4 rounded-md" />
            <Skeleton className="h-4 w-full rounded-md" />
            <Skeleton className="h-4 w-2/3 rounded-md" />
          </div>
        ))}
      </div>
    </div>
  );
}
