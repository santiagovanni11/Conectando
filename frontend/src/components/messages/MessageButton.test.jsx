import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import MessageButton from './MessageButton'
import NavSpy from './NavSpy'
import { startDirectConversation } from '../../services/conversationService'
import { ROUTES } from '../../constants/routes'

vi.mock('../../services/conversationService', () => ({
  startDirectConversation: vi.fn(),
}))

beforeEach(() => {
  vi.clearAllMocks()
})

describe('MessageButton', () => {
  it('inicia la conversación y navega al hilo', async () => {
    const user = userEvent.setup()
    startDirectConversation.mockResolvedValue({ id: 'conv-9' })

    render(
      <MemoryRouter>
        <MessageButton userId="user-2" />
        <NavSpy />
      </MemoryRouter>,
    )

    await user.click(screen.getByRole('button', { name: /Enviar mensaje/ }))

    expect(startDirectConversation).toHaveBeenCalledWith('user-2')
    expect(await screen.findByTestId('location')).toHaveTextContent(
      ROUTES.conversation('conv-9'),
    )
  })

  it('muestra el error si no se puede abrir el chat', async () => {
    const user = userEvent.setup()
    startDirectConversation.mockRejectedValue(new Error('No podés escribirle'))

    render(
      <MemoryRouter>
        <MessageButton userId="user-2" />
      </MemoryRouter>,
    )

    await user.click(screen.getByRole('button', { name: /Enviar mensaje/ }))

    expect(await screen.findByRole('alert')).toHaveTextContent('No podés escribirle')
  })
})