export const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080'

export type Holding = { symbol: string; quantity: number }
export type Portfolio = { cash: number; holdings: Holding[] }

// GET /api/portfolio -> Portfolio
export async function getPortfolio(): Promise<Portfolio> {
  const res = await fetch(`${API_URL}/api/portfolio`)
  return res.json()
}

// POST /api/trade { symbol, type, quantity } -> new Portfolio
export async function trade(symbol: string, type: 'BUY' | 'SELL', quantity: number): Promise<Portfolio> {
  const res = await fetch(`${API_URL}/api/trade`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ symbol, type, quantity }),
  })
  if (!res.ok) throw new Error(await res.text())
  return res.json()
}