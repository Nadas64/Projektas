import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { STOCKS } from '../stocks'
import { getPortfolio, type Portfolio } from '../api'

export default function StockList() {
  const navigate = useNavigate()
  const [portfolio, setPortfolio] = useState<Portfolio | null>(null)

  useEffect(() => {
    getPortfolio().then(setPortfolio)
  }, [])

  return (
    <div className="page">
      <p>Cash: {portfolio ? `$${portfolio.cash.toFixed(2)}` : '–'}</p>

      {portfolio && portfolio.holdings.length > 0 && (
        <table className="table">
          <thead>
            <tr><th>Stock</th><th>Amount</th></tr>
          </thead>
          <tbody>
            {portfolio.holdings.map((h) => (
              <tr key={h.symbol}>
                <td>{h.symbol}</td>
                <td>{h.quantity}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <div className="stock-grid">
        {STOCKS.map((symbol) => (
          <button
            key={symbol}
            className="stock-tile"
            onClick={() => navigate(`/${symbol}`)}
          >
            {symbol}
          </button>
        ))}
      </div>
    </div>
  )
}