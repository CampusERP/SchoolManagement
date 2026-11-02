import { cn } from "@/lib/utils";

interface LogoProps {
  className?: string;
  alt?: string;
}

export default function Logo({ className, alt = "CampusERP" }: LogoProps) {
  return (
    <img
      src="/CampusERP_Education_Logo.png"
      alt={alt}
      className={cn("h-10 w-10 shrink-0 rounded-xl object-contain", className)}
    />
  );
}
