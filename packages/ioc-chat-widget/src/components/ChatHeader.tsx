import banner from "../assets/banner.png";

export function ChatHeader({
  title,
  onNewChat,
}: {
  title: string;
  onNewChat: () => void;
}) {
  return (
    <header className="relative flex items-center pl-4 pr-2 py-3 bg-brand border-b border-white/25 text-white">
      <div className="flex items-center gap-3">
        <h2 className="font-semibold">{title}</h2>
        <div className="shrink-0 h-8">
          <img
            src={banner}
            alt="Logo corporativo"
            className="h-8 w-auto object-contain select-none"
            draggable={false}
          />
        </div>
      </div>

      <button
        onClick={onNewChat}
        className="absolute right-2 top-1/2 -translate-y-1/2 text-xs px-2 py-1 rounded-md bg-white/15 hover:bg-white/25 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-white"
      >
        Nuevo chat
      </button>
    </header>
  );
}
