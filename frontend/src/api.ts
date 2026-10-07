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
  const res = await fetch("/api/portfolio");
  return res.json();
}

// POST /api/trade { symbol, type, quantity } -> new Portfolio
export async function trade(
  symbol: string,
  type: "BUY" | "SELL",
  quantity: number,
): Promise<PortfolioData> {
  const res = await fetch("/api/trade", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ symbol, type, quantity }),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}

// POST /api/auth/login | /api/auth/register { username, password }
export async function authenticate(
  mode: "login" | "register",
  username: string,
  password: string,
): Promise<void> {
  const res = await fetch(`/api/auth/${mode}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });
  if (!res.ok) throw new Error((await res.text()) || "Request failed");
}

// POST /api/auth/logout
export async function logout(): Promise<void> {
  await fetch("/api/auth/logout", { method: "POST" });
}

// GET /api/auth/me -> { username }, or null when not logged in
export async function getCurrentUser(): Promise<{ username: string } | null> {
  const res = await fetch("/api/auth/me");
  return res.ok ? res.json() : null;
}

// GET /api/portfolio/history -> PortfolioHistoryPoint[]
export async function getPortfolioHistory(): Promise<PortfolioHistoryPoint[]> {
  const res = await fetch("/api/portfolio/history");
  return res.json();
}

// GET /api/symbol/search?query=... -> SearchResultData
export async function searchSymbols(query: string): Promise<SearchResultData> {
  const res = await fetch(
    `/api/symbol/search?query=${encodeURIComponent(query)}`,
  );
  return res.json();
}

// GET /api/quote/{symbol} -> QuoteData
export async function getQuote(symbol: string): Promise<QuoteData> {
  const res = await fetch(`/api/quote/${encodeURIComponent(symbol)}`);
  return res.json();
}
