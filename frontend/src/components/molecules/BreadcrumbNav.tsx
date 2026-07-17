import { Link } from "react-router-dom";
import { ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";

interface BreadcrumbItem {
  label: string;
  href?: string;
}

interface BreadcrumbNavProps {
  items: BreadcrumbItem[];
  className?: string;
}

export default function BreadcrumbNav({
  items,
  className,
}: BreadcrumbNavProps) {
  return (
    <nav className={cn("flex items-center text-sm", className)}>
      {items.map((item, index) => {
        const isLast = index === items.length - 1;

        return (
          <span key={item.label} className="flex items-center">
            {index > 0 && (
              <ChevronRight className="mx-2 h-3.5 w-3.5 text-[var(--color-text-muted)]" />
            )}

            {item.href && !isLast ? (
              <Link
                to={item.href}
                className="font-medium text-[var(--color-text-secondary)] hover:text-[var(--color-primary)]"
              >
                {item.label}
              </Link>
            ) : (
              <span
                className={cn(
                  "font-medium",
                  isLast
                    ? "text-[var(--color-text-primary)]"
                    : "text-[var(--color-text-secondary)]"
                )}
              >
                {item.label}
              </span>
            )}
          </span>
        );
      })}
    </nav>
  );
}
