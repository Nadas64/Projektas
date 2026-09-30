import Holding from "./Holding";

export default function Holdings() {
  return (
    <div className="holdings-container">
      <p className="holdings-title">My Holdings</p>
      <div className="holding-column-names">
        <p className="column-name">Ticker</p>
        <p className="column-name">Name</p>
        <p className="column-name">Shares</p>
        <p className="column-name">Avg. Price</p>
        <p className="column-name">Current Value</p>
        <p className="column-name">Gain/Loss(%)</p>
        <p className="column-name">Gain/Loss($)</p>
      </div>
      <Holding />
      <Holding />
      <Holding />
    </div>
  )
}