import * as Tooltip from "@radix-ui/react-tooltip";
import { cn } from "@/lib/utils";

type IconButtonSize = "sm" | "md";

export interface IconButtonProps {
  icon: React.ReactNode;
  tooltip?: string;
  onClick?: React.MouseEventHandler<HTMLButtonElement>;
  className?: string;
  size?: IconButtonSize;
  disabled?: boolean;
}

const sizeStyles: Record<IconButtonSize, string> = {
  sm: "h-8 w-8",
  md: "h-10 w-10",
};

export default function IconButton({
  icon,
  tooltip,
  onClick,
  className,
  size = "md",
  disabled = false,
}: IconButtonProps) {
  const button = (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      className={cn(
        "inline-flex items-center justify-center rounded-[var(--border-radius)] text-[var(--color-text-secondary)] transition-colors",
        "hover:bg-[var(--color-primary-light)] hover:text-[var(--color-primary)]",
        "disabled:pointer-events-none disabled:opacity-50",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--color-primary)] focus-visible:ring-offset-2",
        sizeStyles[size],
        className,
      )}
    >
      {icon}
    </button>
  );

  if (!tooltip) return button;

  return (
    <Tooltip.Provider>
      <Tooltip.Root>
        <Tooltip.Trigger asChild>{button}</Tooltip.Trigger>
        <Tooltip.Portal>
          <Tooltip.Content
            sideOffset={5}
            className="z-50 rounded-[var(--border-radius)] bg-[var(--color-text-primary)] px-3 py-1.5 text-xs text-white shadow-md animate-in fade-in-0"
          >
            {tooltip}
            <Tooltip.Arrow className="fill-[var(--color-text-primary)]" />
          </Tooltip.Content>
        </Tooltip.Portal>
      </Tooltip.Root>
    </Tooltip.Provider>
  );
}
