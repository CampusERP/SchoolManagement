import { Bell } from "lucide-react";
import { cn } from "@/lib/utils";
import { toast } from "sonner";

interface NotificationBellProps {
  count?: number;
  onClick?: () => void;
}

export default function NotificationBell({
  count = 0,
  onClick,
}: NotificationBellProps) {
  const handleClick = () => {
    if (onClick) {
      onClick();
    } else {
      toast.info("Notifications feature coming soon!");
    }
  };

  return (
    <button
      type="button"
      onClick={handleClick}
      className="relative flex h-10 w-10 items-center justify-center rounded-[var(--border-radius)] text-[var(--color-text-secondary)] hover:bg-[var(--color-border)]/50 hover:text-[var(--color-text-primary)]"
    >
      <Bell className="h-5 w-5" />

      {count > 0 && (
        <span
          className={cn(
            "absolute -right-0.5 -top-0.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-[var(--color-danger)] px-1 text-[10px] font-bold leading-none text-white"
          )}
        >
          {count > 99 ? "99+" : count}
        </span>
      )}
    </button>
  );
}
