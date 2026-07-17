import * as AvatarPrimitive from "@radix-ui/react-avatar";
import { cn } from "@/lib/utils";

type AvatarSize = "sm" | "md" | "lg";

export interface AvatarProps {
  src?: string;
  fallback?: string;
  size?: AvatarSize;
  className?: string;
}

const sizeStyles: Record<AvatarSize, string> = {
  sm: "h-8 w-8 text-xs",
  md: "h-10 w-10 text-sm",
  lg: "h-12 w-12 text-base",
};

export default function Avatar({
  src,
  fallback,
  size = "md",
  className,
}: AvatarProps) {
  return (
    <AvatarPrimitive.Root
      className={cn(
        "relative inline-flex shrink-0 overflow-hidden rounded-full",
        sizeStyles[size],
        className,
      )}
    >
      <AvatarPrimitive.Image
        src={src}
        alt={fallback ?? "Avatar"}
        className="h-full w-full object-cover"
      />
      <AvatarPrimitive.Fallback
        className="flex h-full w-full items-center justify-center rounded-full bg-[var(--color-primary-light)] text-[var(--color-primary)] font-medium"
        delayMs={src ? 600 : 0}
      >
        {fallback ?? "?"}
      </AvatarPrimitive.Fallback>
    </AvatarPrimitive.Root>
  );
}
