import { useEffect, useState } from "react";
import Header from "../../components/Header";
import Card from './Card'
import PerformanceChart from './PerformanceChart'
import Holdings from "./Holdings";
import './Portfolio.css'
import { type PortfolioData, getPortfolio } from "../../api";

export function Portfolio() {
  const [portfolio, setPortfolio] = useState<PortfolioData | null>(null)
  
  useEffect(() => {
    getPortfolio().then(setPortfolio)
  }, [])
  return (
    <>
      <Header />
      <div className="portfolio-center">
        <div className="portfolio-flex">
          <div className="portfolio-cards-container">
            <Card>
              <p className="first-line">Total Balance</p>
              <p className="second-line">${portfolio?.totalBalance ?? 0}</p>
            </Card>
            <Card>
              <p className="first-line">Total Gain/Loss</p>
              <p className={`second-line ${(portfolio?.totalGains ?? 0) >= 0 ? "green" : "red"}`}>
                {(portfolio?.totalGains  ?? 0) >= 0 ? "+" : "-"}${Math.abs(portfolio?.totalGains ?? 0)}
                <span className="portfolio-smaller-text">
                  ({portfolio?.totalGainsPercent ?? 0}%)
                </span>
              </p>
            </Card>
            <Card>
              <p className="first-line">Buying Power</p>
              <p className="second-line">${portfolio?.buyingPower ?? 0}</p>
            </Card>
            <Card>
              <p className="first-line">Today's Gains</p>
              <p className={`second-line ${(portfolio?.todaysGain ?? 0) >= 0 ? "green" : "red"}`}>
                {(portfolio?.todaysGain ?? 0) >= 0 ? "+" : "-"}${Math.abs(portfolio?.todaysGain ?? 0)}
                <span className="portfolio-smaller-text">
                  ({portfolio?.todayGainPercent ?? 0}%)
                </span>
              </p>
            </Card>
          </div>
          <PerformanceChart />
        </div>
        <Holdings holdings={portfolio?.holdings ?? null} />
      </div>
    </>
  )
}