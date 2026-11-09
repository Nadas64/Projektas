import { useEffect, useState } from 'react'
import { getCompanyNews, type NewsItemData } from '../api'
import './CompanyNews.css'

export default function CompanyNews({ symbol }: { symbol: string }) {
  const [news, setNews] = useState<NewsItemData[] | null>(null)

  useEffect(() => {
    getCompanyNews(symbol)
      .then(setNews)
      .catch(() => setNews([]))
  }, [symbol])

  return (
    <section className="news">
      <h2>News</h2>
      {news === null && <p>loading...</p>}
      {news?.length === 0 && <p>No recent news.</p>}
      {news?.map((n) => (
        <article key={n.url}>
          <a href={n.url} target="_blank" rel="noopener noreferrer">{n.headline}</a>
          <small>{n.source} · {new Date(n.datetime * 1000).toLocaleString()}</small>
          <p>{n.summary}</p>
        </article>
      ))}
    </section>
  )
}