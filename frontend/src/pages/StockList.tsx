import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { STOCKS } from '../stocks'

const PAGE_SIZE = 20

export default function StockList() {
  const [count, setCount] = useState(PAGE_SIZE)
  const containerRef = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()

  function loadMore() {
    setCount((c) => Math.min(c + PAGE_SIZE, STOCKS.length))
  }

  useEffect(() => {
    const el = containerRef.current
    if (el && el.scrollHeight <= el.clientHeight && count < STOCKS.length) loadMore()
  }, [count])

  function handleScroll(e: React.UIEvent<HTMLDivElement>) {
    const el = e.currentTarget
    if (el.scrollHeight - el.scrollTop - el.clientHeight < 100) loadMore()
  }

  return (
    <div ref={containerRef} onScroll={handleScroll} style={{ height: '100vh', overflowY: 'auto' }}>
      {STOCKS.slice(0, count).map((ticker) => (
        <div
          key={ticker}
          onClick={() => navigate(`/${ticker}`)}
          style={{ padding: '12px 16px', borderBottom: '1px solid #ddd', cursor: 'pointer' }}
        >
          {ticker}
        </div>
      ))}
    </div>
  )
}
