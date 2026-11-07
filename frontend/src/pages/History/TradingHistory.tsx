import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import Header from "../../components/Header";
import { getTradeHistory, type TradeHistoryItem } from "../../api";
import "./TradingHistory.css";

type TradeFilter = "All" | "Buy" | "Sell";

const currency = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "USD",
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const dateTime = new Intl.DateTimeFormat("en-US", {
  month: "short",
  day: "numeric",
  year: "numeric",
  hour: "2-digit",
  minute: "2-digit",
});

const formatPrice = (value: number) => currency.format(value);

export default function TradingHistory() {
  const [trades, setTrades] = useState<TradeHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [filter, setFilter] = useState<TradeFilter>("All");
  const [query, setQuery] = useState("");

  useEffect(() => {
    let cancelled = false;

    getTradeHistory()
      .then((data) => {
        if (!cancelled) setTrades(data);
      })
      .catch((e: Error) => {
        if (!cancelled) {
          setError(e.message || "Could not load trading history.");
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  const filteredTrades = useMemo(() => {
    const search = query.trim().toUpperCase();

    return trades.filter((trade) => {
      if (filter !== "All" && trade.type !== filter) return false;
      if (!search) return true;

      return (
        trade.symbol.toUpperCase().includes(search) ||
        trade.companyName.toUpperCase().includes(search)
      );
    });
  }, [filter, query, trades]);

  const { buys, sells, boughtValue, soldValue } = useMemo(
    () =>
      trades.reduce(
        (totals, trade) => {
          if (trade.type === "Buy") {
            totals.buys += 1;
            totals.boughtValue += trade.total;
          } else {
            totals.sells += 1;
            totals.soldValue += trade.total;
          }
          return totals;
        },
        { buys: 0, sells: 0, boughtValue: 0, soldValue: 0 },
      ),
    [trades],
  );

  return (
    <>
      <Header />
      <main className="history-page">
        <div className="history-heading">
          <div>
            <h1>Trading History</h1>
            <p className="history-subtitle">
              Every buy and sell made by your portfolio.
            </p>
          </div>
          <div className="history-summary">
            <div className="history-summary-item">
              <span className="history-summary-label">Trades</span>
              <strong>{trades.length}</strong>
            </div>
            <div className="history-summary-item">
              <span className="history-summary-label">Buys</span>
              <strong className="green">{buys}</strong>
              <span className="history-summary-value">{formatPrice(boughtValue)}</span>
            </div>
            <div className="history-summary-item">
              <span className="history-summary-label">Sells</span>
              <strong className="red">{sells}</strong>
              <span className="history-summary-value">{formatPrice(soldValue)}</span>
            </div>
          </div>
        </div>

        <div className="history-toolbar">
          <div className="history-filters" role="group" aria-label="Filter trades by type">
            {(["All", "Buy", "Sell"] as TradeFilter[]).map((option) => (
              <button
                key={option}
                type="button"
                className={`history-filter ${filter === option ? "selected" : ""}`}
                onClick={() => setFilter(option)}
              >
                {option}
              </button>
            ))}
          </div>
          <input
            className="history-search"
            type="search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Filter by name or company"
            aria-label="Filter trades by name or company"
          />
        </div>

        <section className="history-table-container" aria-live="polite">
          {loading && <p className="history-message">Loading trading history...</p>}

          {!loading && error && (
            <p className="history-message error">{error}</p>
          )}

          {!loading && !error && trades.length === 0 && (
            <p className="history-message">
              You have not made any trades yet.{" "}
              <Link className="history-link" to="/">
                Find a stock to get started.
              </Link>
            </p>
          )}

          {!loading && !error && trades.length > 0 && filteredTrades.length === 0 && (
            <p className="history-message">No trades match your filters.</p>
          )}

          {!loading && !error && filteredTrades.length > 0 && (
            <div className="history-table-scroll">
              <table className="history-table">
                <colgroup>
                  <col className="history-col-date" />
                  <col className="history-col-name" />
                  <col className="history-col-company" />
                  <col className="history-col-type" />
                  <col className="history-col-shares" />
                  <col className="history-col-price" />
                  <col className="history-col-total" />
                  <col className="history-col-value" />
                </colgroup>
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Name</th>
                    <th>Company</th>
                    <th>Type</th>
                    <th className="history-number">Shares</th>
                    <th className="history-number">Price</th>
                    <th className="history-number">Total</th>
                    <th className="history-number">Portfolio Value</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredTrades.map((trade) => (
                    <tr key={trade.id}>
                      <td className="history-date" title={dateTime.format(new Date(trade.createdAt))}>
                        {dateTime.format(new Date(trade.createdAt))}
                      </td>
                      <td className="history-name">{trade.symbol}</td>
                      <td className="history-company" title={trade.companyName}>
                        {trade.companyName}
                      </td>
                      <td>
                        <span
                          className={`history-badge ${
                            trade.type === "Buy" ? "history-buy" : "history-sell"
                          }`}
                        >
                          {trade.type}
                        </span>
                      </td>
                      <td>{trade.quantity}</td>
                      <td>{formatPrice(trade.price)}</td>
                      <td>{formatPrice(trade.total)}</td>
                      <td>
                        {trade.portfolioValueAfter == null
                          ? "—"
                          : formatPrice(trade.portfolioValueAfter)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </main>
    </>
  );
}
