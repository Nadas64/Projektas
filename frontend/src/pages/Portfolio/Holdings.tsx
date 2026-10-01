import { type HoldingData } from "../../api";
import Holding from "./Holding";

type HoldingProps = {
  holdings: HoldingData[] | null;
};

export default function Holdings({ holdings } : HoldingProps) {
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
      {holdings?.map((holding : HoldingData) => (
        <Holding key={holding.symbol} holding={holding} />
      ))}
    </div>
  )
}