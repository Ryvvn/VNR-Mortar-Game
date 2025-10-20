"use client";

import dynamic from "next/dynamic";

const InteractiveTimeline = dynamic(() => import("@/components/InteractiveTimeline"), {
  ssr: false,
});

export default function Home() {
  return (
    <div className="min-h-screen w-full p-6 sm:p-10 relative">
      {/* Animated background elements */}
      <div className="fixed inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-20 left-10 w-32 h-32 bg-gradient-to-br from-red-500/10 to-yellow-500/10 rounded-full animate-float" style={{ animationDelay: '0s' }} />
        <div className="absolute top-40 right-20 w-24 h-24 bg-gradient-to-br from-yellow-500/10 to-red-500/10 rounded-full animate-float" style={{ animationDelay: '1s' }} />
        <div className="absolute bottom-32 left-1/4 w-20 h-20 bg-gradient-to-br from-red-600/10 to-red-400/10 rounded-full animate-float" style={{ animationDelay: '2s' }} />
      </div>

      <main className="mx-auto max-w-6xl relative z-10">
        <header className="mb-8 sm:mb-10 animate-fade-in-up">
          <div className="glass-effect p-8 rounded-2xl mb-6">
            <h1 className="text-4xl sm:text-5xl font-bold tracking-tight gradient-text mb-4">
              Dòng thời gian: Đảng Cộng sản Việt Nam
            </h1>
            <p className="text-lg sm:text-xl text-slate-600 font-medium mb-2">
              1945–1946
            </p>
            <p className="text-sm sm:text-base text-slate-700 max-w-2xl leading-relaxed">
              Hiển thị các tiến trình quan trọng sau Cách mạng Tháng Tám.
            </p>
            {/* Enhanced accent divider */}
            <div className="mt-6 flex items-center gap-2">
              <div className="h-1 w-32 rounded-full bg-gradient-to-r from-red-600 via-red-500 to-yellow-500 animate-pulse-glow" />
              <div className="w-2 h-2 rounded-full bg-red-500 animate-pulse" />
            </div>
          </div>
        </header>
        
        <section 
          id="timeline" 
          className="scroll-mt-20 p-4 sm:p-6 surface-card animate-zoom-in-soft"
          style={{ animationDelay: '200ms' }}
        >
          <InteractiveTimeline />
        </section>
      </main>
    </div>
  );
}
