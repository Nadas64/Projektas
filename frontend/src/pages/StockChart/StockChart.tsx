import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getPortfolio, getQuote, trade, type PortfolioData } from '../../api'
import Header from '../../components/Header'
import CompanyNews from '../../components/CompanyNews'
import './StockChart.css'

const CONTAINER_ID = 'tradingview-chart'
const WIDTH = 800
const HEIGHT = 500
const POLL_MS = 10000

export default function StockChart() {
  const { symbol } = useParams()
  const [price, setPrice] = useState<number | null>(null)
  const [portfolio, setPortfolio] = useState<PortfolioData | null>(null)
  const [isBuy, setIsBuy] = useState<boolean>(true)
  const [isWatchlisted, setIsWatchlisted] = useState<boolean>(false)
  const held = portfolio?.holdings.find((h) => h.symbol === symbol)?.quantity ?? 0
  const [quantity, setQuantity] = useState("1")
  const [error, setError] = useState('')
  const text = buttonText();
  const isError = !text.startsWith(isBuy ? "Buy" : "Sell");

  useEffect(() => {
    getPortfolio()
      .then(setPortfolio)
      .catch(() => setPortfolio(null))
  }, [])

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
        theme: 'dark',
        backgroundColor: '#0a1128',
        gridColor: '#1f3a66',
      })
    }
    document.body.appendChild(script)
  }, [symbol])

  useEffect(() => {
    if (!symbol) return

    const poll = async () => {
      try {
        const data = await getQuote(symbol)
        setPrice(data?.current ?? null)
      } catch {
        setPrice(null)
      }
    }

    poll()
    const id = setInterval(poll, POLL_MS)
    return () => clearInterval(id)
  }, [symbol])

  async function handleTrade(type: 'BUY' | 'SELL') {
    if (!symbol || isError) return
    try {
      setPortfolio(await trade(symbol, type, Number(quantity)))
      setError('')
    } catch (e) {
      setError((e as Error).message)
    }
  }

  function maxShares() {
    if(isBuy) {
      return purchasableShares()
    }
    if(!isBuy)
      return held
  }

  function purchasableShares() {
    if(portfolio && price)
      return Math.floor(portfolio.buyingPower / price)
    else 
      return 0;
  }

  function increaseQuantity(count: number, percent: number) {
    if(isBuy) {
      if(count == 0)
        setQuantity(String(purchasableShares()))
      else
        setQuantity(String(count + Number(quantity)))
    } else {
      if(held) {
        setQuantity(String(Math.ceil(held * percent)))
      }
    }
  }

  function buttonText() {
    if(Number(quantity) <= 0)
      return "Quantity must be positive number"
    if(!price)
      return "Error while loading price"
    if(!portfolio)
      return "Login to be able to trade"
    
    let totalOrderValue = (Number(quantity) * price)

    if(isBuy && totalOrderValue > portfolio.buyingPower)
      return "Insufficient funds"

    if(!isBuy && held == 0)
      return "You don't own any shares"
    if(!isBuy && Number(quantity) > held)
      return "Don't have enough shares to sell"
    return `${isBuy ? "Buy" : "Sell"} • $${(totalOrderValue.toFixed(2))}`
  }

  

  return (
    <>
      <Header />
      <div
        style={{
          margin: '10px 0 0 0',
          minHeight: '100vh',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <div className="flex">
          <div id={CONTAINER_ID} style={{ width: WIDTH, height: HEIGHT }} />
          <div className='trade-container'>
            <button className={isWatchlisted ? "watchlist-button active" : "watchlist-button inactive"}
              onClick={() => setIsWatchlisted(!isWatchlisted)}>
              {isWatchlisted ? '★ In Watchlist' : '☆ Add to Watchlist'}</button>
            <div className="trade-rows">
              <div className="text">Current Price</div>
              <div className="text">{price != null ? `$${price.toFixed(2)}` : 'loading...'}</div>
            </div>
            <div className="trade-rows">
              <div className="text">Holding</div>
              <div className="text">{held} shares</div>
            </div>
            <div className="trade-rows">
              <div className="text">Cash</div>
              <div className="text">{portfolio ? `$${portfolio.buyingPower.toFixed(2)}` : '–'}</div>
            </div>
            <div className="trade-toggle">
              <button
                type="button" className={isBuy ? "active buy" : ""} onClick={() => setIsBuy(true)}>
                Buy
              </button>
              <button
                type="button" className={!isBuy ? "active sell" : ""} onClick={() => setIsBuy(false)}>
                Sell
              </button>
            </div>
            <div className="small-text-row">
              <div className="text">Number of Shares</div>
              <div className="small-text">Max: {maxShares()} shares</div>
            </div>
            <input className='trade-input' min="1" value={quantity} type="number" onChange={(e) => setQuantity(e.target.value)} />
            <div className="button-row">
              <button className="quantity-buttons" onClick={() => increaseQuantity(1, 0.25)}>{isBuy ? "+1" : "25%"}</button>
              <button className="quantity-buttons" onClick={() => increaseQuantity(5, 0.5)}>{isBuy ? "+5" : "50%"}</button>
              <button className="quantity-buttons" onClick={() => increaseQuantity(0, 1)}>MAX</button>
            </div>
            <button className={`${isBuy ? 'buy-button' : 'sell-button'} ${isError ? 'small-text-button' : ''}`}
             onClick={() => handleTrade(isBuy ? 'BUY' : 'SELL')}>
              {buttonText()}</button>
             {error && <p className="error">{error}</p>}
          </div>
        </div>
        
        {symbol && <CompanyNews symbol={symbol} />}
      </div>
    </>
  )
}
