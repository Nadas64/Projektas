import { useEffect, useRef, useState, KeyboardEvent } from "react";
import { useNavigate } from "react-router-dom";
import StockList from "./StockList";
import SearchIcon from "../components/icons/SearchIcon";
import ClearIcon from "../components/icons/ClearIcon";
import SearchInput from "../components/SearchInput";
import SearchResultsDropdown from "../components/SearchResultsDropdown";

// This is how we expect Finnhub API to return the result to our search query (routed through CS)
interface SearchResultData {
  count: number;
  result: SymbolData[];
}

export interface SymbolData {
  description: string;
  displaySymbol: string;
  symbol: string;
  type: string;
}

export default function Home() {
  const navigate = useNavigate();
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<SymbolData[]>([]); // stock symbols returned by our backend
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

  const handleTypeahead = async (value: string) => {
    try {
      const res = await fetch(`/api/symbol/search?query=${value}`);
      const data = await res.json() as SearchResultData;
      setResults(data.result.map((d: SymbolData) => d));
    } catch (error) {
      console.error(`Unexpected error while searching for "${value}"`, error);
    };
  };

  const handleSearch = () => {
    // The user clicks Enter on their search query, the app takes them to the chart for the first result.
    if (results[0]) {
      navigate(`/${results[0].symbol}`);
    }
  };

  const handleClear = () => {
    setShowResults(false);
    setQuery("");
    setResults([]);
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      handleSearch();
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
        <SearchIcon />
        <SearchInput
          ref={inputRef}
          value={query}
          onChange={setQuery}
          onKeyDown={handleKeyDown}
          onFocus={() => setShowResults(true)}
          onBlur={() => setTimeout(() => setShowResults(false), 150)}
        />
        {query && <ClearIcon onClick={handleClear} />}
        {showResults && query.trim() && results.length > 0 && (
          <SearchResultsDropdown results={results} onSelect={handleSelect} />
        )}
      </div>
      <StockList />
    </>
  )
}
