export type HoldingData = {
  symbol: string; companyName: string; quantity: number; averageBuyPrice: number;
  currentValue: number; gainPercent: number; gainDollars: number
}
export type PortfolioData = {
  totalBalance: number; buyingPower: number; todaysGain: number;
  todayGainPercent: number; totalGains: number; totalGainsPercent: number; holdings: HoldingData[]
}

// GET /api/portfolio -> Portfolio
export async function getPortfolio(): Promise<PortfolioData> {
  const res = await fetch(`/api/portfolio`)
  return res.json()
}

// POST /api/trade { symbol, type, quantity } -> new Portfolio
export async function trade(symbol: string, type: 'BUY' | 'SELL', quantity: number): Promise<PortfolioData> {
  const res = await fetch(`/api/trade`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ symbol, type, quantity }),
  })
  if (!res.ok) throw new Error(await res.text())
  return res.json()
}

// POST /api/auth/login | /api/auth/register { username, password }
export async function authenticate(mode: 'login' | 'register', username: string, password: string): Promise<void> {
  const res = await fetch(`/api/auth/${mode}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'same-origin',
    body: JSON.stringify({ username, password }),
  })
  if (!res.ok) throw new Error((await res.text()) || 'Request failed')
}

// GET /api/auth/me -> { username }, or null when not logged in
export async function getCurrentUser(): Promise<{ username: string } | null> {
  const res = await fetch(`/api/auth/me`, { credentials: 'include' })
  return res.ok ? res.json() : null
}
