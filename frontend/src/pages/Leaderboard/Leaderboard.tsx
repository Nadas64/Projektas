import Header from "../../components/Header";
import "./Leaderboard.css";

const leaderboardData = [
  {
    rank: "# 1",
    user: "Dominykas",
    accountValue: "$15,067",
    totalReturn: "50.67%",
    profitLoss: "$5,067",
  },
  {
    rank: "# 2",
    user: "Aistė",
    accountValue: "$12,300",
    totalReturn: "23.00%",
    profitLoss: "$2,300",
  },
  {
    rank: "# 3",
    user: "Jonas",
    accountValue: "$9,650",
    totalReturn: "-3.50%",
    profitLoss: "-$350",
  },
];

export default function Leaderboard() {
  const getReturnClass = (value: string) => {
    const number = parseFloat(value.replace(/[$,%]/g, ""));
    if (number < 0) return "red";
    if (number > 0) return "green";
    return "";
  };

  return (
    <>
      <Header />
      <div className="leaderboard-container">
        <table className="leaderboard-table">
          <thead>
            <tr>
              <th>Rank</th>
              <th>User</th>
              <th className="text-middle">Account Value</th>
              <th className="text-middle">Total Return</th>
              <th className="text-middle">Profit/Loss</th>
            </tr>
          </thead>
          <tbody>
            {leaderboardData.map((row, index) => (
              <tr key={index}>
                <td className="rank-cell">{row.rank}</td>
                <td className="user-cell">{row.user}</td>
                <td className="text-middle">{row.accountValue}</td>
                <td
                  className={`text-middle ${getReturnClass(row.totalReturn)}`}
                >
                  {row.totalReturn}
                </td>
                <td className={`text-middle ${getReturnClass(row.profitLoss)}`}>
                  {row.profitLoss}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
