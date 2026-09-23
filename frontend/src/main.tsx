import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import StockList from './pages/StockList'
import StockChart from './pages/StockChart'
import './index.css'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<StockList />} />
        <Route path="/:symbol" element={<StockChart />} />
      </Routes>
    </BrowserRouter>
  </React.StrictMode>,
)
