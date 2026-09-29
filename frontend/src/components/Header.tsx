import { NavLink  } from 'react-router-dom'
import './Header.css'

export default function Header() {
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
      
      <NavLink className="nav-link end" to='/login' >
        Login
      </NavLink>
    </div> 
  )
}