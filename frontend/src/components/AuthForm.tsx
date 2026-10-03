import { FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authenticate } from '../api'
import './AuthForm.css'

export default function AuthForm({ mode }: { mode: 'login' | 'register' }) {
  const isLogin = mode === 'login'
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')
    setBusy(true)
    try {
      await authenticate(mode, username.trim(), password)
      navigate('/')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong')
    } finally {
      setBusy(false)
    }
  }

  return (
    <form className="auth-form" onSubmit={onSubmit}>
      <h1>{isLogin ? 'Sign in' : 'Sign up'}</h1>
      <label>
        Username
        <input value={username} onChange={e => setUsername(e.target.value)}
          autoComplete="username" required minLength={3} />
      </label>
      <label>
        Password
        <input type="password" value={password} onChange={e => setPassword(e.target.value)}
          autoComplete={isLogin ? 'current-password' : 'new-password'} required minLength={isLogin ? 1 : 6} />
      </label>
      {error && <p className="error">{error}</p>}
      <button type="submit" disabled={busy}>{isLogin ? 'Sign in' : 'Create account'}</button>
      <p className="auth-switch">
        {isLogin ? <>No account? <Link to="/register">Sign up</Link></> : <>Have an account? <Link to="/login">Sign in</Link></>}
      </p>
    </form>
  )
}
