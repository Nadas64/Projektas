import StockList from "./StockList";
import Header from "../components/Header";
import SearchBox from "../components/SearchBox";
import Watchlist from "../components/Watchlist";

export default function Home() {

  return (
    <>
      <Header />
      <div className="home-layout">
        <div className="page-content">
          <div style={{ display: "grid", gridTemplateColumns: "auto 1fr" }}>
            <Watchlist />
            <div>
              <SearchBox />
              <StockList />
            </div>
          </div>
        </div>
      </div>
    </>
  )
}
