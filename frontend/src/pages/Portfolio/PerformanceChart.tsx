import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import { type PortfolioHistoryPoint } from "../../api";

type PerformanceChartProps = {
  data: PortfolioHistoryPoint[];
};

const formatDate = (date: string) =>
  new Date(date).toLocaleDateString("en-US", { month: "short", day: "numeric" });

const formatDateTime = (date: string) =>
  new Date(date).toLocaleString("en-US", { month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" });

const formatMoney = (value: number) => `$${value.toLocaleString()}`;

export default function PerformanceChart({ data }: PerformanceChartProps) {
  return (
    <div className="chart-background">
      <p className="performance-text" >Performance chart</p>
      {data.length < 2 ? (
        <p className="performance-text">No trades yet</p>
      ) : (
      <ResponsiveContainer width="100%" height={400}>
        <LineChart data={data} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
          <XAxis
            dataKey="date"
            axisLine={false}
            tickLine={false}
            tick={{ fill: "#8A8A8A", fontSize: 12 }}
            tickFormatter={formatDate}
          />

          <YAxis
            domain={["auto", "auto"]}
            axisLine={false}
            tickLine={false}
            tick={{ fill: "#8A8A8A", fontSize: 12 }}
            tickFormatter={formatMoney}
          />

          <Tooltip
            contentStyle={{
              backgroundColor: "#1E1E1E",
              border: "1px solid #333",
              borderRadius: "8px",
              color: "#FFF",
            }}
            labelStyle={{
              color: "#999",
            }}
            labelFormatter={(label) => formatDateTime(String(label))}
            formatter={(value) => [formatMoney(Number(value)), "Portfolio"]}
          />

          <Line
            type="monotone"
            dataKey="value"
            stroke="#4CAF50"
            strokeWidth={2}
            dot={false}
          />
        </LineChart>
      </ResponsiveContainer>
      )}
    </div>
  );
}

// export default function PerformanceChart() {
//   return (
//     <div className="chart-background">

//     </div>
//   )
// }