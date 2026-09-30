import { describe, it, expect, vi } from 'vitest'
import { readFileSync } from 'node:fs'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MessageBubble from './MessageBubble'
import { CURRENT_USER_ID, PEER_ID, message } from './messageTestFixtures'

const BUBBLE_CSS = 'src/styles/pages/message-bubble.css'
const MENU_CSS = 'src/styles/pages/message-menu.css'

/** Handler de un mensaje propio: tiene todo lo que el menú ofrece. */
const ownHandlers = { onEdit: vi.fn(), onDelete: vi.fn() }

function renderBubble(msg, isOwn, handlers = {}) {
  return render(<MessageBubble message={msg} isOwn={isOwn} {...handlers} />)
}

describe('menu del mensaje', () => {
  it('el menu de un mensaje propio ofrece editar y eliminar', () => {
    renderBubble(message('m1', CURRENT_USER_ID, 'hola'), true, ownHandlers)

    expect(screen.getByLabelText('Opciones del mensaje')).toBeInTheDocument()
  })

  it('un mensaje ajeno no ofrece editar, solo eliminar', async () => {
    const user = userEvent.setup()
    renderBubble(message('m2', PEER_ID, 'hola'), false, ownHandlers)

    await user.click(screen.getByLabelText('Opciones del mensaje'))

    expect(screen.queryByRole('menuitem', { name: 'Editar' })).toBeNull()
    expect(screen.getByRole('menuitem', { name: 'Eliminar' })).toBeInTheDocument()
  })

  it('el menu aparece para los mensajes propios y tambien para los ajenos', () => {
    const { unmount } = renderBubble(message('m1', CURRENT_USER_ID, 'hola'), true, ownHandlers)
    expect(screen.getByLabelText('Opciones del mensaje')).toBeInTheDocument()
    unmount()

    renderBubble(message('m2', PEER_ID, 'hola'), false, { onDelete: vi.fn() })
    expect(screen.getByLabelText('Opciones del mensaje')).toBeInTheDocument()
  })

  it('un mensaje eliminado no ofrece editar', () => {
    renderBubble(message('m1', PEER_ID, '', { isDeleted: true }), true, ownHandlers)

    expect(screen.queryByLabelText('Opciones del mensaje')).toBeNull()
  })

  it('el menu ofrece responder, propio y ajeno', async () => {
    // Responder es lo que más se usa, y sirve para los dos lados: es la
    // razón por la que el chat se puede seguir como una conversación.
    const user = userEvent.setup()
    const own = renderBubble(message('m1', CURRENT_USER_ID, 'hola'), true, {
      ...ownHandlers, onReply: vi.fn(),
    })

    await user.click(screen.getByLabelText('Opciones del mensaje'))
    expect(screen.getByRole('menuitem', { name: 'Responder' })).toBeInTheDocument()
    own.unmount()

    renderBubble(message('m2', PEER_ID, 'hola'), false, { onDelete: vi.fn(), onReply: vi.fn() })
    await user.click(screen.getByLabelText('Opciones del mensaje'))
    expect(screen.getByRole('menuitem', { name: 'Responder' })).toBeInTheDocument()
  })

  it('responder avisa y cierra el menu', async () => {
    const onReply = vi.fn()
    const user = userEvent.setup()
    renderBubble(message('m1', PEER_ID, 'hola'), false, { onDelete: vi.fn(), onReply })

    await user.click(screen.getByLabelText('Opciones del mensaje'))
    await user.click(screen.getByRole('menuitem', { name: 'Responder' }))

    expect(onReply).toHaveBeenCalled()
    expect(screen.queryByRole('menu')).toBeNull()
  })

  it('no ofrece editar, solo responder y eliminar', async () => {
    const user = userEvent.setup()
    renderBubble(message('m2', PEER_ID, 'hola'), false, { ...ownHandlers, onReply: vi.fn() })

    await user.click(screen.getByLabelText('Opciones del mensaje'))

    expect(screen.queryByRole('menuitem', { name: 'Editar' })).toBeNull()
    expect(screen.getByRole('menuitem', { name: 'Responder' })).toBeInTheDocument()
    expect(screen.getByRole('menuitem', { name: 'Eliminar' })).toBeInTheDocument()
  })

  it('no dibuja un boton que no hace nada', async () => {
    // Regresion: "Eliminar" salia siempre. Con el reply puede haber acciones
    // sueltas, y un boton que no hace nada es peor que uno que no esta.
    const user = userEvent.setup()
    renderBubble(message('m2', PEER_ID, 'hola'), false, { onReply: vi.fn() })

    await user.click(screen.getByLabelText('Opciones del mensaje'))

    expect(screen.getByRole('menuitem', { name: 'Responder' })).toBeInTheDocument()
    expect(screen.queryByRole('menuitem', { name: 'Eliminar' })).toBeNull()
  })

  it('el boton del menu no se oculta, para que se vea en el celular', () => {
    // Regresion: el boton tenia opacity:0 y solo aparecia con hover. En
    // pantallas tactiles eso lo dejaba invisible siempre.
    const css = readFileSync(MENU_CSS, 'utf8')
    const trigger = css.match(/\.message-menu__trigger\s*\{[^}]*\}/)?.[0] ?? ''

    expect(trigger).not.toMatch(/opacity:\s*0/)
  })

  it('mantiene la regla que solapa las dos tildes del "Visto"', () => {
    // Regresion: al reescribir los estilos se perdio el margen negativo y
    // las tildes quedaron separadas, pareciendo dos iconos sueltos.
    const css = readFileSync(BUBBLE_CSS, 'utf8')
    const overlap = css.match(
      /\.message-bubble__ticks \.icon--tick \+ \.icon--tick\s*\{[^}]*margin-left:\s*(-[\d.]+rem)/,
    )

    expect(overlap).toBeTruthy()
    expect(parseFloat(overlap[1])).toBeLessThanOrEqual(-0.3)
  })
})
