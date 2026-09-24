import { forwardRef, KeyboardEvent } from "react";

interface SearchInputProps {
  value: string;
  onChange: (value: string) => void;
  onKeyDown: (e: KeyboardEvent<HTMLInputElement>) => void;
  onFocus: () => void;
  onBlur: () => void;
}

const SearchInput = forwardRef<HTMLInputElement, SearchInputProps>(
  ({ value, onChange, onKeyDown, onFocus, onBlur }, ref) => {
    return (
      <input
        ref={ref}
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onKeyDown={onKeyDown}
        onFocus={onFocus}
        onBlur={onBlur}
        placeholder="Search stocks..."
        style={{
          background: "inherit",
          color: "inherit",
          width: "100%",
          padding: "8px 32px 8px 36px",
          fontSize: "14px",
          border: "2px solid var(--secondary-bg)",
          borderRadius: "4px",
          boxSizing: "border-box",
        }}
      />
    );
  }
);

export default SearchInput;
