import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import StartConversationPanel from './StartConversationPanel'
import { startDirectConversation } from '../../services/conversationService'

vi.mock('../../services/conversationService', () => ({
  startDirectConversation: vi.fn(() => Promise.resolve({ id: 'conv-1' })),
}))

const friends = [
  { user: { id: 'u1', displayName: 'Ana López', userName: 'ana', profileImageUrl: null } },
  { user: { id: 'u2', displayName: 'Bruno Díaz', userName: 'bruno', profileImageUrl: null } },
]

function renderPanel() {
  return render(
    <MemoryRouter>
      <StartConversationPanel friends={friends} status="loaded" />
    </MemoryRouter>,
  )
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('StartConversationPanel', () => {
  it('lista los amigos con su botón de mensaje', () => {
    renderPanel()

    expect(screen.getByText('Ana López')).toBeInTheDocument()
    expect(screen.getByText('Bruno Díaz')).toBeInTheDocument()
    expect(screen.getAllByRole('button', { name: /Enviar mensaje/ })).toHaveLength(2)
  })

  it('filtra por nombre al escribir', async () => {
    const user = userEvent.setup()
    renderPanel()

    await user.type(screen.getByPlaceholderText('Escribí un nombre'), 'bruno')

    expect(screen.queryByText('Ana López')).toBeNull()
    expect(screen.getByText('Bruno Díaz')).toBeInTheDocument()
  })

  it('avisa cuando nadie coincide', async () => {
    const user = userEvent.setup()
    renderPanel()

    await user.type(screen.getByPlaceholderText('Escribí un nombre'), 'zzzz')

    expect(screen.getByText(/Ningún amigo coincide/)).toBeInTheDocument()
  })

  it('inicia el chat con el amigo elegido', async () => {
    const user = userEvent.setup()
    renderPanel()

    const buttons = screen.getAllByRole('button', { name: /Enviar mensaje/ })
    await user.click(buttons[1])

    expect(startDirectConversation).toHaveBeenCalledWith('u2')
  })

  it('invita a hacer amigos si no tenés ninguno', () => {
    render(
      <MemoryRouter>
        <StartConversationPanel friends={[]} status="loaded" />
      </MemoryRouter>,
    )

    expect(screen.getByText('Todavía no tenés amigos')).toBeInTheDocument()
  })
})