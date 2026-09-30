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
    await user.click(container.querySelector('.modal__backdrop'))

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('reusa el modal de la casa y no uno propio sin estilos', async () => {
    // Regresión: el compositor llevaba clases `composer-modal__*` que no
    // existían en ningún archivo de estilos —solo había reglas de móvil—.
    // El panel salía como un div común en el flujo de la página: sin fondo,
    // sin overlay, sin sombra y sin centrado, en cualquier pantalla.
    const user = userEvent.setup()
    const { container } = render(<PostComposerModal onPublished={() => {}} />)

    await user.click(screen.getByRole('button', { name: /crear una nueva publicación/i }))
    const dialog = await screen.findByRole('dialog')

    expect(container.querySelector('.modal')).toBeInTheDocument()
    expect(container.querySelector('.modal__backdrop')).toBeInTheDocument()
    expect(container.querySelector('.modal__panel')).toBeInTheDocument()
    // La variante alta es la que lo hace ocupar toda la pantalla en celular.
    expect(container.querySelector('.modal__panel--tall')).toBeInTheDocument()
    expect(dialog).toBeInTheDocument()

    // Y no queda rastro de las clases muertas.
    expect(container.querySelector('[class*="composer-modal"]')).toBeNull()
  })

  it('el disparador muestra la foto y el ícono, no una caja vacía', () => {
    // Antes era un recuadro con el texto suelto y sin foto, y se leía como
    // un placeholder roto en lugar de como algo que se puede tocar.
    useAuth.mockReturnValue({
      user: { displayName: 'Agustín', userName: 'agustin', profileImageUrl: '/a.jpg' },
    })

    const { container } = render(<PostComposerModal onPublished={() => {}} />)

    expect(container.querySelector('.composer-trigger__photo')).toBeInTheDocument()
    expect(container.querySelector('.composer-trigger .avatar')).toBeInTheDocument()
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