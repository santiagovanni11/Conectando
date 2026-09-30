import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import MessageReplyQuote from './MessageReplyQuote'
import { DELETED_PLACEHOLDER } from '../../constants/messages'

/** Arma la cita tal como la manda el servidor. */
function quote(name, preview, extra = {}) {
  return { id: 'm1', senderDisplayName: name, preview, isDeleted: false, ...extra }
}

describe('MessageReplyQuote', () => {
  it('no dibuja nada cuando el mensaje no cita a otro', () => {
    const { container } = render(<MessageReplyQuote replyTo={null} />)

    expect(container).toBeEmptyDOMElement()
  })

  it('muestra el nombre y el recorte del original', () => {
    render(<MessageReplyQuote replyTo={quote('Ana', 'hola qué tal')} />)

    expect(screen.getByText('Ana')).toBeInTheDocument()
    expect(screen.getByText('hola qué tal')).toBeInTheDocument()
  })

  it('avisa cuando el mensaje citado fue borrado', () => {
    render(<MessageReplyQuote replyTo={quote('Ana', '', { isDeleted: true })} />)

    expect(screen.getByText(DELETED_PLACEHOLDER)).toBeInTheDocument()
  })

  it('no muestra el texto de un original borrado aunque venga en el dato', () => {
    // El servidor ya lo manda vacío, pero si por lo que sea llegara, la cita
    // no puede exhibirlo.
    render(<MessageReplyQuote replyTo={quote('Ana', 'secreto', { isDeleted: true })} />)

    expect(screen.queryByText('secreto')).not.toBeInTheDocument()
  })
})
