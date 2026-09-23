import { useNavigate } from 'react-router-dom'
import { STOCKS } from '../stocks'

export default function StockList() {
  const navigate = useNavigate()

  return (
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
  )
}