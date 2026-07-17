import type { ReactNode } from "react";
import { cn } from "@/lib/utils";
import StatCard from "@/components/molecules/StatCard";

interface AnalyticsCard {
  title: string;
  value: string | number;
  icon: ReactNode;
  trend?: { value: number; isPositive: boolean };
}

interface AnalyticsCardsGridProps {
  cards: AnalyticsCard[];
  className?: string;
}

export default function AnalyticsCardsGrid({
  cards,
  className,
}: AnalyticsCardsGridProps) {
  return (
    <div
      className={cn(
        "grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4",
        className
      )}
    >
      {cards.map((card, index) => (
        <StatCard
          key={index}
          title={card.title}
          value={card.value}
          icon={card.icon}
          trend={card.trend}
        />
      ))}
    </div>
  );
}
