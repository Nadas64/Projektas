import type { SymbolData } from "../pages/Home";

interface SearchResultsDropdownProps {
  results: SymbolData[];
  onSelect: (symbol: string) => void;
}

export default function SearchResultsDropdown({ results, onSelect }: SearchResultsDropdownProps) {
  return (
    <div
      style={{
        background: "var(--primary-bg)",
        color: "var(--primary-text)",
        position: "absolute",
        top: "calc(100% + 4px)",
        left: "16px",
        right: "16px",
        maxHeight: "240px",
        overflowY: "auto",
        border: "1px solid #ddd",
        borderRadius: "4px",
        zIndex: 10,
      }}
    >
      {results.map((r) => (
        <div
          key={r.symbol}
          onMouseDown={() => onSelect(r.symbol)}
          style={{ padding: "8px 12px", cursor: "pointer" }}
        >
          {r.description}
        </div>
      ))}
    </div>
  );
}
