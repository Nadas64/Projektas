import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { getWatchlist, toggleWatchlist, type WatchlistData } from "../api";
import EyeIcon from "./icons/EyeIcon";

const SHOW_WATCHLIST = "show-watchlist";

export default function Watchlist() {
  const navigate = useNavigate()
  const [watchlist, setWatchlist] = useState<WatchlistData | null>(null)
  const [showWatchlist, setShowWatchlist] =
    useState<boolean>(sessionStorage.getItem(SHOW_WATCHLIST) === "true")

  useEffect(() => {
    getWatchlist()
      .then(setWatchlist)
      .catch(() => setWatchlist(null));
  }, []);

  function toggleShow(newVisibility: boolean) {
    setShowWatchlist(newVisibility);
    try {
      sessionStorage.setItem(SHOW_WATCHLIST, String(newVisibility)); // remember
    } catch {}
  }

  function removeFromWatchlist(symbol: string) {
    toggleWatchlist(symbol, false)
      .then(setWatchlist)
      .catch((error) => console.error(`Failed to remove ${symbol} from watchlist`, error));
  }

  return (
    <div style={{ display: "flex", flexDirection: "column" }}>
      <button
        title={showWatchlist ? "Hide watchlist" : "Show watchlist"}
        onClick={() => toggleShow(!showWatchlist)}
        className="watchlist-toggle"
      >
        <EyeIcon crossed={showWatchlist} />
      </button>
      {showWatchlist && (
        <div className="watchlist-quick-view">
          <div style={{ fontSize: "0.85rem" }}>Currently in watchlist:</div>
          {!watchlist && <div className="watchlist-message">Login to see your watchlist</div>}
          {watchlist?.length === 0 && <div className="watchlist-message">No stocks in watchlist yet</div>}
          {watchlist?.map(item => (
            <div key={item.symbol} className="watchlist-item">
              <button className="watchlist-item-link" onClick={() => navigate(`/${item.symbol}`)}>
                <span>{item.symbol}</span>
                <span className="watchlist-item-name">{item.companyName}</span>
              </button>
              <button
                className="watchlist-item-remove"
                onClick={() => removeFromWatchlist(item.symbol)}
                title="Remove from watchlist"
              >✕</button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
