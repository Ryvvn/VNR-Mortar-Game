"use client";

import React from "react";
import { timelineItems } from "@/data/timeline";
import EventCard from "./EventCard";

type Props = {
  startSide?: "left" | "right"; // choose which side the first card appears
};

// An interactive grid-based timeline with enhanced card layouts.
export default function InteractiveTimeline({ startSide = "left" }: Props) {
  return (
    <div className="timeline-container">
      {/* Timeline Header */}
      <div className="timeline-header">
        <div className="header-decoration" />
        <h2 className="timeline-title">Lịch sử Đảng Cộng sản Việt Nam</h2>
        <p className="timeline-subtitle">
          Hành trình phát triển và những cột mốc quan trọng
        </p>
      </div>

      {/* Timeline Tree */}
      <div className="timeline-tree">
        <div className="timeline-line" />
        {timelineItems.map((item, index) => {
          const isLeft = startSide === "left" ? index % 2 === 0 : index % 2 === 1;
          return (
            <div key={index} className={`timeline-item ${isLeft ? 'left' : 'right'}`}>
              <div className="timeline-connector">
                <div className="connector-dot" />
                <div className="connector-line" />
              </div>
              <div className="card-wrapper">
                <EventCard 
                  item={item} 
                  index={index}
                  position={isLeft ? 'left' : 'right'}
                />
              </div>
            </div>
          );
        })}
      </div>
      <style jsx>{`
        .timeline-container {
          width: 100%;
          max-width: 1400px;
          margin: 0 auto;
          padding: 2rem 1rem;
        }

        .timeline-header {
          text-align: center;
          margin-bottom: 4rem;
          position: relative;
        }

        .header-decoration {
          width: 100px;
          height: 4px;
          background: linear-gradient(90deg, #dc2626 0%, #fbbf24 50%, #dc2626 100%);
          margin: 0 auto 2rem;
          border-radius: 2px;
          animation: pulse 2s ease-in-out infinite;
        }

        @keyframes pulse {
          0%, 100% { opacity: 1; transform: scaleX(1); }
          50% { opacity: 0.7; transform: scaleX(1.1); }
        }

        .timeline-title {
          font-size: 2.5rem;
          font-weight: 800;
          background: linear-gradient(135deg, #dc2626 0%, #b91c1c 50%, #991b1b 100%);
          -webkit-background-clip: text;
          -webkit-text-fill-color: transparent;
          background-clip: text;
          margin-bottom: 1rem;
          line-height: 1.2;
        }

        .timeline-subtitle {
          font-size: 1.125rem;
          color: #64748b;
          font-weight: 500;
          max-width: 600px;
          margin: 0 auto;
          line-height: 1.6;
        }

        .timeline-tree {
          position: relative;
          max-width: 1200px;
          margin: 0 auto;
        }

        .timeline-line {
          position: absolute;
          left: 50%;
          top: 0;
          bottom: 0;
          width: 4px;
          background: linear-gradient(180deg, #dc2626 0%, #fbbf24 50%, #dc2626 100%);
          transform: translateX(-50%);
          border-radius: 2px;
          box-shadow: 0 0 20px rgba(220, 38, 38, 0.3);
        }

        .timeline-item {
          position: relative;
          display: flex;
          align-items: center;
          margin-bottom: 4rem;
          min-height: 200px;
        }

        .timeline-item.left {
          flex-direction: row;
        }

        .timeline-item.right {
          flex-direction: row-reverse;
        }

        .timeline-connector {
          position: absolute;
          left: 50%;
          top: 50%;
          transform: translate(-50%, -50%);
          z-index: 10;
        }

        .connector-dot {
          width: 20px;
          height: 20px;
          background: linear-gradient(135deg, #dc2626 0%, #fbbf24 100%);
          border: 4px solid white;
          border-radius: 50%;
          box-shadow: 0 4px 12px rgba(220, 38, 38, 0.4);
          animation: pulse-dot 2s ease-in-out infinite;
        }

        @keyframes pulse-dot {
          0%, 100% { transform: scale(1); box-shadow: 0 4px 12px rgba(220, 38, 38, 0.4); }
          50% { transform: scale(1.1); box-shadow: 0 6px 20px rgba(220, 38, 38, 0.6); }
        }

        .connector-line {
          position: absolute;
          top: 50%;
          width: 60px;
          height: 2px;
          background: linear-gradient(90deg, #dc2626 0%, #fbbf24 100%);
          transform: translateY(-50%);
        }

        .timeline-item.left .connector-line {
          left: 20px;
        }

        .timeline-item.right .connector-line {
          right: 20px;
        }

        .card-wrapper {
          flex: 1;
          max-width: 450px;
          margin: 0 80px;
        }

        .timeline-item.left .card-wrapper {
          margin-right: 80px;
          margin-left: 0;
        }

        .timeline-item.right .card-wrapper {
          margin-left: 80px;
          margin-right: 0;
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
          .timeline-container {
            padding: 1rem 0.5rem;
          }

          .timeline-header {
            margin-bottom: 2rem;
          }

          .timeline-title {
            font-size: 2rem;
          }

          .timeline-subtitle {
            font-size: 1rem;
          }

          .timeline-tree {
            max-width: 100%;
          }

          .timeline-line {
            left: 30px;
          }

          .timeline-item {
            flex-direction: column !important;
            align-items: flex-start;
            margin-bottom: 3rem;
            padding-left: 60px;
          }

          .timeline-connector {
            left: 30px;
            top: 20px;
          }

          .connector-line {
            width: 30px;
            left: 20px !important;
            right: auto !important;
          }

          .card-wrapper {
            max-width: 100%;
            margin: 0 !important;
            width: 100%;
          }
        }
      `}</style>
    </div>
  );
}