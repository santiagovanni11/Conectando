import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import ProfilePage from './ProfilePage'
import { useAuth } from '../hooks/useAuth'
import { userService } from '../services/userService'
import { socialService } from '../services/socialService'

vi.mock('../hooks/useAuth', () => ({ useAuth: vi.fn() }))
vi.mock('../hooks/useProfile', () => ({ useProfile: vi.fn() }))
vi.mock('../services/userService', () => ({
  userService: { getPublicCounts: vi.fn().mockResolvedValue(null) },
}))
vi.mock('../services/socialService', () => ({ socialService: { getStatus: vi.fn() } }))

const ME = { id: 'yo', displayName: 'Yo' }
const OTHER = { id: 'otro', displayName: 'Otro' }

function mockProfile(id) {
  return {
    id,
    userName: id,
    displayName: id,
    bio: '',
    profileImageUrl: null,
    isPrivate: false,
    createdAt: '2026-01-01T00:00:00Z',
    postsCount: 0,
    friendsCount: 0,
    followersCount: 0,
  }
}

function renderAt(path, element) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>{element}</Routes>
    </MemoryRouter>,
  )
}

describe('ProfilePage', () => {
  // Regresión: al clickear tu propio nombre en el feed se caía en
  // /users/{miId}, se tomaba como perfil ajeno y el backend respondía
  // "No podés realizar esta acción sobre tu propio usuario".
  it('no pide la relación social contra uno mismo', async () => {
    useAuth.mockReturnValue({ user: ME })
    socialService.getStatus.mockResolvedValue({ friendship: 'friends' })

    const { useProfile } = await import('../hooks/useProfile')
    useProfile.mockReturnValue({
      status: 'loaded',
      profile: mockProfile(ME.id),
      error: '',
      notFound: false,
      reload: vi.fn(),
    })

    renderAt('/users/yo', <Route path="/users/:userId" element={<ProfilePage />} />)

    expect(socialService.getStatus).not.toHaveBeenCalled()
    await waitFor(() => expect(screen.queryByRole('alert')).not.toBeInTheDocument())
  })

  it('no pide la relación social en el perfil propio', async () => {
    useAuth.mockReturnValue({ user: ME })
    socialService.getStatus.mockResolvedValue({ friendship: 'friends' })

    const { useProfile } = await import('../hooks/useProfile')
    useProfile.mockReturnValue({
      status: 'loaded',
      profile: mockProfile(ME.id),
      error: '',
      notFound: false,
      reload: vi.fn(),
    })

    renderAt('/profile', <Route path="/profile" element={<ProfilePage />} />)

    expect(socialService.getStatus).not.toHaveBeenCalled()
  })

  it('el perfil propio pide los conteos, no los deja en cero', async () => {
    // Regresión: al mover los conteos a un endpoint aparte, el perfil propio
    // se quedó sin pedirlos y los tres números quedaron en 0.
    useAuth.mockReturnValue({ user: ME })
    socialService.getStatus.mockResolvedValue({ friendship: 'friends' })
    const { userService } = await import('../services/userService')
    const { useProfile } = await import('../hooks/useProfile')

    useProfile.mockReturnValue({
      status: 'loaded',
      profile: mockProfile(ME.id),
      error: '',
      notFound: false,
      reload: vi.fn(),
    })
    userService.getPublicCounts.mockClear()

    renderAt('/profile', <Route path="/profile" element={<ProfilePage />} />)

    // Sin id en la URL, el hook tiene que usar el del perfil cargado.
    await waitFor(() => expect(userService.getPublicCounts).toHaveBeenCalledWith(ME.id))
  })

  it('el perfil ajeno pide los conteos con el id de la URL', async () => {
    useAuth.mockReturnValue({ user: ME })
    socialService.getStatus.mockResolvedValue({ friendship: 'none' })
    const { userService } = await import('../services/userService')
    const { useProfile } = await import('../hooks/useProfile')

    useProfile.mockReturnValue({
      status: 'loaded',
      profile: mockProfile('otro'),
      error: '',
      notFound: false,
      reload: vi.fn(),
    })
    userService.getPublicCounts.mockClear()

    renderAt('/users/otro', <Route path="/users/:userId" element={<ProfilePage />} />)

    await waitFor(() => expect(userService.getPublicCounts).toHaveBeenCalledWith('otro'))
  })

  it('pide la relación en el perfil de otra persona', async () => {
    useAuth.mockReturnValue({ user: ME })
    socialService.getStatus.mockResolvedValue({ friendship: 'none' })

    const { useProfile } = await import('../hooks/useProfile')
    useProfile.mockReturnValue({
      status: 'loaded',
      profile: mockProfile(OTHER.id),
      error: '',
      notFound: false,
      reload: vi.fn(),
    })

    renderAt('/users/otro', <Route path="/users/:userId" element={<ProfilePage />} />)

    await waitFor(() => expect(socialService.getStatus).toHaveBeenCalledWith(OTHER.id))
  })
})