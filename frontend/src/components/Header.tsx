import { useEffect, useState } from 'react'
import { NavLink  } from 'react-router-dom'
import { getCurrentUser, logout } from '../api'
import './Header.css'

export default function Header() {
  const [username, setUsername] = useState<string | null>(null)

  useEffect(() => {
    getCurrentUser().then(user => setUsername(user?.username ?? null)).catch(() => setUsername(null))
  }, [])

  async function onLogout() {
    await logout()
    setUsername(null)
  }

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
        ? <>
            <span className="nav-link end">{username}</span>
            <button className="nav-link logout" onClick={onLogout}>Log out</button>
          </>
        : <NavLink className="nav-link end" to='/login' >
            Login
          </NavLink>}
    </div>
  )
}
