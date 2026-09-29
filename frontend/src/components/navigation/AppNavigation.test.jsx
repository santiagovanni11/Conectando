import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import AppNavigation from './AppNavigation'

vi.mock('../../hooks/useAuth', () => ({
  useAuth: () => ({ user: { id: 'u1', displayName: 'Ana' }, logout: vi.fn() }),
}))
vi.mock('../../hooks/useNavCounts', () => ({
  useNavCounts: () => ({
    counts: { unreadMessages: 0, pendingFriendRequests: 0, unreadNotifications: 0 },
  }),
}))

function renderNav() {
  return render(
    <MemoryRouter>
      <AppNavigation />
    </MemoryRouter>,
  )
}

describe('AppNavigation', () => {
  it('incluye el acceso a Ayuda en el menú', () => {
    // AppNavigation decide entre barra superior y barra inferior con
    // matchMedia, que jsdom no implementa.
    window.matchMedia = () => ({ matches: true, addEventListener() {}, removeEventListener() {} })

    renderNav()

    expect(screen.getByRole('link', { name: 'Ayuda' })).toHaveAttribute('href', '/help')
  })
})