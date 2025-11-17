export function formatDateTime(ts: number, locale = (typeof navigator !== "undefined" ? navigator.language : "es-ES")) {
    try {
      return new Intl.DateTimeFormat(locale, {
        day: "2-digit",
        month: "2-digit",
        year: "2-digit",
        hour: "2-digit",
        minute: "2-digit"
      }).format(ts);
    } catch {
      return new Date(ts).toLocaleString();
    }
  }
  