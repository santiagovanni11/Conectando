import { Link } from 'react-router-dom'
import Logo from '../components/ui/Logo'
import { ROUTES } from '../constants/routes'

export default function AuthLayout({ title, subtitle, visual = null, children }) {
  return (
    <div className="auth-layout">
      <main className={`auth-card ${visual ? 'auth-card--split' : ''}`}>
        {visual && <div className="auth-card__visual">{visual}</div>}
        <div className="auth-card__panel">
          <Link to={ROUTES.home} aria-label="Conectando — inicio">
            <Logo size="lg" />
          </Link>
          <div className="auth-card__heading">
            <h1>{title}</h1>
            {subtitle && <p>{subtitle}</p>}
          </div>
          {children}
        </div>
      </main>
      <footer className="auth-footer">
        <Link to={ROUTES.login}>Iniciar sesión</Link>
        <span aria-hidden="true"> · </span>
        <Link to={ROUTES.register}>Crear cuenta</Link>
        <span aria-hidden="true"> · </span>
        {/* También acá: si el problema es justamente no poder entrar,
            el contacto tiene que estar a mano. */}
        <Link to={ROUTES.help}>Ayuda</Link>
      </footer>
    </div>
  )
}