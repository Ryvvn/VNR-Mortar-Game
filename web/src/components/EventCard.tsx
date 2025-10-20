"use client";

import React from "react";
import { TimelineItem } from "@/data/timeline";

// Local cache for Wikipedia thumbnails to avoid repeated requests
const wikiCache = new Map<string, string | null>();

function useWikiThumb(query: string | undefined, lang: "vi" | "en" = "vi") {
  const [src, setSrc] = React.useState<string | null | undefined>(undefined);
  React.useEffect(() => {
    const q = (query || "").trim();
    if (!q) { setSrc(null); return; }
    const key = `${lang}:${q}`;
    if (wikiCache.has(key)) { setSrc(wikiCache.get(key)); return; }
    let cancelled = false;
    (async () => {
      const encode = (s: string) => encodeURIComponent(s.replace(/\s*\([^)]+\)/g, ""));
      try {
        const res = await fetch(`https://${lang}.wikipedia.org/api/rest_v1/page/summary/${encode(q)}`);
        if (res.ok) {
          const data = await res.json();
          const thumb = data?.thumbnail?.source || null;
          if (!cancelled) { wikiCache.set(key, thumb); setSrc(thumb); }
          return;
        }
      } catch (e) {}
      if (lang !== "en") {
        try {
          const res2 = await fetch(`https://en.wikipedia.org/api/rest_v1/page/summary/${encode(q)}`);
          const data2 = res2.ok ? await res2.json() : null;
          const thumb2 = data2?.thumbnail?.source || null;
          if (!cancelled) { wikiCache.set(key, thumb2); setSrc(thumb2); }
        } catch (e) {
          if (!cancelled) { wikiCache.set(key, null); setSrc(null); }
        }
      } else {
        if (!cancelled) { wikiCache.set(key, null); setSrc(null); }
      }
    })();
    return () => { cancelled = true; };
  }, [query, lang]);
  return src; // undefined=loading, string|null=loaded
}

interface EventCardProps {
  item: TimelineItem;
  index: number;
  position?: 'left' | 'right';
}

export default function EventCard({ item, index, position = 'left' }: EventCardProps) {
  // Prefer explicit imageQuery; otherwise try cardTitle
  const thumb = useWikiThumb(item.imageQuery ?? item.cardTitle, "vi");
  const details = Array.isArray(item.cardDetailedText)
    ? item.cardDetailedText
    : item.cardDetailedText
    ? [item.cardDetailedText]
    : [];

  // Use summary text if available, otherwise fall back to detailed text
  const summaryText = item.cardSummaryText || (details.length > 0 ? details[0] : "");

  return (
    <article 
      className="group relative w-full max-w-md mx-auto"
      style={{ animationDelay: `${index * 100}ms` }}
    >
      {/* Main Card Container */}
      <div className="event-card">
        {/* Decorative Elements */}
        <div className="card-decoration" />
        <div className="card-glow" />
        <div className="card-corner-accent" />
        
        {/* Date Badge */}
        <div className="date-badge">
          <div className="date-icon">📅</div>
          <span className="date-text">{item.title}</span>
          <div className="badge-shine" />
        </div>

        {/* Image Container */}
        <div className="image-container">
          {thumb === undefined ? (
            <div className="image-skeleton" aria-hidden />
          ) : thumb ? (
            <img 
              src={thumb} 
              alt={item.cardTitle} 
              className="event-image" 
              loading="lazy" 
            />
          ) : (
            <div className="image-placeholder">
              <div className="placeholder-icon">🖼️</div>
              <span className="placeholder-text">Không có hình ảnh</span>
            </div>
          )}
        </div>

        {/* Content Container */}
        <div className="card-content">
          {/* Header */}
          <header className="card-header">
            <h3 className="card-title">{item.cardTitle}</h3>
            {item.cardSubtitle && (
              <div className="card-subtitle-container">
                <span className="card-subtitle">{item.cardSubtitle}</span>
              </div>
            )}
          </header>

          {/* Body */}
          <div className="card-body">
            {/* Summary text shown by default */}
            <div className="card-summary">
              <p className="card-text">{summaryText}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Detailed Popup - positioned on opposite side */}
      {details.length > 0 && (
        <div className={`detail-popup ${position === 'left' ? 'popup-right' : 'popup-left'}`}>
          <div className="popup-content">
            <div className="popup-header">
              <h4 className="popup-title">{item.cardTitle}</h4>
              {item.cardSubtitle && (
                <span className="popup-subtitle">{item.cardSubtitle}</span>
              )}
            </div>
            <div className="popup-body">
              {details.map((detail: string, i: React.Key) => (
                <p key={i} className="popup-text">
                  {detail}
                </p>
              ))}
            </div>
            <div className="popup-arrow" />
          </div>
        </div>
      )}

      <style jsx>{`
        .event-card {
          position: relative;
          background: linear-gradient(145deg, #ffffff 0%, #f8fafc 100%);
          border: 1px solid rgba(226, 232, 240, 0.8);
          border-radius: 24px;
          padding: 0;
          box-shadow: 
            0 4px 6px -1px rgba(0, 0, 0, 0.1), 
            0 2px 4px -1px rgba(0, 0, 0, 0.06),
            0 0 0 1px rgba(255, 255, 255, 0.05);
          transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
          backdrop-filter: blur(10px);
          overflow: hidden;
          animation: slideInUp 0.6s cubic-bezier(0.4, 0, 0.2, 1) both;
        }

        @keyframes slideInUp {
          from {
            opacity: 0;
            transform: translateY(30px) scale(0.95);
          }
          to {
            opacity: 1;
            transform: translateY(0) scale(1);
          }
        }

        .group:hover .event-card {
          transform: translateY(-8px) scale(1.02);
          box-shadow: 
            0 20px 25px -5px rgba(0, 0, 0, 0.1), 
            0 10px 10px -5px rgba(0, 0, 0, 0.04),
            0 0 0 1px rgba(220, 38, 38, 0.1);
          border-color: rgba(220, 38, 38, 0.2);
        }

        .card-decoration {
          position: absolute;
          top: 0;
          left: 0;
          right: 0;
          height: 6px;
          background: linear-gradient(90deg, #dc2626 0%, #fbbf24 50%, #dc2626 100%);
          border-radius: 24px 24px 0 0;
          transition: height 0.3s ease-out;
        }

        .group:hover .card-decoration {
          height: 8px;
        }

        .card-glow {
          position: absolute;
          top: -50%;
          left: -50%;
          width: 200%;
          height: 200%;
          background: linear-gradient(45deg, transparent, rgba(255,255,255,0.1), transparent);
          transform: rotate(45deg);
          transition: all 0.6s ease-out;
          opacity: 0;
          pointer-events: none;
        }

        .group:hover .card-glow {
          opacity: 1;
          top: -100%;
          left: -100%;
        }

        .card-corner-accent {
          position: absolute;
          top: 0;
          right: 0;
          width: 60px;
          height: 60px;
          background: linear-gradient(135deg, rgba(220, 38, 38, 0.1) 0%, rgba(251, 191, 36, 0.1) 100%);
          border-radius: 0 24px 0 60px;
          opacity: 0;
          transition: all 0.4s ease-out;
        }

        .group:hover .card-corner-accent {
          opacity: 1;
          transform: scale(1.1);
        }

        .date-badge {
          position: absolute;
          top: 20px;
          right: 20px;
          display: flex;
          align-items: center;
          gap: 8px;
          background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
          color: white;
          padding: 8px 16px;
          border-radius: 20px;
          font-size: 0.875rem;
          font-weight: 600;
          box-shadow: 0 4px 8px rgba(220, 38, 38, 0.3);
          z-index: 20;
          transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
          overflow: hidden;
        }

        .group:hover .date-badge {
          transform: scale(1.05);
          box-shadow: 0 6px 12px rgba(220, 38, 38, 0.4);
        }

        .badge-shine {
          position: absolute;
          top: 0;
          left: -100%;
          width: 100%;
          height: 100%;
          background: linear-gradient(90deg, transparent, rgba(255,255,255,0.4), transparent);
          transition: left 0.6s ease-out;
        }

        .group:hover .badge-shine {
          left: 100%;
        }

        .date-icon {
          font-size: 1rem;
        }

        .date-text {
          white-space: nowrap;
        }

        .image-container {
          position: relative;
          width: 100%;
          height: 200px;
          margin-top: 6px;
          border-radius: 20px 20px 0 0;
          overflow: hidden;
        }

        .event-image {
          width: 100%;
          height: 100%;
          object-fit: contain;
          object-position: center;
          background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
          transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
        }

        .group:hover .event-image {
          transform: scale(1.1);
        }

        .image-placeholder {
          width: 100%;
          height: 100%;
          background: linear-gradient(135deg, #f1f5f9 0%, #e2e8f0 100%);
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
          color: #94a3b8;
          gap: 8px;
        }

        .placeholder-icon {
          font-size: 2rem;
          opacity: 0.7;
        }

        .placeholder-text {
          font-size: 0.875rem;
          font-weight: 500;
        }

        .image-skeleton {
          width: 100%;
          height: 100%;
          background: linear-gradient(90deg, #f1f5f9, #e2e8f0, #f1f5f9);
          background-size: 200% 100%;
          animation: shimmer 1.5s ease-in-out infinite;
        }

        @keyframes shimmer {
          0% { background-position: 200% 0; }
          100% { background-position: -200% 0; }
        }

        .card-content {
          padding: 24px;
        }

        .card-header {
          margin-bottom: 16px;
        }

        .card-title {
          font-size: 1.375rem;
          font-weight: 800;
          color: #0f172a;
          line-height: 1.3;
          margin-bottom: 12px;
          transition: color 0.3s ease-out;
          letter-spacing: -0.025em;
          text-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
        }

        .group:hover .card-title {
          color: #dc2626;
          text-shadow: 0 2px 4px rgba(220, 38, 38, 0.1);
        }

        .card-subtitle-container {
          display: inline-block;
          background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
          padding: 8px 16px;
          border-radius: 16px;
          box-shadow: 0 2px 4px rgba(251, 191, 36, 0.2);
          position: relative;
          overflow: hidden;
        }

        .card-subtitle-container::before {
          content: '';
          position: absolute;
          top: 0;
          left: -100%;
          width: 100%;
          height: 100%;
          background: linear-gradient(90deg, transparent, rgba(255,255,255,0.3), transparent);
          transition: left 0.5s ease-out;
        }

        .group:hover .card-subtitle-container::before {
          left: 100%;
        }

        .card-subtitle {
          font-size: 0.875rem;
          font-weight: 700;
          color: #92400e;
          text-transform: uppercase;
          letter-spacing: 0.05em;
          position: relative;
          z-index: 1;
        }

        .card-body {
          margin-bottom: 20px;
          position: relative;
        }

        .card-summary {
          opacity: 1;
          transition: opacity 0.3s ease-out;
        }

        .card-text {
          color: #374151;
          font-size: 1rem;
          line-height: 1.75;
          margin-bottom: 12px;
          transition: color 0.3s ease-out;
          font-weight: 400;
          letter-spacing: 0.01em;
        }

        .card-text:last-child {
          margin-bottom: 0;
        }

        .group:hover .card-text {
          color: #1f2937;
        }

        /* Detailed Popup Styles */
        .detail-popup {
          position: absolute;
          top: 50%;
          transform: translateY(-50%);
          width: 350px;
          max-width: 90vw;
          opacity: 0;
          visibility: hidden;
          transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
          transition-delay: 0.1s;
          z-index: 1000;
          pointer-events: none;
        }

        .popup-right {
          left: calc(100% + 20px);
        }

        .popup-left {
          right: calc(100% + 20px);
        }

        /* Create hover bridge between card and popup */
        .detail-popup::before {
          content: '';
          position: absolute;
          top: 0;
          bottom: 0;
          width: 20px;
          background: transparent;
          z-index: -1;
        }

        .popup-right::before {
          right: 100%;
        }

        .popup-left::before {
          left: 100%;
        }

        .group:hover .detail-popup,
        .detail-popup:hover {
          opacity: 1;
          visibility: visible;
          pointer-events: auto;
          transition-delay: 0s;
        }

        .popup-content {
          background: linear-gradient(145deg, #ffffff 0%, #f8fafc 100%);
          border: 1px solid rgba(226, 232, 240, 0.8);
          border-radius: 16px;
          padding: 24px;
          box-shadow: 
            0 20px 25px -5px rgba(0, 0, 0, 0.1),
            0 10px 10px -5px rgba(0, 0, 0, 0.04),
            0 0 0 1px rgba(255, 255, 255, 0.05);
          backdrop-filter: blur(10px);
          position: relative;
        }

        .popup-header {
          margin-bottom: 16px;
          padding-bottom: 12px;
          border-bottom: 1px solid rgba(226, 232, 240, 0.6);
        }

        .popup-title {
          font-size: 1.25rem;
          font-weight: 700;
          color: #1f2937;
          margin: 0 0 8px 0;
          line-height: 1.4;
        }

        .popup-subtitle {
          font-size: 0.875rem;
          font-weight: 600;
          color: #92400e;
          text-transform: uppercase;
          letter-spacing: 0.05em;
        }

        .popup-body {
          max-height: 300px;
          overflow-y: auto;
          scrollbar-width: thin;
          scrollbar-color: rgba(156, 163, 175, 0.5) transparent;
        }

        .popup-body::-webkit-scrollbar {
          width: 6px;
        }

        .popup-body::-webkit-scrollbar-track {
          background: transparent;
        }

        .popup-body::-webkit-scrollbar-thumb {
          background: rgba(156, 163, 175, 0.5);
          border-radius: 3px;
        }

        .popup-body::-webkit-scrollbar-thumb:hover {
          background: rgba(156, 163, 175, 0.7);
        }

        .popup-text {
          color: #374151;
          font-size: 0.95rem;
          line-height: 1.7;
          margin-bottom: 12px;
          font-weight: 400;
          letter-spacing: 0.01em;
        }

        .popup-text:last-child {
          margin-bottom: 0;
        }

        .popup-arrow {
          position: absolute;
          top: 50%;
          transform: translateY(-50%);
          width: 0;
          height: 0;
        }

        .popup-right .popup-arrow {
          left: -8px;
          border-top: 8px solid transparent;
          border-bottom: 8px solid transparent;
          border-right: 8px solid #ffffff;
        }

        .popup-left .popup-arrow {
          right: -8px;
          border-top: 8px solid transparent;
          border-bottom: 8px solid transparent;
          border-left: 8px solid #ffffff;
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
          .detail-popup {
            width: 280px;
          }
          
          .popup-right {
            left: calc(100% + 10px);
          }
          
          .popup-left {
            right: calc(100% + 10px);
          }
        }

        .card-footer {
          border-top: 1px solid rgba(226, 232, 240, 0.8);
          padding-top: 16px;
          margin-top: 20px;
        }

        .card-link {
          display: inline-flex;
          align-items: center;
          gap: 8px;
          color: #dc2626;
          font-size: 0.9375rem;
          font-weight: 700;
          text-decoration: none;
          transition: all 0.3s ease-out;
          padding: 8px 16px;
          border-radius: 12px;
          background: linear-gradient(135deg, rgba(220, 38, 38, 0.05) 0%, rgba(220, 38, 38, 0.1) 100%);
          border: 1px solid rgba(220, 38, 38, 0.2);
          position: relative;
          overflow: hidden;
        }

        .card-link::before {
          content: '';
          position: absolute;
          top: 0;
          left: -100%;
          width: 100%;
          height: 100%;
          background: linear-gradient(90deg, transparent, rgba(220, 38, 38, 0.1), transparent);
          transition: left 0.4s ease-out;
        }

        .card-link:hover::before {
          left: 100%;
        }

        .card-link:hover {
          color: #ffffff;
          background: linear-gradient(135deg, #dc2626 0%, #b91c1c 100%);
          border-color: #dc2626;
          transform: translateY(-2px);
          box-shadow: 0 4px 8px rgba(220, 38, 38, 0.3);
        }

        .link-icon {
          width: 16px;
          height: 16px;
          transition: transform 0.3s ease-out;
          position: relative;
          z-index: 1;
        }

        .card-link:hover .link-icon {
          transform: scale(1.1) translateX(2px);
        }
      `}</style>
    </article>
  );
}