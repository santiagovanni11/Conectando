import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import ConversationListItem from './ConversationListItem'
import { DELETED_PLACEHOLDER } from '../../constants/messages'

const currentUserId = 'user-1'

function conversation(lastMessage, peers) {
  return {
    id: 'conv-1',
    updatedAt: '2026-01-01T10:00:00Z',
    unreadCount: 0,
    peers: peers ?? [{ id: 'user-2', displayName: 'Ana', userName: 'ana' }],
    lastMessage,
  }
}

function renderItem(conv) {
  return render(
    <MemoryRouter>
      <ul>
        <ConversationListItem conversation={conv} currentUserId={currentUserId} />
      </ul>
    </MemoryRouter>,
  )
}

const lastMessage = (overrides) => ({
  id: 'm1',
  content: 'hola',
  createdAt: '2026-01-01T10:00:00Z',
  isSeenByPeer: false,
  isDeleted: false,
  sender: { id: 'user-2', displayName: 'Ana' },
  ...overrides,
})

describe('ConversationListItem: previsualización', () => {
  it('muestra el aviso de borrado sin entrar al chat', () => {
    // Regresión: el backend manda el texto vacío cuando se borra, así que
    // la fila quedaba en blanco y no se entendía que había algo ahí.
    renderItem(conversation(lastMessage({ content: '', isDeleted: true })))

    expect(screen.getByText(new RegExp(DELETED_PLACEHOLDER))).toBeInTheDocument()
  })

  it('no muestra el aviso si el mensaje existe', () => {
    renderItem(conversation(lastMessage({ content: 'hola qué tal' })))

    expect(screen.queryByText(new RegExp(DELETED_PLACEHOLDER))).toBeNull()
    expect(screen.getByText(/hola qué tal/)).toBeInTheDocument()
  })

  it('avisa cuando la conversación está vacía', () => {
    renderItem(conversation(null))

    expect(screen.getByText(/Sin mensajes todavía/)).toBeInTheDocument()
  })

  it('el aviso de borrado no lleva el tick de visto', () => {
    const { container } = renderItem(
      conversation(lastMessage({ content: '', isDeleted: true, isSeenByPeer: true })),
    )

    // El tick va en otro <span>; el aviso debe ser solo texto.
    expect(container.querySelector('.conversation-item__preview')).toHaveTextContent(
      DELETED_PLACEHOLDER,
    )
  })
})

describe('ConversationListItem: foto de perfil', () => {
  it('muestra la foto del otro cuando tiene una', () => {
    // El backend ya mandaba profileImageUrl y la fila lo ignoraba: salía
    // una letra. Ahora usa el mismo Avatar que el resto de la app.
    const { container } = renderItem(
      conversation(
        lastMessage(),
        [{ id: 'user-2', displayName: 'Ana', userName: 'ana', profileImageUrl: '/ana.jpg' }],
      ),
    )

    expect(container.querySelector('img')).toHaveAttribute('src', '/ana.jpg')
  })

  it('cae a las iniciales cuando el otro no tiene foto', () => {
    const { container } = renderItem(conversation(lastMessage()))

    expect(container.querySelector('img')).toBeNull()
    expect(container.querySelector('.avatar__initials')).toHaveTextContent('A')
  })

  it('el nombre sigue junto a la hora, no junto a la foto', () => {
    // La lista tiene foto + nombre + hora en una línea. Si el nombre se
    // pegara a la foto se rompería ese orden, que es el de WhatsApp.
    const { container } = renderItem(conversation(lastMessage()))

    const header = container.querySelector('.conversation-item__header')
    expect(header).toHaveTextContent('Ana')
    expect(container.querySelector('.conversation-identity')).toBeNull()
  })
})