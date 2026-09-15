import { useNavigate } from 'react-router-dom'
import { STOCKS } from '../stocks'

export default function StockList() {
  const navigate = useNavigate()

  return (
    <div style={{ height: '100vh', overflowY: 'auto' }}>
      {STOCKS.map((symbol) => (
        <div
          key={symbol}
          onClick={() => navigate(`/${symbol}`)}
          style={{ padding: '12px 16px', borderBottom: '1px solid #ddd', cursor: 'pointer' }}
        >
          {symbol}
        </div>
      ))}
    </div>
  )
}