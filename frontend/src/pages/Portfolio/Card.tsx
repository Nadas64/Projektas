import { ReactNode } from "react"

type CardProps = {
  children: ReactNode
}

export default function Card({ children } : CardProps) {
  return (
    <>
      <div className="portfolio-card-container">
        {children}
      </div>
    </>
  )
}