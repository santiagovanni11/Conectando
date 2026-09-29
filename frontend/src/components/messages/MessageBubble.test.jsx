import { describe, it, expect } from 'vitest'
import { readFileSync } from 'node:fs'
import { render, screen } from '@testing-library/react'
import MessageBubble from './MessageBubble'
import { DELETED_PLACEHOLDER } from '../../constants/messages'
import { CURRENT_USER_ID, PEER_ID, message } from './messageTestFixtures'

/** La burbuja es un <div>: el <li> lo aporta la fila del hilo. */
function renderBubble(msg, isOwn, handlers = {}) {
  return render(<MessageBubble message={msg} isOwn={isOwn} {...handlers} />)
}

describe('MessageBubble', () => {
  it('marca los mensajes propios como propios', () => {
    const { container } = renderBubble(message('m1', CURRENT_USER_ID, 'hola'), true)

    expect(container.querySelector('.is-own')).toBeTruthy()
  })

  it('no muestra el nombre del autor en los propios', () => {
    renderBubble(message('m1', CURRENT_USER_ID, 'hola'), true)

    expect(screen.queryByText('Yo')).toBeNull()
  })

  it('muestra el nombre del autor en los ajenos', () => {
    renderBubble(message('m2', PEER_ID, 'hola'), false)

    expect(screen.getByText('Ana')).toBeInTheDocument()
  })

  it('el nombre del autor es el del remitente real, no el del usuario', () => {
    // Regresion: se confundia el remitente con la sesion activa.
    renderBubble(message('m2', PEER_ID, 'hola'), false)

    expect(screen.getByText('Ana')).toBeInTheDocument()
  })

  it('muestra "Enviado" en un mensaje propio que nadie leyo', () => {
    renderBubble(message('m1', CURRENT_USER_ID, 'hola'), true)

    expect(screen.getByTestId('seen-state')).toHaveTextContent('Enviado')
  })

  it('muestra "Visto" con doble tilde cuando el otro ya lo leyo', () => {
    renderBubble(message('m1', CURRENT_USER_ID, 'hola', { isSeenByPeer: true }), true)

    const badge = screen.getByTestId('seen-state')
    expect(badge).toHaveTextContent('Visto')
    expect(badge).toHaveClass('is-seen')
    expect(badge.querySelectorAll('.message-bubble__ticks svg')).toHaveLength(2)
  })

  it('no muestra el visto en los mensajes del otro', () => {
    renderBubble(message('m2', PEER_ID, 'hola'), false)

    expect(screen.queryByTestId('seen-state')).toBeNull()
  })

  it('muestra "Editado" en un mensaje que se corrigio', () => {
    renderBubble(message('m1', PEER_ID, 'hola', { isEdited: true }), false)

    expect(screen.getByText('Editado')).toBeInTheDocument()
  })

  it('no muestra "Editado" en un mensaje normal', () => {
    renderBubble(message('m1', PEER_ID, 'hola'), false)

    expect(screen.queryByText('Editado')).toBeNull()
  })

  it('sustituye el texto por el aviso cuando el mensaje fue eliminado', () => {
    renderBubble(message('m1', PEER_ID, '', { isDeleted: true }), false)

    expect(screen.getByText(DELETED_PLACEHOLDER)).toBeInTheDocument()
    expect(screen.queryByTestId('seen-state')).toBeNull()
  })

  it('el aviso de borrado es el mismo que usa la lista de conversaciones', () => {
    // Regresion: el texto estaba duplicado en dos archivos. Si uno se
    // corrigiera, el chat y la lista mostrarian mensajes distintos.
    const constants = readFileSync('src/constants/messages.js', 'utf8')
    const bubble = readFileSync('src/components/messages/MessageBubble.jsx', 'utf8')

    // El literal solo puede existir en el modulo compartido.
    expect(constants).toMatch(/DELETED_PLACEHOLDER = 'Este mensaje fue eliminado'/)
    expect(bubble).toMatch(/import \{ DELETED_PLACEHOLDER \}/)
    expect(bubble).not.toMatch(/= 'Este mensaje fue eliminado'/)
  })
})
