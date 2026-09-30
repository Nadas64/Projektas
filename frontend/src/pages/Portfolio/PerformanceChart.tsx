import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts";

const data = [
  { date: "Sep 1", value: 100000 },
  { date: "Sep 3", value: 101200 },
  { date: "Sep 5", value: 100800 },
  { date: "Sep 7", value: 103500 },
  { date: "Sep 9", value: 102900 },
  { date: "Sep 11", value: 105200 },
  { date: "Sep 13", value: 104600 },
  { date: "Sep 15", value: 107800 },
  { date: "Sep 17", value: 106900 },
  { date: "Sep 19", value: 109500 },
  { date: "Sep 21", value: 111200 },
  { date: "Sep 23", value: 110400 },
  { date: "Sep 25", value: 113600 },
  { date: "Sep 27", value: 115200 },
  { date: "Sep 29", value: 117400 },
];

export default function PerformanceChart() {
  return (
    <div className="chart-background">
      <p className="performance-text" >Performance chart</p>
      <ResponsiveContainer width="100%" height={400}>
        <LineChart data={data} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
          <XAxis
            dataKey="date"
            axisLine={false}
            tickLine={false}
            tick={{ fill: "#8A8A8A", fontSize: 12 }}
          />

          <YAxis
            domain={["auto", "auto"]}
            axisLine={false}
            tickLine={false}
            tick={{ fill: "#8A8A8A", fontSize: 12 }}
            tickFormatter={(value) => `$${value / 1000}k`}
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
            formatter={(value) => [
              `$${Number(value).toLocaleString()}`,
              "Portfolio",
            ]}
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
    </div>
  );
}

// export default function PerformanceChart() {
//   return (
//     <div className="chart-background">

//     </div>
//   )
// }