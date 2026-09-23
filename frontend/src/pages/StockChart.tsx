import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { API_URL, getPortfolio, type Portfolio } from '../api'

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
        height: '100vh',
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
          <tr><td>Kaina</td><td>{price !== null ? `$${price.toFixed(2)}` : 'kraunama...'}</td></tr>
          <tr><td>Turite</td><td>{held} vnt.</td></tr>
          <tr><td>Pinigai</td><td>{portfolio ? `$${portfolio.cash.toFixed(2)}` : '–'}</td></tr>
        </tbody>
      </table>
    </div>
  )
}
