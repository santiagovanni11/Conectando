import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BlockedProfilesSection from './BlockedProfilesSection'
import { socialService } from '../../services/socialService'

vi.mock('../../services/socialService', () => ({
  socialService: { getBlocks: vi.fn(), unblock: vi.fn() },
}))

const blocks = [
  {
    user: { id: 'u2', displayName: 'Ana', userName: 'ana', profileImageUrl: null },
    blockedAt: '2026-01-01T10:00:00Z',
  },
  {
    user: { id: 'u3', displayName: 'Beto', userName: 'beto', profileImageUrl: null },
    blockedAt: '2026-01-02T10:00:00Z',
  },
]

beforeEach(() => {
  vi.clearAllMocks()
  socialService.getBlocks.mockResolvedValue(blocks)
})

describe('BlockedProfilesSection', () => {
  it('lista a quienes bloqueaste', async () => {
    render(<BlockedProfilesSection />)

    expect(await screen.findByText('Ana')).toBeInTheDocument()
    expect(screen.getByText('Beto')).toBeInTheDocument()
  })

  it('ofrece desbloquear a cada uno', async () => {
    const viewer = userEvent.setup()
    socialService.unblock.mockResolvedValue(undefined)

    render(<BlockedProfilesSection />)
    await screen.findByText('Ana')
    await viewer.click(screen.getAllByRole('button', { name: 'Desbloquear' })[0])

    expect(socialService.unblock).toHaveBeenCalledWith('u2')
  })

  it('saca la fila al desbloquear, sin recargar la página', async () => {
    const viewer = userEvent.setup()
    socialService.unblock.mockResolvedValue(undefined)

    render(<BlockedProfilesSection />)
    await screen.findByText('Ana')
    await viewer.click(screen.getAllByRole('button', { name: 'Desbloquear' })[0])

    expect(await screen.findByText('Beto')).toBeInTheDocument()
    expect(screen.queryByText('Ana')).toBeNull()
  })

  it('avisa cuando no hay nadie bloqueado', async () => {
    socialService.getBlocks.mockResolvedValue([])

    render(<BlockedProfilesSection />)

    expect(await screen.findByText(/no bloqueaste a nadie/i)).toBeInTheDocument()
  })

  it('muestra el error si no se puede cargar la lista', async () => {
    socialService.getBlocks.mockRejectedValue(new Error('No se pudo cargar.'))

    render(<BlockedProfilesSection />)

    expect(await screen.findByRole('alert')).toHaveTextContent('No se pudo cargar.')
  })
})