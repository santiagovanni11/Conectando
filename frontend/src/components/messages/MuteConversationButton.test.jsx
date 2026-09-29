import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MuteConversationButton from './MuteConversationButton'
import { setConversationMuted } from '../../services/conversationService'

vi.mock('../../services/conversationService', () => ({
  setConversationMuted: vi.fn(),
}))

function renderButton(props = {}) {
  const onChanged = vi.fn()
  render(
    <MuteConversationButton
      conversationId="conv-1"
      isMuted={false}
      onChanged={onChanged}
      {...props}
    />,
  )
  return { onChanged }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('MuteConversationButton', () => {
  it('silencia el chat al tocarlo', async () => {
    setConversationMuted.mockResolvedValue({ isMuted: true })
    const viewer = userEvent.setup()
    const { onChanged } = renderButton()

    await viewer.click(screen.getByRole('button', { name: /silenciar/i }))

    expect(setConversationMuted).toHaveBeenCalledWith('conv-1', true)
    expect(onChanged).toHaveBeenCalledWith(true)
  })

  it('reactiva un chat silenciado', async () => {
    setConversationMuted.mockResolvedValue({ isMuted: false })
    const viewer = userEvent.setup()

    renderButton({ isMuted: true })

    await viewer.click(screen.getByRole('button', { name: /reactivar/i }))

    expect(setConversationMuted).toHaveBeenCalledWith('conv-1', false)
  })

  it('marca el estado silenciado para que se note sin color', () => {
    renderButton({ isMuted: true })

    // La barra diagonal acompaña al ícono: el color solo no alcanza.
    expect(screen.getByRole('button', { name: /reactivar/i })).toHaveAttribute('aria-pressed', 'true')
    expect(document.querySelector('.conversation-mute__slash')).toBeTruthy()
  })

  it('muestra el error si el servidor rechaza', async () => {
    setConversationMuted.mockRejectedValue(new Error('No existe esa conversación.'))
    const viewer = userEvent.setup()

    renderButton()
    await viewer.click(screen.getByRole('button', { name: /silenciar/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent('No existe esa conversación.')
  })
})