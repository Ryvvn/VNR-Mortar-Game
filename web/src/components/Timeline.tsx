"use client";

import React from "react";
import { Chrono } from "react-chrono";
import { timelineItems } from "@/data/timeline";

export default function Timeline() {
  const [mounted, setMounted] = React.useState(false);
  React.useEffect(() => setMounted(true), []);

  // Hide private source URLs from the UI
  const itemsSafe = React.useMemo(() => timelineItems.map(({ url, ...rest }) => rest), []);

  return (
    <div className="w-full">
      <div className="rounded-lg">
        {mounted ? (
          <Chrono
            items={itemsSafe}
            mode="VERTICAL"
            scrollable={{ scrollbar: true }}
            // removed hideControls (deprecated) — use disableToolbar instead
            disableToolbar
            highlightCardsOnHover
            borderLessCards
            cardHeight={180}
            focusActiveItemOnLoad
            fontSizes={{
              title: "16px",
              cardTitle: "18px",
              cardSubtitle: "12px",
              cardText: "14px",
            }}
            timelinePointShape="circle"
            timelinePointDimension={10}
            lineWidth={3}
            theme={{
              primary: "#b91c1c", // brand accent red
              secondary: "#d1d5db", // soft gray track
              cardBgColor: "#ffffff",
              textColor: "#1f2937",
              cardTitleColor: "#111827",
              titleColor: "#111827",
              timelineBgColor: "transparent",
              shadowColor: "#00000014",
            }}
          />
        ) : (
          <div className="p-8 text-center text-sm text-gray-500">Đang tải dòng thời gian…</div>
        )}
      </div>
    </div>
  );
}