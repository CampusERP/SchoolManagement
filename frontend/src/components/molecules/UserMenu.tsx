import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuLabel,
} from "@radix-ui/react-dropdown-menu";
import { LogOut, User } from "lucide-react";
import { cn } from "@/lib/utils";

interface UserMenuProps {
  user: {
    firstName: string;
    lastName: string;
    email: string;
    role: string;
  };
  onLogout: () => void;
}

export default function UserMenu({ user, onLogout }: UserMenuProps) {
  const initials = `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button
          type="button"
          className="flex items-center gap-2 rounded-[var(--border-radius)] px-2 py-1.5 outline-none hover:bg-[var(--color-border)]/50"
        >
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-[var(--color-primary)] text-xs font-semibold text-white">
            {initials}
          </div>
          <span className="hidden text-sm font-medium text-[var(--color-text-primary)] md:block">
            {user.firstName} {user.lastName}
          </span>
        </button>
      </DropdownMenuTrigger>

      <DropdownMenuContent
        align="end"
        sideOffset={8}
        className="min-w-56 rounded-[var(--border-radius)] border border-[var(--color-border)] bg-[var(--color-surface-card)] p-1.5 shadow-[var(--shadow-elevated)]"
      >
        <DropdownMenuLabel className="px-2.5 py-2">
          <p className="text-sm font-medium text-[var(--color-text-primary)]">
            {user.firstName} {user.lastName}
          </p>
          <p className="mt-0.5 text-xs text-[var(--color-text-secondary)]">
            {user.email}
          </p>
          <span className="mt-1 inline-block rounded-full bg-[var(--color-primary-light)] px-2 py-0.5 text-xs font-medium text-[var(--color-primary)]">
            {user.role}
          </span>
        </DropdownMenuLabel>

        <DropdownMenuSeparator className="my-1 h-px bg-[var(--color-border)]" />

        <DropdownMenuItem
          onClick={() => {}}
          className="flex cursor-pointer items-center gap-2 rounded-[6px] px-2.5 py-2 text-sm text-[var(--color-text-secondary)] outline-none hover:bg-[var(--color-border)]/50 hover:text-[var(--color-text-primary)]"
        >
          <User className="h-4 w-4" />
          Profile
        </DropdownMenuItem>

        <DropdownMenuSeparator className="my-1 h-px bg-[var(--color-border)]" />

        <DropdownMenuItem
          onClick={onLogout}
          className="flex cursor-pointer items-center gap-2 rounded-[6px] px-2.5 py-2 text-sm text-[var(--color-danger)] outline-none hover:bg-red-50"
        >
          <LogOut className="h-4 w-4" />
          Log out
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
