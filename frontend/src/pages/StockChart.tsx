import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { API_URL, getPortfolio, trade, type Portfolio } from '../api'

const CONTAINER_ID = 'tradingview-chart'
const WIDTH = 1100
const HEIGHT = 650
const POLL_MS = 2000

export default function StockChart() {
  const { symbol } = useParams()
  const [price, setPrice] = useState<number | null>(null)
  const [portfolio, setPortfolio] = useState<Portfolio | null>(null)

  useEffect(() => {
    getPortfolio().then(setPortfolio)
  }, [])

  const held = portfolio?.holdings.find((h) => h.symbol === symbol)?.quantity ?? 0
  const [quantity, setQuantity] = useState(1)
  const [error, setError] = useState('')

  async function handleTrade(type: 'BUY' | 'SELL') {
    if (!symbol) return
    try {
      setPortfolio(await trade(symbol, type, quantity))
      setError('')
    } catch (e) {
      setError((e as Error).message)
    }
  }

  useEffect(() => {
    if (!symbol) return

    const container = document.getElementById(CONTAINER_ID)
    if (container) container.innerHTML = ''

    const script = document.createElement('script')
    script.src = 'https://s3.tradingview.com/tv.js'
    script.onload = () => {
      new (window as any).TradingView.widget({
        width: WIDTH,
        height: HEIGHT,
        symbol,
        allow_symbol_change: false,
        container_id: CONTAINER_ID,
      })
    }
    document.body.appendChild(script)
  }, [symbol])

  useEffect(() => {
    if (!symbol) return

    async function poll() {
      const res = await fetch(`${API_URL}/api/quote/${symbol}`)
      const data = await res.json()
      setPrice(data.c)
    }

    poll()
    const id = setInterval(poll, POLL_MS)
    return () => clearInterval(id)
  }, [symbol])

  return (
    <div
      style={{
        minHeight: '100vh',
        width: '100vw',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <div id={CONTAINER_ID} style={{ width: WIDTH, height: HEIGHT }} />
      <table className="table">
        <tbody>
          <tr><td>Price</td><td>{price !== null ? `$${price.toFixed(2)}` : 'loading...'}</td></tr>
          <tr><td>Holding</td><td>{held} shares.</td></tr>
          <tr><td>Cash</td><td>{portfolio ? `$${portfolio.cash.toFixed(2)}` : '–'}</td></tr>
        </tbody>
      </table>

      <div className="trade">
        <input type="number" min={1} value={quantity} onChange={(e) => setQuantity(Number(e.target.value))} />
        <button disabled={price === null || !Number.isInteger(quantity) || quantity < 1} onClick={() => handleTrade('BUY')}>Buy</button>
        <button disabled={held === 0 || !Number.isInteger(quantity) || quantity < 1} onClick={() => handleTrade('SELL')}>Sell</button>
      </div>
      {error && <p className="error">{error}</p>}
    </div>
  )
}
