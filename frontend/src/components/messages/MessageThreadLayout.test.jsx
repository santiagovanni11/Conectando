import { describe, it, expect, vi } from 'vitest'
import { readFileSync } from 'node:fs'
import { render, screen } from '@testing-library/react'
import { renderWithRouter } from '../../test-utils/renderWithRouter'
import userEvent from '@testing-library/user-event'
import MessageThread from './MessageThread'
import { CURRENT_USER_ID, message } from './messageTestFixtures'

const currentUserId = CURRENT_USER_ID

/**
 * Tests de estructura del hilo.
 *
 * Van aparte porque jsdom no calcula CSS: acá se verifica el marcado y las
 * reglas que lo sostienen, que es lo que se rompió cuando se metió un
 * envoltorio entre la fila y la burbuja. Los tests de comportamiento
 * están en MessageThread.test.jsx.
 */
describe('MessageThread: estructura de la fila', () => {
  it('la burbuja cuelga directo de la fila, sin envoltorio intermedio', () => {
    // Se agregó un <div> entre el <li> y la burbuja para colgar de ahí el
    // gesto de responder, y rompió el layout en tres lugares: el
    // `max-width: 75%` de la burbuja pasó a resolverse contra un contenedor
    // angosto y se encogió; el `row`/`row-reverse` del <li>, que distingue
    // propios de ajenos, dejó de mandar; y el `overflow: hidden` recortó el
    // desplegable del menú.
    const { container } = renderWithRouter(
      <MessageThread
        messages={[message('m1', 'user-2', 'hola')]}
        currentUserId={currentUserId}
        isLoading={false}
      />,
    )

    const item = container.querySelector('ul.message-thread > li')
    const bubble = item.querySelector('.message-bubble')

    expect(bubble).toBeTruthy()
    // Lo importante: la burbuja cuelga DIRECTO de la fila, y la fila no
    // tiene nada más adentro.
    expect(bubble.parentElement).toBe(item)
    expect(item.children).toHaveLength(1)
  })

  it('el gesto no deja flecha ni ningún otro indicador en la fila', () => {
    // La flecha que aparecía al deslizar se sacó porque quedó pesada en el
    // hilo. La señal del gesto es la burbuja corriendo, y nada más.
    const { container } = renderWithRouter(
      <MessageThread
        messages={[message('m1', 'user-2', 'hola')]}
        currentUserId={currentUserId}
        isLoading={false}
      />,
    )

    const item = container.querySelector('ul.message-thread > li')

    expect(container.querySelector('.message-row__hint')).toBeNull()
    expect(item.textContent).not.toMatch(/[↩↪→←⇢]/)
  })

  it('no quedan estilos de la flecha que se haya sacado', () => {
    // Si el marcado se fue pero el CSS quedo, la regla sigue ahi esperando
    // a que alguien la vuelva a usar, y entonces reaparece sola.
    const css = readFileSync('src/styles/pages/message-reply.css', 'utf8')

    expect(css).not.toMatch(/message-row__hint/)
  })

  it('cada mensaje conserva su clase de propio o ajeno', () => {
    // El wrapper tenía `justify-content: flex-end` sin condiciones y anulaba
    // la inversión de la fila: todo quedaba del mismo lado.
    const { container } = renderWithRouter(
      <MessageThread
        messages={[
          message('m1', currentUserId, 'mío'),
          message('m2', 'user-2', 'suyo'),
        ]}
        currentUserId={currentUserId}
        isLoading={false}
      />,
    )

    expect(container.querySelectorAll('.is-own')).toHaveLength(1)
    expect(container.querySelectorAll('.is-theirs')).toHaveLength(1)
  })

  it('el gesto se aplica a la fila y la fila no recorta su contenido', () => {
    // `overflow: hidden` hacía desaparecer el desplegable del menú de tres
    // puntitos, que cuelga de la misma fila.
    const css = readFileSync('src/styles/pages/message-reply.css', 'utf8')
    const item = css.match(/\.message-thread__item\s*\{[^}]*\}/)?.[0] ?? ''

    expect(item).toMatch(/transform:\s*translateX\(/)
    expect(item).not.toMatch(/overflow:\s*hidden/)
  })

  it('el scroll vertical lo maneja el navegador, no el gesto', () => {
    // Sin `pan-y`, el hilo deja de poder desplazarse con el dedo.
    const css = readFileSync('src/styles/pages/message-reply.css', 'utf8')
    const item = css.match(/\.message-thread__item\s*\{[^}]*\}/)?.[0] ?? ''

    expect(item).toMatch(/touch-action:\s*pan-y/)
  })

  it('un mensaje se responde desde el menú de tres puntitos', async () => {
    const onReply = vi.fn()
    const user = userEvent.setup()

    renderWithRouter(
      <MessageThread
        messages={[message('m1', 'user-2', 'hola')]}
        currentUserId={currentUserId}
        isLoading={false}
        onReply={onReply}
      />,
    )

    await user.click(screen.getByLabelText('Opciones del mensaje'))
    await user.click(screen.getByRole('menuitem', { name: 'Responder' }))

    // El menú llama a su callback sin argumentos: si el mensaje no llegara,
    // la página no sabría a cuál responder y la barra no se levantaría.
    expect(onReply).toHaveBeenCalledWith(expect.objectContaining({ id: 'm1' }))
  })
})
