export function Spinner({ className = "", size = 16 }: { className?: string; size?: number }) {
    return (
      <svg
        className={`animate-spin ${className}`}
        width={size}
        height={size}
        viewBox="0 0 24 24"
        role="status"
        aria-label="Cargando"
      >
        <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" className="opacity-25" />
        <path fill="currentColor" d="M4 12a8 8 0 0 1 8-8v4a4 4 0 0 0-4 4H4z" className="opacity-100" />
      </svg>
    );
  }
  