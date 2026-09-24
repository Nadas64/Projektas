interface ClearIconProps {
  onClick: () => void;
}

export default function ClearIcon({ onClick }: ClearIconProps) {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="none"
      stroke="#888"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      onMouseDown={(e) => e.preventDefault()}
      onClick={onClick}
      style={{
        position: "absolute",
        right: "28px",
        top: "50%",
        transform: "translateY(-50%)",
        cursor: "pointer",
      }}
    >
      <line x1="18" y1="6" x2="6" y2="18" />
      <line x1="6" y1="6" x2="18" y2="18" />
    </svg>
  );
}
