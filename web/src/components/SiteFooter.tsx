export default function SiteFooter() {
  return (
    <footer className="mt-16 border-t border-gray-200 bg-white/70">
      <div className="mx-auto max-w-6xl px-4 sm:px-6 py-8 text-sm text-gray-600">
        <p>
          © {new Date().getFullYear()} Dòng thời gian (1945–1946). Thiết kế hiện đại, trực quan và phản hồi tốt trên mọi thiết bị.
        </p>
      </div>
    </footer>
  );
}