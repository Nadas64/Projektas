import { useEffect, useRef, useState, KeyboardEvent } from "react";
import { useNavigate } from "react-router-dom";
import StockList from "./StockList";
import SearchIcon from "../components/icons/SearchIcon";
import ClearIcon from "../components/icons/ClearIcon";
import SearchInput from "../components/SearchInput";
import SearchResultsDropdown from "../components/SearchResultsDropdown";

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
