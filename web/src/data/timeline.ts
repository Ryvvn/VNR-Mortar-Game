export type TimelineItem = {
  title: string; // Ngày hiển thị trên trục thời gian (VD: "03/09/1945")
  cardTitle: string; // Tiêu đề thẻ
  cardSubtitle?: string; // Giai đoạn / chủ đề
  cardSummaryText?: string; // Mô tả ngắn gọn hiển thị mặc định
  cardDetailedText?: string | string[]; // Mô tả chi tiết hiển thị khi hover
  url?: string; // Nguồn tham khảo
  imageQuery?: string; // Tùy chọn: tiêu đề bài Wikipedia dùng để tìm thumbnail
  // Visual enhancement metadata
  relatedFigures?: string[]; // Key figures involved in this event
  relatedDocuments?: string[]; // Historical documents related to this event
  hasMapData?: boolean; // Whether this event has territorial/map significance
};

export const timelineItems: TimelineItem[] = [
  {
    title: "03/09/1945",
    cardTitle: "Chính phủ lâm thời họp phiên đầu tiên",
    cardSubtitle: "Xây dựng chế độ mới",
    cardSummaryText: "Chính phủ lâm thời đề ra ba nhiệm vụ cấp bách: diệt giặc đói, diệt giặc dốt, diệt giặc ngoại xâm.",
    cardDetailedText:
      "Sau khi tuyên bố độc lập, Chính phủ lâm thời Việt Nam Dân chủ Cộng hòa do Chủ tịch Hồ Chí Minh đứng đầu đã họp phiên đầu tiên tại Hà Nội. Trong bối cảnh đất nước vừa thoát khỏi ách thống trị của thực dân Pháp và phát xít Nhật, chính phủ non trẻ đã đề ra ba nhiệm vụ cấp bách: diệt giặc đói - giải quyết nạn đói hoành hành khắp nơi, diệt giặc dốt - xóa nạn mù chữ trong nhân dân, và diệt giặc ngoại xâm - bảo vệ độc lập dân tộc. Đồng thời, chính phủ quyết tâm cố chính quyền cách mạng, chống lại âm mưu tái xâm lược của Pháp, bài trừ các thế lực phản động trong nước và từng bước cải thiện đời sống của nhân dân sau nhiều năm chiến tranh.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Chính phủ Cách mạng lâm thời Việt Nam Dân chủ Cộng hòa",
    relatedFigures: ["ho-chi-minh", "pham-van-dong", "truong-chinh"],
    relatedDocuments: ["independence-declaration"],
    hasMapData: true,
  },
  {
    title: "17–24/09/1945",
    cardTitle: "Tuần lễ vàng, Quỹ Độc lập và các quỹ kháng chiến",
    cardSubtitle: "Huy động nguồn lực toàn dân",
    cardSummaryText: "Phát động 'Tuần lễ vàng' và thành lập các quỹ: Độc lập, Đảm phụ quốc phòng, Nam Bộ kháng chiến.",
    cardDetailedText:
      "Trước tình hình tài chính khó khăn của chính quyền mới thành lập, Chủ tịch Hồ Chí Minh đã phát động phong trào 'Tuần lễ vàng' kêu gọi nhân dân cả nước đóng góp vàng bạc, trang sức để xây dựng đất nước. Phong trào này đã nhận được sự hưởng ứng nhiệt tình của mọi tầng lớp nhân dân từ thành thị đến nông thôn. Đồng thời, chính phủ đã thành lập các quỹ quan trọng: Quỹ Độc lập để tài trợ cho việc xây dựng đất nước, Quỹ Đảm phụ quốc phòng hỗ trợ gia đình quân nhân, và Quỹ Nam Bộ kháng chiến để chi viện cho cuộc đấu tranh ở miền Nam. Những quỹ này không chỉ giải quyết khó khăn tài chính mà còn thể hiện tinh thần đoàn kết, ý chí quyết tâm bảo vệ nền độc lập non trẻ của toàn dân tộc.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Tuần lễ Vàng (năm 1945)",
  },
  {
    title: "23/09/1945",
    cardTitle: "Pháp nổ súng ở Sài Gòn – Gia Định",
    cardSubtitle: "Nam Bộ kháng chiến",
    cardSummaryText: "Quân Pháp tấn công Sài Gòn - Gia Định, nhân dân Nam Bộ thực hiện 'tiêu thổ kháng chiến'.",
    cardDetailedText:
      "Ngày 23/9/1945 đánh dấu bước ngoặt quan trọng khi quân đội Pháp chính thức nổ súng tấn công các khu vực do chính quyền cách mạng kiểm soát ở Sài Gòn và Gia Định, mở màn cho cuộc chiến tranh thực dân lần thứ hai. Trước sự tấn công dữ dội của địch, nhân dân Nam Bộ đã anh dũng thực hiện chính sách 'tiêu thổ kháng chiến' - phá hủy mọi cơ sở hạ tầng, kho tàng để không để lại gì cho kẻ thù. Với tinh thần bất khuất và khẩu hiệu bất hủ 'Thà chết tự do còn hơn sống nô lệ', đồng bào Nam Bộ đã quyết tâm bảo vệ chính quyền cách mạng, sẵn sàng hy sinh tất cả vì độc lập tự do của dân tộc. Đây là khởi đầu của cuộc kháng chiến trường kỳ, gian khổ nhưng đầy anh hùng của nhân dân miền Nam.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Nam Bộ kháng chiến",
    relatedFigures: ["jean-leclerc", "vo-nguyen-giap"],
    hasMapData: true,
  },
  {
    title: "25/10/1945",
    cardTitle: "Hội nghị cán bộ Đảng bộ Nam Bộ (Thiên Hộ – Cái Bè – Tiền Giang)",
    cardSubtitle: "Tổ chức lực lượng",
    cardSummaryText: "Kiện toàn tổ chức Đảng Nam Bộ, phối hợp các lực lượng vũ trang và xây dựng cơ sở cách mạng.",
    cardDetailedText:
      "Trong bối cảnh Nam Bộ đang chịu sự tấn công dữ dội của quân Pháp, Hội nghị cán bộ Đảng bộ Nam Bộ được tổ chức tại vùng căn cứ Thiên Hộ, huyện Cái Bè, tỉnh Tiền Giang đã đóng vai trò then chốt trong việc định hướng cuộc kháng chiến ở miền Nam. Hội nghị tập trung vào việc kiện toàn tổ chức Đảng từ cấp tỉnh đến cơ sở, xây dựng hệ thống lãnh đạo thống nhất và hiệu quả để chỉ đạo cuộc kháng chiến trong điều kiện khó khăn. Đặc biệt, hội nghị đã đề ra phương hướng phối hợp chặt chẽ giữa các lực lượng vũ trang chính quy, bán chính quy và dân quân tự vệ, đồng thời xây dựng mạng lưới cơ sở cách mạng bí mật trong các vùng tạm thời bị địch chiếm đóng. Những quyết định của hội nghị đã tạo nền tảng vững chắc cho sự phát triển của phong trào kháng chiến Nam Bộ trong những năm tiếp theo.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Đảng Cộng sản Việt Nam",
  },
  {
    title: "11/11/1945",
    cardTitle: "Tuyên bố giải tán Đảng Cộng sản Đông Dương",
    cardSubtitle: "Chiến lược ngoại giao",
    cardSummaryText: "Quyết định chiến lược nhằm mở rộng mặt trận dân tộc thống nhất và tránh can thiệp từ Trung Quốc.",
    cardDetailedText:
      "Trong bối cảnh chính trị phức tạp với sự hiện diện của quân Tưởng Giới Thạch ở miền Bắc và nguy cơ can thiệp từ các thế lực quốc tế, Ban Chấp hành Trung ương Đảng đã đưa ra quyết định táo bạo: tuyên bố giải tán Đảng Cộng sản Đông Dương. Đây là một nước cờ ngoại giao khôn khéo nhằm làm dịu thái độ thù địch của chính quyền Tưởng Giới Thạch - vốn coi Đảng Cộng sản là mối đe dọa. Đồng thời, quyết định này giúp mở rộng mặt trận dân tộc thống nhất, thu hút các tầng lớp nhân dân và các đảng phái yêu nước khác cùng tham gia vào cuộc đấu tranh chống thực dân Pháp. Mặc dù chỉ là sự giải tán về mặt hình thức, nhưng quyết định này thể hiện sự linh hoạt và tài năng chính trị của lãnh đạo cách mạng trong việc vận dụng các thủ đoạn đấu tranh phù hợp với từng giai đoạn lịch sử cụ thể.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Hồ Chí Minh",
  },
  {
    title: "25/11/1945",
    cardTitle: "Chỉ thị \"Kháng chiến kiến quốc\"",
    cardSubtitle: "Chuẩn bị toàn diện",
    cardSummaryText: "Ban Chấp hành Trung ương Đảng ra chỉ thị chuẩn bị toàn diện cho cuộc kháng chiến lâu dài.",
    cardDetailedText:
      "Chỉ thị 'Kháng chiến kiến quốc' do Ban Chấp hành Trung ương Đảng ban hành ngày 25/11/1945 đã đặt ra những định hướng chiến lược toàn diện cho cuộc đấu tranh bảo vệ độc lập dân tộc. Về mặt chính trị, chỉ thị nhấn mạnh việc củng cố khối đại đoàn kết dân tộc, xây dựng chính quyền cách mạng vững mạnh từ trung ương đến địa phương. Về quân sự, chỉ thị đề ra phương hướng xây dựng lực lượng vũ trang nhân dân ba thứ quân, chuẩn bị chiến tranh nhân dân toàn diện. Về ngoại giao, chỉ thị định hướng chính sách đối ngoại linh hoạt, tận dụng mâu thuẫn giữa các cường quốc để bảo vệ lợi ích dân tộc. Đặc biệt, chỉ thị khẳng định tinh thần 'Dân tộc trên hết, Tổ quốc trên hết', thể hiện quyết tâm cao nhất của toàn Đảng, toàn dân trong cuộc đấu tranh giành và giữ vững độc lập dân tộc.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Kháng chiến chống Pháp",
  },
  {
    title: "06/01/1946",
    cardTitle: "Tổng tuyển cử đầu tiên",
    cardSubtitle: "Dân chủ thực sự",
    cardSummaryText: "Cuộc tổng tuyển cử đầu tiên trong lịch sử, thành lập Quốc hội khóa I với 333 đại biểu.",
    cardDetailedText:
      "Cuộc tổng tuyển cử ngày 6/1/1946 đánh dấu một cột mốc lịch sử rô cùng quan trọng - lần đầu tiên trong lịch sử dân tộc, nhân dân Việt Nam được thực hiện quyền dân chủ thực sự thông qua việc bầu cử đại biểu Quốc hội. Cuộc bầu cử diễn ra trong bầu không khí hân hoan, phấn khởi của toàn dân với tinh thần 'Mỗi phiếu bầu là một viên đạn bắn vào kẻ thù'. Kết quả bầu cử đã thành lập Quốc hội khóa I gồm 333 đại biểu đại diện cho ý chí và nguyện vọng của nhân dân các dân tộc Việt Nam. Đây không chỉ là sự kiện chính trị quan trọng mà còn là minh chứng rõ nét cho bản chất dân chủ của chính quyền cách mạng, khẳng định nguyên tắc 'dân có, dân biết, dân làm, dân hưởng' trong việc xây dựng và quản lý đất nước.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Tổng tuyển cử Việt Nam 1946",
  },
  {
    title: "Đầu 1946",
    cardTitle: "Đẩy lùi nạn đói, ổn định đời sống",
    cardSubtitle: "Chống giặc đói",
    cardSummaryText: "Thực hiện các biện pháp tăng sản xuất, tiết kiệm, cải cách ruộng đất và giảm thuế để chống nạn đói.",
    cardDetailedText:
      "Nạn đói khủng khiếp năm 1945 đã cướp đi sinh mạng của hàng triệu người dân Việt Nam, trở thành thách thức sinh tồn cấp bách nhất của chính quyền cách mạng. Thực hiện nhiệm vụ 'diệt giặc đói', chính phủ đã triển khai đồng bộ nhiều biện pháp quyết liệt: khuyến khích tăng gia sản xuất nông nghiệp bằng cách hỗ trợ giống cây trồng và kỹ thuật canh tác, phát động phong trào tiết kiệm lương thực trong toàn xã hội, và thực hiện chính sách cải cách ruộng đất bước đầu để nông dân có đất canh tác. Đặc biệt, chính phủ đã bãi bỏ các loại thuế khóa nặng nề như thuế thân - loại thuế bất công đánh vào mỗi cá nhân, và giảm thuế tô (thuế ruộng) 25% để giảm gánh nặng cho nông dân. Nhờ những chính sách kịp thời và hiệu quả này, đến đầu năm 1946, nạn đói đã cơ bản được đẩy lùi, đời sống nhân dân dần ổn định, tạo tiền đề quan trọng cho việc xây dựng đất nước và chuẩn bị kháng chiến.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Vietnamese Famine of 1945",
  },
  {
    title: "06/01/1946",
    cardTitle: "Tổng tuyển cử bầu Quốc hội khóa I",
    cardSubtitle: "Xây dựng chính quyền",
    cardDetailedText:
      "Cuộc tổng tuyển cử lịch sử đầu tiên của nước Việt Nam Dân chủ Cộng hòa đã diễn ra trong bầu không khí hân hoan, phấn khởi của toàn dân. Đây là lần đầu tiên trong lịch sử dân tộc, nhân dân Việt Nam được thực hiện quyền dân chủ cơ bản - quyền bầu cử và ứng cử một cách tự do, bình đẳng. Với tỷ lệ cử tri tham gia bỏ phiếu lên tới 89% - một con số ấn tượng thể hiện sự tin tưởng và ủng hộ mạnh mẽ của nhân dân đối với chính quyền cách mạng, cuộc bầu cử đã chọn ra 333 đại biểu đại diện cho ý chí và nguyện vọng của toàn dân tộc. Quốc hội khóa I ra đời đánh dấu bước tiến quan trọng trong việc xây dựng nhà nước pháp quyền dân chủ, tạo nền tảng vững chắc cho các hoạt động lập pháp và giám sát quyền lực nhà nước.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "1946 Vietnamese National Assembly election",
  },
  {
    title: "28/02/1946",
    cardTitle: "Hiệp ước Trùng Khánh (Hoa – Pháp)",
    cardSubtitle: "Bối cảnh ngoại giao",
    cardSummaryText: "Trung Quốc và Pháp ký hiệp ước, đồng ý rút quân khỏi miền Bắc Việt Nam.",
    cardDetailedText:
      "Hiệp ước Trùng Khánh được ký kết giữa Trung Hoa Dân Quốc và Pháp tại thủ đô Trùng Khánh đã tạo ra một bước ngoặt quan trọng trong cục diện chính trị khu vực Đông Dương. Theo thỏa thuận này, Trung Quốc đồng ý rút quân khỏi miền Bắc Việt Nam và cho phép quân Pháp trở lại, đổi lại Pháp nhượng bộ một số quyền lợi kinh tế và chính trị cho Trung Quốc tại Đông Dương. Hiệp ước này đặt Việt Nam vào tình thế khó khăn khi phải đối mặt trực tiếp với quân Pháp mà không còn 'lá chắn' từ quân Tưởng. Tuy nhiên, lãnh đạo Việt Nam đã xử lý tình hình một cách khéo léo, tận dụng khoảng thời gian trước khi quân Pháp tiến vào để củng cố lực lượng, đồng thời duy trì thế chủ động trong các cuộc đàm phán ngoại giao sắp tới. Đây là bài học quý báu về việc vận dụng linh hoạt ngoại giao trong hoàn cảnh bất lợi.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Chongqing",
    relatedDocuments: ["chongqing-accord"],
    hasMapData: true,
  },
  {
    title: "02/03/1946",
    cardTitle: "Quốc hội họp, thành lập Chính phủ chính thức",
    cardSubtitle: "Củng cố chính quyền",
    cardSummaryText: "Thành lập Ban Thường trực và Chính phủ 10 bộ.",
    cardDetailedText:
      "Quốc hội khóa I họp; thành lập Ban Thường trực (Nguyễn Văn Tố), tổ chức Chính phủ gồm 10 bộ; Chủ tịch Hồ Chí Minh đứng đầu.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "National Assembly of Vietnam",
    relatedFigures: ["ho-chi-minh", "nguyen-van-to"],
  },
  {
    title: "06/03/1946",
    cardTitle: "Hiệp định sơ bộ Việt – Pháp",
    cardSubtitle: "Thỏa thuận tạm thời",
    cardSummaryText: "Pháp thừa nhận VNDCCH, Việt Nam cho phép quân Pháp trở lại.",
    cardDetailedText:
      "Hiệp định sơ bộ Việt-Pháp được ký kết tại Hà Nội đánh dấu một bước ngoặt quan trọng trong quan hệ Việt-Pháp sau Cách mạng Thám Tám. Theo hiệp định, Pháp chính thức thừa nhận Việt Nam Dân chủ Cộng hòa là một quốc gia tự do trong Liên hiệp Pháp và Liên bang Đông Dương, đồng thời công nhận quyền tự trị của Việt Nam trong nội chính. Đổi lại, Việt Nam đồng ý cho quân Pháp trở lại miền Bắc để thay thế quân Tưởng Giới Thạch với cam kết rút dần trong 5 năm. Mặc dù bị nhiều người chỉ trích là 'nhượng bộ', Chủ tịch Hồ Chí Minh đã giải thích rằng đây là quyết định chiến lược nhằm tranh thủ thời gian xây dựng lực lượng và tránh chiến tranh trên hai mặt trận. Ông nói: 'Thà ngửi phân Tây 5 năm, chứ không ăn phân Tàu cả đời'. Hiệp định này thể hiện tài năng ngoại giao của lãnh đạo Việt Nam trong việc tận dụng mâu thuẫn giữa các cường quốc để bảo vệ độc lập dân tộc.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Ho Chi Minh signing agreement with French",
    relatedFigures: ["ho-chi-minh", "jean-sainteny"],
    relatedDocuments: ["franco-vietnamese-agreement"],
    hasMapData: true,
  },
  {
    title: "Cuối 1946",
    cardTitle: "Bình dân học vụ: 2,5 triệu người biết chữ",
    cardSubtitle: "Chống giặc dốt",
    cardSummaryText: "Phong trào 'Toàn dân học chữ quốc ngữ' thành công rực rỡ.",
    cardDetailedText:
      "Phong trào 'Bình dân học vụ' được phát động mạnh mẽ trên toàn quốc với khẩu hiệu 'Toàn dân học chữ quốc ngữ' đã trở thành một cuộc cách mạng văn hóa thực sự. Trong bối cảnh hơn 95% dân số mù chữ do chế độ thực dân để lại, chính phủ cách mạng đã xác định việc xóa nạn mù chữ là nhiệm vụ cấp bách để nâng cao dân trí và xây dựng đất nước. Các lớp học được tổ chức khắp nơi từ thành thị đến nông thôn, từ cơ quan, nhà máy đến các làng xã. Người biết chữ dạy người chưa biết, tạo nên phong trào học tập sôi nổi trong toàn xã hội. Kết quả đáng kinh ngạc: chỉ trong thời gian ngắn, đã có 2,5 triệu người biết chữ, giảm đáng kể tỷ lệ mù chữ. Đồng thời, phong trào này còn góp phần xây dựng nếp sống mới, văn minh, đẩy lùi các tệ nạn xã hội như cờ bạc, hút thuốc phiện, mê tín dị đoan. Đây là minh chứng rõ nét cho chính sách 'dân có, dân biết, dân làm, dân hưởng' của chính quyền cách mạng.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Literacy in Vietnam",
  },
  {
    title: "Cuối 1946",
    cardTitle: "Củng cố lực lượng vũ trang",
    cardSubtitle: "Chuẩn bị kháng chiến lâu dài",
    cardSummaryText: "Mở rộng quân đội lên 80.000 quân chính quy.",
    cardDetailedText:
      "Nhận thức rõ nguy cơ chiến tranh bùng nổ, chính phủ cách mạng đã tập trung xây dựng và củng cố lực lượng vũ trang một cách có hệ thống và quyết liệt. Quân đội nhân dân Việt Nam được mở rộng lên 80.000 quân chính quy với tổ chức hoàn chỉnh từ trung đoàn đến tiểu đoàn, được trang bị vũ khí hiện đại hơn và được huấn luyện theo phương pháp quân sự khoa học. Đồng thời, lực lượng bán chính quy và dân quân tự vệ cũng được phát triển mạnh mẽ ở các địa phương, tạo thành hệ thống quốc phòng toàn dân vững chắc. Các căn cứ địa được xây dựng ở những vùng núi hiểm trở, chuẩn bị sẵn sàng cho cuộc kháng chiến lâu dài. Việc củng cố lực lượng vũ trang này đã tạo nền tảng quan trọng cho những chiến thắng vẻ vang trong cuộc kháng chiến chống thực dân Pháp sau này.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Vietnam People's Army",
  },
  {
    title: "09/11/1946",
    cardTitle: "Thông qua Hiến pháp 1946",
    cardSubtitle: "Nền tảng pháp lý quốc gia",
    cardSummaryText: "Hiến pháp dân chủ đầu tiên trong lịch sử dân tộc.",
    cardDetailedText:
      "Kỳ họp thứ 2 của Quốc hội khóa I đã thông qua Hiến pháp 1946 - một sự kiện có ý nghĩa lịch sử to lớn, đánh dấu việc Việt Nam có được bản Hiến pháp dân chủ đầu tiên trong lịch sử dân tộc. Hiến pháp này được soạn thảo dựa trên tinh thần dân chủ tiến bộ, thể hiện ý chí và nguyện vọng của toàn thể nhân dân Việt Nam. Văn kiện quan trọng này đã xác lập rõ rang nguyên tắc 'quyền lực thuộc về nhân dân', khẳng định chế độ dân chủ cộng hòa với Quốc hội là cơ quan quyền lực nhà nước cao nhất. Hiến pháp quy định đầy đủ các quyền tự do cơ bản của công dân như quốc hội và ý chí độc lập của dân tộc Việt Nam.",
    url:
      "https://lilac-wolf-31a.notion.site/Ch-ng-2-ng-l-nh-o-2-cu-c-kh-ng-chi-n-ho-n-th-nh-gi-i-ph-ng-d-n-t-c-th-ng-nh-t-t-n-c-1945-28c775060d0080e2b7d6d78814df08b4",
    imageQuery: "Constitution of Vietnam",
  },
];