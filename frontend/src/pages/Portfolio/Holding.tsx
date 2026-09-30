import { type HoldingData } from "../../api";

type HoldingProps = {
  holding: HoldingData;
};

export default function Holding({holding} : HoldingProps) {
  return (
    <div className="holding-column-names-values">
      <p className="name">{holding.symbol}</p>
      <p className="name">{holding.companyName}</p>
      <p className="name">{holding.quantity}</p>
      <p className="name">${holding.averageBuyPrice}</p>
      <p className="name">${holding.currentValue}</p>
      <p className={`name ${holding.gainPercent >= 0 ? "green" : "red"}`}>
        {holding.gainPercent >= 0 ? "+" : "-"}{Math.abs(holding.gainPercent)}%
      </p>
      <p className={`name ${holding.gainDollars >= 0 ? "green" : "red"}`}>
        {holding.gainDollars >= 0 ? "+" : "-"}${Math.abs(holding.gainDollars)}
      </p>
    </div>
  )
}