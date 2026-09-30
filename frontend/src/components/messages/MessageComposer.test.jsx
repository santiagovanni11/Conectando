import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MessageComposer from './MessageComposer'
import { PEER_ID, message } from './messageTestFixtures'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('MessageComposer', () => {
  it('envía el mensaje y limpia el campo', async () => {
    const onSend = vi.fn()
    const user = userEvent.setup()

    render(<MessageComposer onSend={onSend} />)

    const input = screen.getByLabelText('Mensaje')
    await user.type(input, 'hola')
    await user.click(screen.getByRole('button', { name: 'Enviar' }))

    // El segundo argumento es el id citado. Sin respuesta va null explícito,
    // no undefined: el servicio lo manda en el body y undefined se perdería
    // al serializar.
    expect(onSend).toHaveBeenCalledWith('hola', null)
    expect(input).toHaveValue('')
  })

  it('envía al presionar Enter', async () => {
    const onSend = vi.fn()
    const user = userEvent.setup()

    render(<MessageComposer onSend={onSend} />)

    await user.type(screen.getByLabelText('Mensaje'), 'hola{Enter}')

    expect(onSend).toHaveBeenCalledWith('hola', null)
  })

  it('envía citando el mensaje que se está respondiendo', async () => {
    const onSend = vi.fn()
    const user = userEvent.setup()

    render(<MessageComposer onSend={onSend} replyTo={message('m1', PEER_ID, 'hola')} />)

    await user.type(screen.getByLabelText('Mensaje'), 'buenos días{Enter}')

    expect(onSend).toHaveBeenCalledWith('buenos días', 'm1')
  })

  it('muestra la barra de respuesta y la puede cancelar', async () => {
    const onCancelReply = vi.fn()
    const user = userEvent.setup()

    render(
      <MessageComposer
        onSend={vi.fn()}
        onCancelReply={onCancelReply}
        replyTo={message('m1', PEER_ID, 'hola')}
      />,
    )

    expect(screen.getByText(/Respondiendo a Ana/)).toBeInTheDocument()
    expect(screen.getByLabelText('Mensaje')).toHaveAttribute(
      'placeholder',
      'Escribí tu respuesta',
    )

    await user.click(screen.getByRole('button', { name: 'Cancelar la respuesta' }))

    expect(onCancelReply).toHaveBeenCalled()
  })

  it('baja la barra de respuesta al enviar', async () => {
    const onCancelReply = vi.fn()
    const user = userEvent.setup()

    render(
      <MessageComposer
        onSend={vi.fn()}
        onCancelReply={onCancelReply}
        replyTo={message('m1', PEER_ID, 'hola')}
      />,
    )

    await user.type(screen.getByLabelText('Mensaje'), 'gracias{Enter}')

    expect(onCancelReply).toHaveBeenCalled()
  })

  it('no envía si el mensaje está vacío', async () => {
    const onSend = vi.fn()
    const user = userEvent.setup()

    render(<MessageComposer onSend={onSend} />)

    // El botón arranca deshabilitado mientras no haya texto.
    expect(screen.getByRole('button', { name: 'Enviar' })).toBeDisabled()
    await user.type(screen.getByLabelText('Mensaje'), '   ')

    expect(onSend).not.toHaveBeenCalled()
  })

  it('muestra el error de envío', () => {
    render(<MessageComposer onSend={vi.fn()} error="No se pudo enviar" />)

    expect(screen.getByText('No se pudo enviar')).toBeInTheDocument()
  })
})