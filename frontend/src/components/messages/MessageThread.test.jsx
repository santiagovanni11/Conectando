import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MessageThread from './MessageThread'
import { CURRENT_USER_ID, message } from './messageTestFixtures'

const currentUserId = CURRENT_USER_ID

beforeEach(() => {
  vi.clearAllMocks()
})

describe('MessageThread', () => {
  it('muestra el aviso cuando no hay mensajes', () => {
    render(<MessageThread messages={[]} currentUserId={currentUserId} isLoading={false} />)

    expect(screen.getByText(/Todavía no hay mensajes/)).toBeInTheDocument()
  })

  it('muestra el estado de carga', () => {
    render(<MessageThread messages={[]} currentUserId={currentUserId} isLoading />)

    expect(screen.getByText('Cargando mensajes…')).toBeInTheDocument()
  })

  it('distingue propios y ajenos por su clase', () => {
    const { container } = render(
      <MessageThread
        messages={[message('m1', currentUserId, 'hola'), message('m2', 'user-2', 'qué tal')]}
        currentUserId={currentUserId}
        isLoading={false}
      />,
    )

    expect(container.querySelectorAll('.is-own')).toHaveLength(1)
    expect(container.querySelectorAll('.is-theirs')).toHaveLength(1)
  })

  it('permite cargar mensajes anteriores si hay más', async () => {
    const onLoadMore = vi.fn()
    const user = userEvent.setup()

    render(
      <MessageThread
        messages={[message('m1', 'user-2', 'hola')]}
        currentUserId={currentUserId}
        isLoading={false}
        hasMore
        onLoadMore={onLoadMore}
      />,
    )

    await user.click(screen.getByRole('button', { name: 'Cargar mensajes anteriores' }))

    expect(onLoadMore).toHaveBeenCalled()
  })

  it('cada mensaje es un <li> propio, sin anidar', () => {
    // Regresión: la burbuja era un <li> dentro de otro <li>. Eso no es HTML
    // válido y rompía el flex, dejando todos los mensajes en una fila.
    const { container } = render(
      <MessageThread
        messages={[
          message('m1', 'user-2', 'primero'),
          message('m2', currentUserId, 'segundo'),
        ]}
        currentUserId={currentUserId}
        isLoading={false}
      />,
    )

    const items = container.querySelectorAll('ul.message-thread > li')
    expect(items).toHaveLength(2)
    // Ningún <li> puede contener otro <li>.
    expect(container.querySelectorAll('li li')).toHaveLength(0)
  })
})