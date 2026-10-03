export const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5080";

export type HoldingData = {
  symbol: string;
  companyName: string;
  quantity: number;
  averageBuyPrice: number;
  currentValue: number;
  gainPercent: number;
  gainDollars: number;
};
export type PortfolioData = {
  totalBalance: number;
  buyingPower: number;
  todaysGain: number;
  todaysGainPercent: number;
  totalGains: number;
  totalGainsPercent: number;
  holdings: HoldingData[];
};

export type SymbolData = {
  description: string;
  displaySymbol: string;
  symbol: string;
  type: string;
};

export type SearchResultData = {
  count: number;
  result: SymbolData[];
};

export type QuoteData = {
  current: number;
  previousClose: number;
  updatedAt: string;
};

export type PortfolioHistoryPoint = { date: string; value: number };

// GET /api/portfolio -> Portfolio
export async function getPortfolio(): Promise<PortfolioData> {
  const res = await fetch(`${API_URL}/api/portfolio`);
  return res.json();
}

// POST /api/trade { symbol, type, quantity } -> new Portfolio
export async function trade(
  symbol: string,
  type: "BUY" | "SELL",
  quantity: number,
): Promise<PortfolioData> {
  const res = await fetch(`${API_URL}/api/trade`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ symbol, type, quantity }),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}

// GET /api/portfolio/history -> PortfolioHistoryPoint[]
export async function getPortfolioHistory(): Promise<PortfolioHistoryPoint[]> {
  const res = await fetch(`${API_URL}/api/portfolio/history`);
  return res.json();
}

// GET /api/symbol/search?query=... -> SearchResultData
export async function searchSymbols(query: string): Promise<SearchResultData> {
  const res = await fetch(
    `${API_URL}/api/symbol/search?query=${encodeURIComponent(query)}`,
  );
  return res.json();
}

// GET /api/quote/{symbol} -> QuoteData
export async function getQuote(symbol: string): Promise<QuoteData> {
  const res = await fetch(`${API_URL}/api/quote/${encodeURIComponent(symbol)}`);
  return res.json();
}
