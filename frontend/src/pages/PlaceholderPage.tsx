import { Construction } from "lucide-react";

interface PlaceholderPageProps {
  title?: string;
  description?: string;
}

export default function PlaceholderPage({
  title = "Coming Soon",
  description = "This page is under development.",
}: PlaceholderPageProps) {
  return (
    <div className="flex flex-col items-center justify-center py-20 text-center">
      <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-2xl bg-[var(--color-primary-light)] text-[var(--color-primary)]">
        <Construction className="h-8 w-8" />
      </div>
      <h2 className="text-xl font-semibold text-[var(--color-text-primary)]">{title}</h2>
      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{description}</p>
    </div>
  );
}
