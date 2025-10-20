'use client';

import React, { useState } from 'react';

interface HistoricalFigure {
  name: string;
  role: string;
  period: string;
  description: string;
  imageUrl?: string;
  nationality: string;
}

interface PortraitGalleryProps {
  period: string;
  relatedFigures: string[];
}

const PortraitGallery: React.FC<PortraitGalleryProps> = ({ period, relatedFigures }) => {
  const [isVisible, setIsVisible] = useState(false);
  const [selectedFigure, setSelectedFigure] = useState<HistoricalFigure | null>(null);

  // Historical figures database
  const historicalFigures: { [key: string]: HistoricalFigure } = {
    'ho-chi-minh': {
      name: 'Hồ Chí Minh',
      role: 'Chủ tịch nước Việt Nam Dân chủ Cộng hòa',
      period: '1945-1969',
      nationality: 'Việt Nam',
      description: 'Lãnh tụ cách mạng và Chủ tịch nước đầu tiên của Việt Nam. Lãnh đạo phong trào độc lập chống thực dân Pháp và sau đó chống can thiệp của Mỹ. Sinh ra với tên Nguyễn Sinh Cung, Người đã sử dụng nhiều tên khác nhau trong suốt sự nghiệp cách mạng.'
    },
    'vo-nguyen-giap': {
      name: 'Võ Nguyên Giáp',
      role: 'Đại tướng và Nhà chiến lược quân sự',
      period: '1940s-1970s',
      nationality: 'Việt Nam',
      description: 'Nhà chiến lược quân sự xuất sắc, lãnh đạo quân đội Việt Nam giành chiến thắng trước cả quân Pháp và Mỹ. Được mệnh danh là "Napoléon đỏ" vì tài năng chiến thuật trong chiến tranh du kích.'
    },
    'jean-leclerc': {
      name: 'Jean Leclerc',
      role: 'Tư lệnh quân sự Pháp',
      period: '1945-1947',
      nationality: 'Pháp',
      description: 'Tướng Pháp chỉ huy lực lượng tại Đông Dương sau Thế chiến II. Lãnh đạo phản ứng quân sự ban đầu của Pháp đối với tuyên bố độc lập của Việt Nam.'
    },
    'jean-sainteny': {
      name: 'Jean Sainteny',
      role: 'Nhà ngoại giao Pháp',
      period: '1945-1946',
      nationality: 'Pháp',
      description: 'Nhà ngoại giao Pháp đã đàm phán với Hồ Chí Minh và các lãnh đạo Việt Nam. Đóng vai trò quan trọng trong các thỏa thuận sơ bộ Pháp-Việt năm 1946.'
    },
    'pham-van-dong': {
      name: 'Phạm Văn Đồng',
      role: 'Thủ tướng Chính phủ',
      period: '1945-1987',
      nationality: 'Việt Nam',
      description: 'Cộng sự thân cận của Hồ Chí Minh và Thủ tướng lâu năm của Bắc Việt Nam và sau đó là Việt Nam thống nhất. Nhân vật quan trọng trong các cuộc đàm phán ngoại giao.'
    },
    'truong-chinh': {
      name: 'Trường Chinh',
      role: 'Lãnh đạo Đảng Cộng sản',
      period: '1940s-1980s',
      nationality: 'Việt Nam',
      description: 'Lãnh đạo và nhà lý luận quan trọng của Đảng Cộng sản. Đóng vai trò then chốt trong cải cách ruộng đất và tổ chức chính trị trong thời kỳ độc lập.'
    }
  };

  // Get figures relevant to the current period
  const getRelevantFigures = () => {
    return relatedFigures
      .map(figureKey => historicalFigures[figureKey])
      .filter(figure => figure !== undefined);
  };

  const getFlagEmoji = (nationality: string) => {
    switch (nationality) {
      case 'Vietnamese': return '🇻🇳';
      case 'French': return '🇫🇷';
      case 'Chinese': return '🇨🇳';
      default: return '🌍';
    }
  };

  const getPortraitPlaceholder = (name: string, nationality: string) => {
    const initials = name.split(' ').map(n => n[0]).join('').substring(0, 2);
    const bgColor = nationality === 'Vietnamese' ? '#dc2626' : 
                   nationality === 'French' ? '#2563eb' : '#059669';
    
    return (
      <div 
        className="portrait-placeholder"
        style={{ backgroundColor: bgColor }}
      >
        <span>{initials}</span>
      </div>
    );
  };

  const relevantFigures = getRelevantFigures();

  if (relevantFigures.length === 0) {
    return null;
  }

  return (
    <div className="portrait-gallery-container">
      <button
        onClick={() => setIsVisible(!isVisible)}
        className="portrait-toggle-btn"
        title="Xem Nhân Vật Lịch Sử"
      >
        👥 Nhân Vật ({relevantFigures.length})
      </button>

      {isVisible && (
        <div className="portrait-gallery-modal">
          <div className="portrait-gallery-content">
            <div className="gallery-header">
              <h3>Nhân Vật Lịch Sử Quan Trọng ({period})</h3>
              <button 
                onClick={() => setIsVisible(false)}
                className="close-btn"
              >
                ✕
              </button>
            </div>
            
            <div className="portraits-grid">
              {relevantFigures.map((figure, index) => (
                <div 
                  key={index}
                  className="portrait-card"
                  onClick={() => setSelectedFigure(figure)}
                >
                  <div className="portrait-image">
                    {figure.imageUrl ? (
                      <img src={figure.imageUrl} alt={figure.name} />
                    ) : (
                      getPortraitPlaceholder(figure.name, figure.nationality)
                    )}
                  </div>
                  <div className="portrait-info">
                    <h4>{figure.name} {getFlagEmoji(figure.nationality)}</h4>
                    <p className="role">{figure.role}</p>
                    <p className="period">{figure.period}</p>
                  </div>
                </div>
              ))}
            </div>

            {selectedFigure && (
              <div className="figure-detail-modal">
                <div className="figure-detail-content">
                  <div className="detail-header">
                    <h3>{selectedFigure.name} {getFlagEmoji(selectedFigure.nationality)}</h3>
                    <button 
                      onClick={() => setSelectedFigure(null)}
                      className="close-btn"
                    >
                      ✕
                    </button>
                  </div>
                  <div className="detail-body">
                    <div className="detail-portrait">
                      {selectedFigure.imageUrl ? (
                        <img src={selectedFigure.imageUrl} alt={selectedFigure.name} />
                      ) : (
                        getPortraitPlaceholder(selectedFigure.name, selectedFigure.nationality)
                      )}
                    </div>
                    <div className="detail-info">
                      <div className="detail-field">
                        <strong>Vai Trò:</strong> {selectedFigure.role}
                      </div>
                      <div className="detail-field">
                        <strong>Thời Gian:</strong> {selectedFigure.period}
                      </div>
                      <div className="detail-field">
                        <strong>Quốc Tịch:</strong> {selectedFigure.nationality}
                      </div>
                      <div className="detail-description">
                        <strong>Tiểu Sử:</strong>
                        <p>{selectedFigure.description}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      <style jsx>{`
        .portrait-gallery-container {
          position: relative;
          display: inline-block;
          margin-left: 8px;
        }

        .portrait-toggle-btn {
          background: linear-gradient(135deg, #7c3aed, #a855f7);
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

        .portrait-toggle-btn:hover {
          background: linear-gradient(135deg, #8b5cf6, #c084fc);
          transform: translateY(-1px);
          box-shadow: 0 4px 8px rgba(0,0,0,0.3);
        }

        .portrait-gallery-modal {
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

        .portrait-gallery-content {
          background: white;
          border-radius: 12px;
          max-width: 90vw;
          max-height: 90vh;
          overflow: auto;
          box-shadow: 0 20px 40px rgba(0,0,0,0.3);
        }

        .gallery-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 20px;
          border-bottom: 1px solid #eee;
          background: linear-gradient(135deg, #7c3aed, #a855f7);
          color: white;
          border-radius: 12px 12px 0 0;
        }

        .gallery-header h3 {
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

        .portraits-grid {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
          gap: 20px;
          padding: 20px;
        }

        .portrait-card {
          background: white;
          border-radius: 12px;
          padding: 15px;
          text-align: center;
          cursor: pointer;
          transition: all 0.3s ease;
          border: 2px solid #f0f0f0;
          box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }

        .portrait-card:hover {
          transform: translateY(-5px);
          box-shadow: 0 8px 25px rgba(0,0,0,0.15);
          border-color: #7c3aed;
        }

        .portrait-image {
          width: 80px;
          height: 80px;
          margin: 0 auto 15px;
          border-radius: 50%;
          overflow: hidden;
          border: 3px solid #e5e7eb;
        }

        .portrait-image img {
          width: 100%;
          height: 100%;
          object-fit: cover;
        }

        .portrait-placeholder {
          width: 100%;
          height: 100%;
          display: flex;
          align-items: center;
          justify-content: center;
          color: white;
          font-weight: bold;
          font-size: 24px;
        }

        .portrait-info h4 {
          margin: 0 0 8px 0;
          font-size: 16px;
          color: #1f2937;
        }

        .portrait-info .role {
          margin: 0 0 4px 0;
          font-size: 14px;
          color: #6b7280;
          font-weight: 500;
        }

        .portrait-info .period {
          margin: 0;
          font-size: 12px;
          color: #9ca3af;
        }

        .figure-detail-modal {
          position: fixed;
          top: 0;
          left: 0;
          right: 0;
          bottom: 0;
          background: rgba(0, 0, 0, 0.9);
          display: flex;
          align-items: center;
          justify-content: center;
          z-index: 1100;
        }

        .figure-detail-content {
          background: white;
          border-radius: 12px;
          max-width: 600px;
          max-height: 80vh;
          overflow: auto;
          box-shadow: 0 25px 50px rgba(0,0,0,0.4);
        }

        .detail-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 20px;
          border-bottom: 1px solid #eee;
          background: linear-gradient(135deg, #1f2937, #374151);
          color: white;
          border-radius: 12px 12px 0 0;
        }

        .detail-header h3 {
          margin: 0;
          font-size: 20px;
        }

        .detail-body {
          padding: 20px;
        }

        .detail-portrait {
          width: 120px;
          height: 120px;
          margin: 0 auto 20px;
          border-radius: 50%;
          overflow: hidden;
          border: 4px solid #e5e7eb;
        }

        .detail-portrait img {
          width: 100%;
          height: 100%;
          object-fit: cover;
        }

        .detail-field {
          margin-bottom: 15px;
          padding: 10px;
          background: #f9fafb;
          border-radius: 6px;
        }

        .detail-field strong {
          color: #374151;
          margin-right: 8px;
        }

        .detail-description {
          margin-top: 20px;
          padding: 15px;
          background: #f0f9ff;
          border-radius: 8px;
          border-left: 4px solid #7c3aed;
        }

        .detail-description strong {
          display: block;
          margin-bottom: 10px;
          color: #1f2937;
        }

        .detail-description p {
          margin: 0;
          line-height: 1.6;
          color: #4b5563;
        }

        @keyframes fadeIn {
          from { opacity: 0; }
          to { opacity: 1; }
        }

        @media (max-width: 768px) {
          .portraits-grid {
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
            gap: 15px;
            padding: 15px;
          }
          
          .portrait-gallery-content,
          .figure-detail-content {
            margin: 10px;
          }
        }
      `}</style>
    </div>
  );
};

export default PortraitGallery;