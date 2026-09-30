import { render } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'

/**
 * Renderiza con un router de memoria.
 *
 * <para>
 * Existe porque casi todos los componentes terminan enlazando a un perfil, y
 * un <Link> fuera de un router revienta con un error de contexto que dice
 * más o menos "basename es null". El mensaje no señala el router y hace perder
 * tiempo: por eso el wrapper está en un lugar y los tests lo usan siempre.
 *
 * <para>
 * El router es de memoria porque un test no navega de verdad: solo necesita
 * que el contexto exista para que los enlaces se puedan renderizar y consultar.
/// </para>
 */
export function renderWithRouter(ui, { route = '/', ...options } = {}) {
  return render(ui, {
    wrapper: ({ children }) => <MemoryRouter initialEntries={[route]}>{children}</MemoryRouter>,
    ...options,
  })
}
