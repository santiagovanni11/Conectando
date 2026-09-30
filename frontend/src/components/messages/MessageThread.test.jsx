import { describe, it, expect, vi, beforeEach } from 'vitest'
import { readFileSync } from 'node:fs'
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

  it('la burbuja cuelga directo de la fila, sin envoltorio intermedio', () => {
    // Regresión: se metió un <div class="message-row"> entre el <li> y la
    // burbuja. Eso le cambiaba el bloque contenedor, y con él el
    // `max-width: 75%` de la burbuja: se encogía hasta desaparecer. El
    // flex `row`/`row-reverse` que distingue propios de ajenos, además,
    // dejaba de mandar y todos se alineaban al mismo lado.
    const { container } = render(
      <MessageThread
        messages={[message('m1', 'user-2', 'hola')]}
        currentUserId={currentUserId}
        isLoading={false}
      />,
    )

    const item = container.querySelector('ul.message-thread > li')
    const bubble = item.querySelector('.message-bubble')

    expect(bubble).toBeTruthy()
    // Lo importante: la burbuja cuelga DIRECTO de la fila. El único otro
    // hijo es la flecha del gesto, que está en absolute y no participa del
    // flex.
    expect(bubble.parentElement).toBe(item)
    expect(item.children).toHaveLength(2)
    expect(item.lastElementChild).toBe(bubble)
  })

  it('el gesto se aplica a la fila y no recorta el menú', () => {
    // Regresión: la fila llevaba `overflow: hidden` para que la burbuja no
    // saliera al deslizarse, y eso recortaba el desplegable del menú de
    // tres puntitos, que cuelga de la misma fila.
    const css = readFileSync('src/styles/pages/message-reply.css', 'utf8')
    const item = css.match(/\.message-thread__item\s*\{[^}]*\}/)?.[0] ?? ''

    expect(item).toMatch(/transform:\s*translateX\(/)
    expect(item).not.toMatch(/overflow:\s*hidden/)
  })

  it('un mensaje se responde desde el menú de tres puntitos', async () => {
    const onReply = vi.fn()
    const user = userEvent.setup()

    render(
      <MessageThread
        messages={[message('m1', 'user-2', 'hola')]}
        currentUserId={currentUserId}
        isLoading={false}
        onReply={onReply}
      />,
    )

    await user.click(screen.getByLabelText('Opciones del mensaje'))
    await user.click(screen.getByRole('menuitem', { name: 'Responder' }))

    expect(onReply).toHaveBeenCalledWith(expect.objectContaining({ id: 'm1' }))
  })
})