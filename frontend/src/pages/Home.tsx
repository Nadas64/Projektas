import StockList from "./StockList";
import Header from "../components/Header";
import SearchBox from "../components/SearchBox";

export default function Home() {
  return (
    <>
      <Header />
      <div className="page-content">
        <SearchBox />
        <StockList />
      </div>
    </>
  )
}
