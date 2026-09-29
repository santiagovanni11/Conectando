import { describe, it, expect, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import PostComposerForm from './PostComposerForm'
import { postService } from '../../services/postService'

vi.mock('../../services/postService', () => ({
  postService: {
    createPost: vi.fn(),
    updatePost: vi.fn(),
    uploadMedia: vi.fn(),
    deleteMedia: vi.fn(),
  },
}))

function renderForm() {
  return render(
    <PostComposerForm avatarName="Agustín" avatarSrc={null} onCancel={() => {}} onPublished={() => {}} />,
  )
}

describe('PostComposerForm', () => {
  it('monta sin romperse y muestra el nombre del autor', () => {
    // Regresión: antes se usaba `content` antes de declararlo (TDZ) y reventaba.
    renderForm()
    expect(screen.getByText('Agustín')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('¿Qué estás pensando?')).toBeInTheDocument()
  })

  it('Publicar arranca deshabilitado y se habilita con texto', async () => {
    const user = userEvent.setup()
    renderForm()

    const publish = screen.getByRole('button', { name: /publicar/i })
    expect(publish).toBeDisabled()

    await user.type(screen.getByPlaceholderText('¿Qué estás pensando?'), 'hola')
    expect(publish).toBeEnabled()
  })

  it('Publicar sigue deshabilitado si solo hay espacios', async () => {
    const user = userEvent.setup()
    renderForm()

    await user.type(screen.getByPlaceholderText('¿Qué estás pensando?'), '   ')
    expect(screen.getByRole('button', { name: /publicar/i })).toBeDisabled()
  })

  it('Publica el texto y notifica', async () => {
    const user = userEvent.setup()
    postService.createPost.mockResolvedValue({ id: 'nuevo' })
    const onPublished = vi.fn()
    render(
      <PostComposerForm avatarName="Ana" avatarSrc={null} onCancel={() => {}} onPublished={onPublished} />,
    )

    await user.type(screen.getByPlaceholderText('¿Qué estás pensando?'), 'hola')
    await user.click(screen.getByRole('button', { name: /publicar/i }))

    expect(postService.createPost).toHaveBeenCalledWith(
      expect.objectContaining({ content: 'hola', privacy: 'Public', mediaIds: [] }),
    )
    expect(onPublished).toHaveBeenCalledWith({ id: 'nuevo' })
  })

  it('muestra el error sin cerrar el formulario', async () => {
    const user = userEvent.setup()
    postService.createPost.mockRejectedValue(new Error('No se pudo publicar'))
    renderForm()

    await user.type(screen.getByPlaceholderText('¿Qué estás pensando?'), 'hola')
    await user.click(screen.getByRole('button', { name: /publicar/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent('No se pudo publicar')
  })

  it('permite cambiar la privacidad', async () => {
    const user = userEvent.setup()
    postService.createPost.mockResolvedValue({ id: 'nuevo' })
    renderForm()

    await user.click(screen.getByRole('button', { name: /público/i }))
    await user.click(screen.getByRole('option', { name: /solo yo/i }))

    await user.type(screen.getByPlaceholderText('¿Qué estás pensando?'), 'hola')
    await user.click(screen.getByRole('button', { name: /publicar/i }))

    expect(postService.createPost).toHaveBeenCalledWith(
      expect.objectContaining({ privacy: 'Private' }),
    )
  })

  it('permite elegir fotos desde cámara o fototeca', async () => {
    const user = userEvent.setup()
    postService.uploadMedia.mockResolvedValue([{ id: 'm1', url: 'http://x/1.jpg' }])
    renderForm()

    await user.click(screen.getByRole('button', { name: /agregar fotos/i }))
    await user.click(await screen.findByRole('button', { name: /elegir de la fototeca/i }))

    const input = screen.getByTestId('gallery-input')
    await user.upload(input, new File(['x'], 'a.jpg', { type: 'image/jpeg' }))

    await waitFor(() => expect(postService.uploadMedia).toHaveBeenCalled())
  })

  it('limita a 8 fotos', async () => {
    const user = userEvent.setup()
    postService.uploadMedia.mockResolvedValue(
      Array.from({ length: 8 }, (_, i) => ({ id: `m${i}`, url: `http://x/${i}.jpg` })),
    )
    renderForm()

    const input = screen.getByTestId('gallery-input')
    await user.upload(
      input,
      Array.from({ length: 8 }, () => new File(['x'], 'a.jpg', { type: 'image/jpeg' })),
    )

    await waitFor(() => expect(screen.getByRole('button', { name: /máximo 8 fotos/i })).toBeDisabled())
  })
})