import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MessageComposer from './MessageComposer'

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

    expect(onSend).toHaveBeenCalledWith('hola')
    expect(input).toHaveValue('')
  })

  it('envía al presionar Enter', async () => {
    const onSend = vi.fn()
    const user = userEvent.setup()

    render(<MessageComposer onSend={onSend} />)

    await user.type(screen.getByLabelText('Mensaje'), 'hola{Enter}')

    expect(onSend).toHaveBeenCalledWith('hola')
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