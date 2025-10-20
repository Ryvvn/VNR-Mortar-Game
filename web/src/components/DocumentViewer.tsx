'use client';

import React, { useState } from 'react';

interface HistoricalDocument {
  title: string;
  type: 'treaty' | 'proclamation' | 'agreement' | 'declaration';
  date: string;
  description: string;
  content: string;
  significance: string;
  parties?: string[];
}

interface DocumentViewerProps {
  period: string;
  relatedDocuments: string[];
}

const DocumentViewer: React.FC<DocumentViewerProps> = ({ period, relatedDocuments }) => {
  const [isVisible, setIsVisible] = useState(false);
  const [selectedDocument, setSelectedDocument] = useState<HistoricalDocument | null>(null);

  // Historical documents database
  const historicalDocuments: { [key: string]: HistoricalDocument } = {
    'independence-declaration': {
      title: 'Tuyên Ngôn Độc Lập Việt Nam',
      type: 'declaration',
      date: '2 tháng 9, 1945',
      description: 'Tuyên ngôn lịch sử thành lập nước Việt Nam Dân chủ Cộng hòa',
      parties: ['Việt Nam Dân chủ Cộng hòa'],
      content: `"Tất cả mọi người đều sinh ra có quyền bình đẳng. Tạo hóa cho họ những quyền không ai có thể xâm phạm được; trong những quyền ấy, có quyền được sống, quyền tự do và quyền mưu cầu hạnh phúc."

Lời bất hủ ấy ở trong bản Tuyên ngôn Độc lập năm 1776 của nước Mỹ. Suy rộng ra, câu ấy có nghĩa là: Tất cả các dân tộc trên thế giới đều sinh ra bình đẳng; dân tộc nào cũng có quyền sống, quyền sung sướng và quyền tự do.

Bản Tuyên ngôn Nhân quyền và Dân quyền của Cách mạng Pháp năm 1791 cũng nói: "Người ta sinh ra tự do và bình đẳng về quyền lợi, và phải luôn luôn được tự do và bình đẳng về quyền lợi."

Đó là những lẽ phải không ai chối cãi được.

Thế mà hơn 80 năm nay, bọn thực dân Pháp, lợi dụng lá cờ Tự do, Bình đẳng, Bác ái, đã đến cướp đất nước ta, áp bức đồng bào ta. Hành động của chúng trái hẳn với nhân đạo và chính nghĩa...`,
      significance: 'Thiết lập Việt Nam như một quốc gia độc lập và đặt nền móng cho nước Việt Nam Dân chủ Cộng hòa.'
    },
    'chongqing-accord': {
      title: 'Hiệp Định Trùng Khánh',
      type: 'agreement',
      date: '28 tháng 2, 1946',
      description: 'Thỏa thuận giữa Pháp và Trung Quốc về Việt Nam',
      parties: ['Pháp', 'Trung Hoa Dân Quốc'],
      content: `Thỏa thuận giữa Cộng hòa Pháp và Trung Hoa Dân Quốc về việc rút quân Trung Quốc khỏi Bắc Việt Nam và thiết lập quyền kiểm soát hành chính của Pháp.

Các điều khoản chính:
1. Quân đội Trung Quốc rút khỏi Việt Nam phía bắc vĩ tuyến 16
2. Pháp tôn trọng lợi ích của Trung Quốc trong khu vực
3. Thiết lập các đặc quyền kinh tế đặc biệt cho Trung Quốc tại Việt Nam
4. Công nhận chủ quyền của Pháp đối với Đông Dương

Thỏa thuận này mở đường cho Pháp trở lại miền Bắc Việt Nam đồng thời giải quyết mối quan ngại của Trung Quốc về an ninh biên giới phía nam.`,
      significance: 'Tạo điều kiện cho Pháp trở lại miền Bắc Việt Nam và tạo tiền đề cho xung đột Pháp-Việt.'
    },
    'preliminary-agreement': {
      title: 'Hiệp Định Sơ Bộ Pháp-Việt',
      type: 'agreement',
      date: '6 tháng 3, 1946',
      description: 'Thỏa thuận ban đầu giữa Pháp và Việt Nam Dân chủ Cộng hòa',
      parties: ['Pháp', 'Việt Nam Dân chủ Cộng hòa'],
      content: `Hiệp Định Sơ Bộ giữa Chính phủ Pháp và Chính phủ Việt Nam Dân chủ Cộng hòa:

Điều 1: Chính phủ Pháp công nhận nước Việt Nam Cộng hòa là một Quốc gia Tự do có Chính phủ, Quốc hội, Quân đội và Tài chính riêng, thuộc Liên bang Đông Dương và Liên hiệp Pháp.

Điều 2: Chính phủ Việt Nam tuyên bố sẵn sàng tiếp đón một cách thân ái quân đội Pháp khi, theo các hiệp định quốc tế, quân đội này thay thế quân đội Trung Quốc.

Điều 3: Các quy định nêu trên sẽ có hiệu lực ngay lập tức. Ngay sau khi trao đổi chữ ký, mỗi Bên Ký kết Cao cấp sẽ thực hiện mọi biện pháp cần thiết để ngừng các hành động thù địch tại chỗ, duy trì quân đội ở vị trí tương ứng, và tạo bầu không khí thuận lợi cho việc mở ngay các cuộc đàm phán thân thiện và thẳng thắn.

Điều 4: Các cuộc đàm phán quy định trong Điều 3 sẽ đặc biệt đề cập đến:
a) Quan hệ ngoại giao của Việt Nam với các Quốc gia nước ngoài;
b) Tương lai của Đông Dương;
c) Lợi ích kinh tế và văn hóa của Pháp tại Việt Nam.`,
      significance: 'Sự công nhận chính thức đầu tiên về quyền tự trị của Việt Nam bởi Pháp, mặc dù trong khuôn khổ Liên hiệp Pháp.'
    },
    'fontainebleau-conference': {
      title: 'Tài Liệu Hội Nghị Fontainebleau',
      type: 'agreement',
      date: 'Tháng 7-9, 1946',
      description: 'Cuộc đàm phán thất bại giữa Pháp và Việt Nam tại Fontainebleau',
      parties: ['Pháp', 'Việt Nam Dân chủ Cộng hòa'],
      content: `Tài liệu từ Hội nghị Fontainebleau (6 tháng 7 - 10 tháng 9, 1946):

Lập trường của Việt Nam:
- Hoàn toàn độc lập cho Việt Nam
- Thống nhất ba kỳ (Bắc Kỳ, Trung Kỳ, Nam Kỳ)
- Việt Nam kiểm soát quan hệ đối ngoại
- Rút quân Pháp

Lập trường của Pháp:
- Việt Nam là quốc gia tự trị trong Liên hiệp Pháp
- Địa vị riêng biệt cho Nam Kỳ
- Pháp kiểm soát chính sách đối ngoại và quốc phòng
- Duy trì sự hiện diện quân sự của Pháp

Hội nghị kết thúc mà không có thỏa thuận, dẫn đến sự đổ vỡ quan hệ ngoại giao và bắt đầu Chiến tranh Đông Dương lần thứ nhất.

Thông cáo cuối cùng ghi nhận "những khác biệt cơ bản" không thể giải quyết thông qua đàm phán.`,
      significance: 'Đánh dấu sự thất bại của các giải pháp ngoại giao và bắt đầu xung đột vũ trang giữa Pháp và Việt Nam.'
    }
  };

  // Get documents relevant to the current period
  const getRelevantDocuments = () => {
    return relatedDocuments
      .map(docKey => historicalDocuments[docKey])
      .filter(doc => doc !== undefined);
  };

  const getDocumentIcon = (type: string) => {
    switch (type) {
      case 'treaty': return '📜';
      case 'proclamation': return '📢';
      case 'agreement': return '🤝';
      case 'declaration': return '📋';
      default: return '📄';
    }
  };

  const getDocumentTypeColor = (type: string) => {
    switch (type) {
      case 'treaty': return '#dc2626';
      case 'proclamation': return '#ea580c';
      case 'agreement': return '#059669';
      case 'declaration': return '#7c3aed';
      default: return '#6b7280';
    }
  };

  const getDocumentTypeLabel = (type: string) => {
    switch (type) {
      case 'treaty': return 'Hiệp ước';
      case 'proclamation': return 'Tuyên cáo';
      case 'agreement': return 'Hiệp định';
      case 'declaration': return 'Tuyên ngôn';
      default: return 'Tài liệu';
    }
  };

  const relevantDocuments = getRelevantDocuments();

  if (relevantDocuments.length === 0) {
    return null;
  }

  return (
    <div className="document-viewer-container">
      <button
        onClick={() => setIsVisible(!isVisible)}
        className="document-toggle-btn"
        title="Xem Tài Liệu Lịch Sử"
      >
        📄 Tài Liệu ({relevantDocuments.length})
      </button>

      {isVisible && (
        <div className="document-viewer-modal">
          <div className="document-viewer-content">
            <div className="viewer-header">
              <h3>Tài Liệu Lịch Sử ({period})</h3>
              <button
                onClick={() => setIsVisible(false)}
                className="close-btn"
              >
                ✕
              </button>
            </div>

            <div className="documents-grid">
              {relevantDocuments.map((document, index) => (
                <div
                  key={index}
                  className="document-card"
                  onClick={() => setSelectedDocument(document)}
                >
                  <div className="document-icon">
                    <span style={{ fontSize: '24px' }}>
                      {getDocumentIcon(document.type)}
                    </span>
                  </div>
                  <div className="document-info">
                    <h4>{document.title}</h4>
                    <div
                      className="document-type"
                      style={{ color: getDocumentTypeColor(document.type) }}
                    >
                      {document.type.toUpperCase()}
                    </div>
                    <p className="document-date">{document.date}</p>
                    <p className="document-description">{document.description}</p>
                    {document.parties && (
                      <div className="document-parties">
                        <strong>Các Bên:</strong> {document.parties.join(', ')}
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>

            {selectedDocument && (
              <div className="document-detail-modal">
                <div className="document-detail-content">
                  <div className="detail-header">
                    <div className="header-info">
                      <h3>{selectedDocument.title}</h3>
                      <div className="header-meta">
                        <span
                          className="document-type-badge"
                          style={{ backgroundColor: getDocumentTypeColor(selectedDocument.type) }}
                        >
                          {getDocumentIcon(selectedDocument.type)} {getDocumentTypeLabel(selectedDocument.type)}
                        </span>
                        <span className="document-date-badge">{selectedDocument.date}</span>
                      </div>
                    </div>
                    <button
                      onClick={() => setSelectedDocument(null)}
                      className="close-btn"
                    >
                      ✕
                    </button>
                  </div>

                  <div className="detail-body">
                    <div className="document-metadata">
                      <div className="metadata-field">
                        <strong>Mô Tả:</strong> {selectedDocument.description}
                      </div>
                      {selectedDocument.parties && (
                        <div className="metadata-field">
                          <strong>Các Bên Liên Quan:</strong> {selectedDocument.parties.join(', ')}
                        </div>
                      )}
                      <div className="metadata-field">
                        <strong>Ý Nghĩa Lịch Sử:</strong> {selectedDocument.significance}
                      </div>
                    </div>

                    <div className="document-content">
                      <h4>Nội Dung Tài Liệu:</h4>
                      <div className="content-text">
                        {selectedDocument.content.split('\n').map((paragraph, idx) => (
                          <p key={idx}>{paragraph}</p>
                        ))}
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
        .document-viewer-container {
          position: relative;
          display: inline-block;
          margin-left: 8px;
        }

        .document-toggle-btn {
          background: linear-gradient(135deg, #059669, #10b981);
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

        .document-toggle-btn:hover {
          background: linear-gradient(135deg, #10b981, #34d399);
          transform: translateY(-1px);
          box-shadow: 0 4px 8px rgba(0,0,0,0.3);
        }

        .document-viewer-modal {
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

        .document-viewer-content {
          background: white;
          border-radius: 12px;
          max-width: 90vw;
          max-height: 90vh;
          overflow: auto;
          box-shadow: 0 20px 40px rgba(0,0,0,0.3);
        }

        .viewer-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          padding: 20px;
          border-bottom: 1px solid #eee;
          background: linear-gradient(135deg, #059669, #10b981);
          color: white;
          border-radius: 12px 12px 0 0;
        }

        .viewer-header h3 {
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

        .documents-grid {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
          gap: 20px;
          padding: 20px;
        }

        .document-card {
          background: white;
          border-radius: 12px;
          padding: 20px;
          cursor: pointer;
          transition: all 0.3s ease;
          border: 2px solid #f0f0f0;
          box-shadow: 0 2px 8px rgba(0,0,0,0.1);
          display: flex;
          gap: 15px;
        }

        .document-card:hover {
          transform: translateY(-5px);
          box-shadow: 0 8px 25px rgba(0,0,0,0.15);
          border-color: #059669;
        }

        .document-icon {
          flex-shrink: 0;
          width: 50px;
          height: 50px;
          display: flex;
          align-items: center;
          justify-content: center;
          background: #f9fafb;
          border-radius: 8px;
        }

        .document-info {
          flex: 1;
        }

        .document-info h4 {
          margin: 0 0 8px 0;
          font-size: 16px;
          color: #1f2937;
          line-height: 1.3;
        }

        .document-type {
          font-size: 12px;
          font-weight: bold;
          margin-bottom: 8px;
        }

        .document-date {
          margin: 0 0 8px 0;
          font-size: 14px;
          color: #6b7280;
          font-weight: 500;
        }

        .document-description {
          margin: 0 0 12px 0;
          font-size: 14px;
          color: #4b5563;
          line-height: 1.4;
        }

        .document-parties {
          font-size: 12px;
          color: #6b7280;
        }

        .document-parties strong {
          color: #374151;
        }

        .document-detail-modal {
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

        .document-detail-content {
          background: white;
          border-radius: 12px;
          max-width: 80vw;
          max-height: 90vh;
          overflow: auto;
          box-shadow: 0 25px 50px rgba(0,0,0,0.4);
        }

        .detail-header {
          display: flex;
          justify-content: space-between;
          align-items: flex-start;
          padding: 20px;
          border-bottom: 1px solid #eee;
          background: linear-gradient(135deg, #1f2937, #374151);
          color: white;
          border-radius: 12px 12px 0 0;
        }

        .header-info h3 {
          margin: 0 0 10px 0;
          font-size: 20px;
        }

        .header-meta {
          display: flex;
          gap: 10px;
          flex-wrap: wrap;
        }

        .document-type-badge,
        .document-date-badge {
          padding: 4px 8px;
          border-radius: 12px;
          font-size: 12px;
          font-weight: 500;
        }

        .document-type-badge {
          color: white;
        }

        .document-date-badge {
          background: rgba(255,255,255,0.2);
          color: white;
        }

        .detail-body {
          padding: 20px;
        }

        .document-metadata {
          margin-bottom: 30px;
        }

        .metadata-field {
          margin-bottom: 15px;
          padding: 12px;
          background: #f9fafb;
          border-radius: 6px;
          border-left: 4px solid #059669;
        }

        .metadata-field strong {
          color: #374151;
          margin-right: 8px;
        }

        .document-content {
          background: #ffffff;
          border: 1px solid #e5e7eb;
          border-radius: 8px;
          padding: 20px;
        }

        .document-content h4 {
          margin: 0 0 15px 0;
          color: #1f2937;
          border-bottom: 2px solid #059669;
          padding-bottom: 8px;
        }

        .content-text {
          line-height: 1.7;
          color: #374151;
        }

        .content-text p {
          margin-bottom: 15px;
        }

        .content-text p:last-child {
          margin-bottom: 0;
        }

        @keyframes fadeIn {
          from { opacity: 0; }
          to { opacity: 1; }
        }

        @media (max-width: 768px) {
          .documents-grid {
            grid-template-columns: 1fr;
            gap: 15px;
            padding: 15px;
          }
          
          .document-viewer-content,
          .document-detail-content {
            margin: 10px;
          }

          .document-card {
            flex-direction: column;
            text-align: center;
          }

          .document-icon {
            align-self: center;
          }
        }
      `}</style>
    </div>
  );
};

export default DocumentViewer;