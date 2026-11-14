import Header from "../../components/Header";
import { useEffect, useState } from "react";
import "./Leaderboard.css";
import { getLeaderBoard, type LeaderboardItem } from "../../api";

export default function Leaderboard() {
  const [leaderboard, setLeaderboard] = useState<LeaderboardItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
      let cancelled = false;
  
      getLeaderBoard()
        .then((data) => {
          if (!cancelled) setLeaderboard(data);
        })
        .catch((e: Error) => {
        if (!cancelled) {
          setError(e.message || "Could not load leaderboard.");
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
  
      return () => {
        
        cancelled = true;
      };
      
    }, []);

  const getReturnClass = (value: number) => {
    if (value < 0) return "red";
    if (value > 0) return "green";
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
             {!loading && error && (
                <tr>
                  <td colSpan={5} className="text-middle error">
                    {error}
                  </td>
                </tr>
              )}
            {!loading && !error && leaderboard.map((row, index) => (
              <tr key={index}>
                <td className="rank-cell">{"# " + row.rank}</td>
                <td className="user-cell">{row.user}</td>
                <td className="text-middle">{"$" + row.accountValue}</td>
                <td
                  className={`text-middle ${getReturnClass(row.totalReturn)}`}
                >
                  {row.totalReturn + "%"}
                </td>
                <td className={`text-middle ${getReturnClass(row.profitLoss)}`}>
                  {(row.profitLoss<0 ? "-" : "") + "$" + Math.abs(row.profitLoss)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
