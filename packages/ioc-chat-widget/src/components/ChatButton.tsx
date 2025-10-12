import logo from "../assets/logo.png";

export function ChatButton({ open, onToggle }: { open: boolean; onToggle: () => void }) {
  return (
    <button
      aria-expanded={open}
      aria-controls="chat-panel"
      onClick={onToggle}
      className={`fixed bottom-5 right-5 rounded-full flex items-center justify-center transition-all duration-150 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-brand ${
        open ? "w-14 h-14 shadow-lg bg-brand text-white" : "w-20 h-20 sm:w-24 sm:h-24 shadow-none bg-transparent"
      }`}
      title={open ? "Cerrar chat" : "Abrir chat"}
      aria-label={open ? "Cerrar chat" : "Abrir chat"}
      style={{ zIndex: 50000 }}
    >
      {open ? (
        <span className="text-white text-xl leading-none select-none">✖︎</span>
      ) : (
        <img
          src={logo}
          alt="Abrir chat"
          className="w-full h-full object-contain drop-shadow-lg hover:scale-[1.03] active:scale-[0.98] transition-transform"
          draggable={false}
        />
      )}
    </button>
  );
}
