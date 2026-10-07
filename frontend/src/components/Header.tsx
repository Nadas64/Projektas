import { useEffect, useState } from 'react'
import { NavLink  } from 'react-router-dom'
import { getCurrentUser } from '../api'
import './Header.css'

export default function Header() {
  const [username, setUsername] = useState<string | null>(null)

  useEffect(() => {
    getCurrentUser().then(user => setUsername(user?.username ?? null)).catch(() => setUsername(null))
  }, [])

  return (
    <div className='header'>
      <div className='center'>
        <NavLink className="nav-link" to='/' >
          Trade
        </NavLink>
        <NavLink className="nav-link" to='/portfolio' >
          Portfolio
        </NavLink>
        <NavLink className="nav-link" to='/leaderboard' >
          Leaderboard
        </NavLink>
      </div>

      {username
        ? <span className="nav-link end">
            {username}
          </span>
        : <NavLink className="nav-link end" to='/login' >
            Login
          </NavLink>}
    </div>
  )
}
