import { useLocation } from 'react-router-dom'

/** Expone la ruta actual para poder verificarla. */
export default function NavSpy() {
  const location = useLocation()
  return <span data-testid="location">{location.pathname}</span>
}