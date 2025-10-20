'use client';

import React, { useState } from 'react';
import VietnamMap from './VietnamMap';

interface MapOverlayProps {
  period: string;
  title: string;
  description: string;
}

const MapOverlay: React.FC<MapOverlayProps> = ({ period, title, description }) => {
  const [isVisible, setIsVisible] = useState(false);

  // Map different historical periods to different map styles/overlays
  const getMapStyle = (period: string) => {
    if (period.includes('1945')) {
      return 'sepia(100%) hue-rotate(30deg) saturate(150%)'; // Golden tone for independence period
    } else if (period.includes('1946')) {
      return 'sepia(80%) hue-rotate(200deg) saturate(120%)'; // Blue tone for diplomatic period
    }
    return 'sepia(60%) hue-rotate(0deg) saturate(100%)'; // Default historical tone
  };

  const getMapRegion = (period: string) => {
    // Different focus regions based on historical events
    if (period.includes('Nam Bộ') || period.includes('Sài Gòn')) {
      return 'Southern Vietnam';
    } else if (period.includes('Trùng Khánh') || period.includes('Hoa')) {
      return 'Northern Vietnam & China Border';
    }
    return 'Vietnam Territory';
  };

  return (
    <div className="map-overlay-container">
      <button
        onClick={() => setIsVisible(!isVisible)}
        className="map-toggle-btn"
        title="Xem Bản Đồ Lịch Sử"
      >
        🗺️ Bản Đồ
      </button>

      {isVisible && (
        <div className="map-overlay-modal">
          <div className="map-overlay-content">
            <div className="map-header">
              <h3>{title}</h3>
              <button 
                onClick={() => setIsVisible(false)}
                className="close-btn"
              >
                ✕
              </button>
            </div>
            
            <div className="map-container">
              <div className="historical-map">
                <VietnamMap period={period} />
              </div>
              
              <div className="map-description">
                <p>{description}</p>
                <div className="map-legend">
                  <h5>Kiểm Soát Lãnh Thổ ({period})</h5>
                  <div className="legend-items">
                    <div className="legend-item">
                      <div className="legend-color vietnam"></div>
                      <span>Việt Nam Dân Chủ Cộng Hòa</span>
                    </div>
                    <div className="legend-item">
                      <div className="legend-color french"></div>
                      <span>Lực Lượng Thực Dân Pháp</span>
                    </div>
                    <div className="legend-item">
                      <div className="legend-color chinese"></div>
                      <span>Quân Đội Trung Hoa Dân Quốc</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      <style jsx>{`
        .map-overlay-container {
          position: relative;
          display: inline-block;
        }

        .map-toggle-btn {
          background: linear-gradient(135deg, #8B4513, #D2691E);
          color: white;
          border: none;
          padding: 6px 12px;
          border-radius: 20px;
          cursor: pointer;
          font-size: 12px;
          font-weight: 500;
          transition: all 0.3s ease;
          box-shadow: 0 2px 4px rgba(0,0,0,0.2);
        }

        .map-toggle-btn:hover {
          background: linear-gradient(135deg, #A0522D, #F4A460);
          transform: translateY(-1px);
          box-shadow: 0 4px 8px rgba(0,0,0,0.3);
        }

        .map-overlay-modal {
          position: fixed;
          top: 0;
          left: 0;
          right: 0;
          bottom: 0;
          background: rgba(0, 0, 0, 0.8);
          display: flex;
          align-items: center;
          justify-content: center;
          z-index: 1000;
          animation: fadeIn 0.3s ease;
        }

        .map-overlay-content {
          background: white;
          border-radius: 12px;
          max-width: 90vw;
          max-height: 90vh;
          overflow: auto;
          box-shadow: 0 20px 40px rgba(0,0,0,0.3);
        }

        .map-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 20px;
          border-bottom: 1px solid #eee;
          background: linear-gradient(135deg, #8B4513, #D2691E);
          color: white;
          border-radius: 12px 12px 0 0;
        }

        .map-header h3 {
          margin: 0;
          font-size: 18px;
        }

        .close-btn {
          background: none;
          border: none;
          color: white;
          font-size: 20px;
          cursor: pointer;
          padding: 5px;
          border-radius: 50%;
          width: 30px;
          height: 30px;
          display: flex;
          align-items: center;
          justify-content: center;
        }

        .close-btn:hover {
          background: rgba(255,255,255,0.2);
        }

        .map-container {
          display: grid;
          grid-template-columns: 2fr 1fr;
          gap: 20px;
          padding: 20px;
        }

        .historical-map {
          background: linear-gradient(45deg, #f0e68c, #daa520);
          border-radius: 8px;
          padding: 20px;
          min-height: 300px;
          max-height: 70vh;
          max-width: 90vw;
          position: relative;
          overflow: auto;
          box-sizing: border-box;
        }

        .map-placeholder {
          width: 100%;
          height: 100%;
          display: flex;
          flex-direction: column;
          align-items: center;
          justify-content: center;
        }

        .map-region h4 {
          text-align: center;
          margin-bottom: 20px;
          color: #8B4513;
          font-size: 16px;
        }

        .territory-indicator {
          display: flex;
          flex-direction: column;
          gap: 10px;
          width: 100%;
        }

        .controlled-area {
          padding: 10px;
          border-radius: 6px;
          text-align: center;
          font-weight: 500;
          font-size: 14px;
        }

        .vietnam-controlled {
          background: rgba(255, 0, 0, 0.2);
          border: 2px solid #ff0000;
          color: #cc0000;
        }

        .french-controlled {
          background: rgba(0, 0, 255, 0.2);
          border: 2px solid #0000ff;
          color: #0000cc;
        }

        .chinese-controlled {
          background: rgba(255, 255, 0, 0.2);
          border: 2px solid #ffcc00;
          color: #cc9900;
        }

        .map-description {
          background: #f9f9f9;
          padding: 20px;
          border-radius: 8px;
        }

        .map-description p {
          margin-bottom: 20px;
          line-height: 1.6;
          color: #333;
        }

        .map-legend h5 {
          margin-bottom: 15px;
          color: #8B4513;
          border-bottom: 2px solid #D2691E;
          padding-bottom: 5px;
        }

        .legend-items {
          display: flex;
          flex-direction: column;
          gap: 8px;
        }

        .legend-item {
          display: flex;
          align-items: center;
          gap: 10px;
          font-size: 14px;
        }

        .legend-color {
          width: 20px;
          height: 20px;
          border-radius: 4px;
          border: 1px solid #ccc;
        }

        .legend-color.vietnam {
          background: rgba(255, 0, 0, 0.6);
        }

        .legend-color.french {
          background: rgba(0, 0, 255, 0.6);
        }

        .legend-color.chinese {
          background: rgba(255, 255, 0, 0.6);
        }

        @keyframes fadeIn {
          from { opacity: 0; }
          to { opacity: 1; }
        }

        @media (max-width: 768px) {
          .map-container {
            grid-template-columns: 1fr;
          }
          
          .map-overlay-content {
            margin: 10px;
          }
        }
      `}</style>
    </div>
  );
};

export default MapOverlay;