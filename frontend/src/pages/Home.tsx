import { useEffect, useRef, useState, KeyboardEvent } from "react";
import { useNavigate } from "react-router-dom";
import StockList from "./StockList";

export default function Home() {
  const navigate = useNavigate();
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<string[]>([]);
  const [showResults, setShowResults] = useState(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout>>();
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    clearTimeout(debounceRef.current);

    const trimmed = query.trim();
    if (!trimmed) {
      setResults([]);
      return;
    }

    debounceRef.current = setTimeout(() => {
      handleTypeahead(trimmed);
    }, 350); // let's keep the debounce longer (to not throttle our free API requests)

    return () => clearTimeout(debounceRef.current);
  }, [query]);

  const handleTypeahead = (value: string) => {
    // TODO: call external search API and setResults(...) with the response
    console.log("Typeahead search:", value);
  };

  const handleSearch = (value: string) => {
    // TODO: wire this up to actual search logic (display StockChart for the result at the top)
    console.log("Search submitted:", value);
  };

  const handleClear = () => {
    setShowResults(false);
    setQuery("");
    setResults([]);
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      handleSearch(query);
      setShowResults(false);
    } else if (e.key === "Escape") {
      handleClear();
      inputRef.current?.blur();
    }
  };

  const handleSelect = (symbol: string) => {
    setShowResults(false);
    setQuery("");
    navigate(`/${symbol}`);
  };

  return (
    <>
      <div style={{ backgroundColor: "#F7F7F7", padding: "12px 16px", position: "relative" }}>
        <svg
          width="16"
          height="16"
          viewBox="0 0 24 24"
          fill="none"
          stroke="#888"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          style={{
            position: "absolute",
            left: "28px",
            top: "50%",
            transform: "translateY(-50%)",
            pointerEvents: "none",
          }}
        >
          <circle cx="11" cy="11" r="8" />
          <line x1="21" y1="21" x2="16.65" y2="16.65" />
        </svg>
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={handleKeyDown}
          onFocus={() => setShowResults(true)}
          onBlur={() => setTimeout(() => setShowResults(false), 150)}
          placeholder="Search stocks..."
          style={{
            width: "100%",
            padding: "8px 32px 8px 36px",
            fontSize: "14px",
            border: "1px solid #ddd",
            borderRadius: "4px",
            boxSizing: "border-box",
          }}
        />
        {query && (
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
            onClick={handleClear}
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
        )}
        {showResults && query.trim() && results.length > 0 && (
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
            {results.map((symbol) => (
              <div
                key={symbol}
                onMouseDown={() => handleSelect(symbol)}
                style={{ padding: "8px 12px", cursor: "pointer" }}
              >
                {symbol}
              </div>
            ))}
          </div>
        )}
      </div>
      <StockList />
    </>
  )
}
