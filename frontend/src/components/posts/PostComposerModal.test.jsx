import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import PostComposerModal from './PostComposerModal'
import { useAuth } from '../../hooks/useAuth'
import { postService } from '../../services/postService'

vi.mock('../../hooks/useAuth', () => ({ useAuth: vi.fn() }))
vi.mock('../../services/postService', () => ({
  postService: {
    createPost: vi.fn().mockResolvedValue({ id: '1' }),
    updatePost: vi.fn(),
    uploadMedia: vi.fn(),
    deleteMedia: vi.fn(),
  },
}))

function mockUser() {
  useAuth.mockReturnValue({ user: { displayName: 'Agustín', userName: 'agustin' } })
}

beforeEach(() => {
  vi.clearAllMocks()
  mockUser()
})

describe('PostComposerModal', () => {
  it('muestra el prompt con el nombre del usuario', () => {
    render(<PostComposerModal onPublished={() => {}} />)
    expect(screen.getByText(/¿Qué estás pensando, Agustín\?/)).toBeInTheDocument()
  })

  it('abre el modal al tocar el disparador', async () => {
    const user = userEvent.setup()
    render(<PostComposerModal onPublished={() => {}} />)

    await user.click(screen.getByRole('button', { name: /crear una nueva publicación/i }))

    expect(await screen.findByRole('dialog')).toBeInTheDocument()
    expect(screen.getByText('Crear publicación')).toBeInTheDocument()
  })

  // Regresión: onClose llegaba undefined y la X rompía toda la app.
  it('la X cierra el modal sin publicar', async () => {
    const user = userEvent.setup()
    render(<PostComposerModal onPublished={() => {}} />)

    await user.click(screen.getByRole('button', { name: /crear una nueva publicación/i }))
    await screen.findByRole('dialog')
    await user.click(screen.getByRole('button', { name: 'Cerrar' }))

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(postService.createPost).not.toHaveBeenCalled()
  })

  it('Escape cierra el modal', async () => {
    const user = userEvent.setup()
    render(<PostComposerModal onPublished={() => {}} />)

    await user.click(screen.getByRole('button', { name: /crear una nueva publicación/i }))
    await screen.findByRole('dialog')
    await user.keyboard('{Escape}')

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('el click en el fondo cierra el modal', async () => {
    const user = userEvent.setup()
    const { container } = render(<PostComposerModal onPublished={() => {}} />)

    await user.click(screen.getByRole('button', { name: /crear una nueva publicación/i }))
    await screen.findByRole('dialog')
    await user.click(container.querySelector('.composer-modal__backdrop'))

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('al publicar avisa y cierra', async () => {
    const user = userEvent.setup()
    const onPublished = vi.fn()
    render(<PostComposerModal onPublished={onPublished} />)

    await user.click(screen.getByRole('button', { name: /crear una nueva publicación/i }))
    await screen.findByRole('dialog')
    await user.type(screen.getByPlaceholderText('¿Qué estás pensando?'), 'hola')
    await user.click(screen.getByRole('button', { name: /publicar/i }))

    expect(onPublished).toHaveBeenCalledWith({ id: '1' })
    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument())
  })
})