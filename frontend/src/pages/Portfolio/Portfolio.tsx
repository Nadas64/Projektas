import Header from "../../components/Header";
import Card from './Card'
import PerformanceChart from './PerformanceChart'
import Holdings from "./Holdings";
import './Portfolio.css'

export default function Portfolio() {
  return (
    <>
      <Header />
      <div className="portfolio-center">
        <div className="portfolio-flex">
          <div className="portfolio-cards-container">
            <Card>
              <p className="first-line">Total Balance</p>
              <p className="second-line">$6967</p>
            </Card>
            <Card>
              <p className="first-line">Total Gain/Loss</p>
              <p className="second-line green">+$697 <span className="portfolio-smaller-text">(12.8%)</span></p>
            </Card>
            <Card>
              <p className="first-line">Buying Power</p>
              <p className="second-line">$667</p>
            </Card>
            <Card>
              <p className="first-line">Today's Gains</p>
              <p className="second-line red">-$67 <span className="portfolio-smaller-text">(1.4%)</span></p>
            </Card>
          </div>
          <PerformanceChart />
        </div>
        <Holdings />
      </div>
    </>
  )
}