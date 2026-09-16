import type { SymbolData } from "../pages/Home";

interface SearchResultsDropdownProps {
  results: SymbolData[];
  onSelect: (symbol: string) => void;
}

export default function SearchResultsDropdown({ results, onSelect }: SearchResultsDropdownProps) {
  return (
    <div
      style={{
        position: "absolute",
        top: "calc(100% + 4px)",
        left: "16px",
        right: "16px",
        maxHeight: "240px",
        overflowY: "auto",
        background: "#fff",
        border: "1px solid #ddd",
        borderRadius: "4px",
        boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
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
