import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act, waitFor } from '@testing-library/react'
import { useConversations } from './useConversations'
import { fetchConversations } from '../services/conversationService'

vi.mock('../services/conversationService', () => ({
  fetchConversations: vi.fn(),
}))

// El hub se aísla: acá se prueba la lista, no el WebSocket.
const hubHandlers = { current: {} }
vi.mock('./useMessageHub', () => ({
  useMessageHub: (handlers) => {
    hubHandlers.current = handlers
  },
}))

function conversation(overrides = {}) {
  return {
    id: 'conv-1',
    updatedAt: '2026-01-01T10:00:00Z',
    unreadCount: 0,
    peers: [{ id: 'user-2', displayName: 'Ana', userName: 'ana' }],
    lastMessage: null,
    ...overrides,
  }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useConversations', () => {
  it('carga la lista de conversaciones', async () => {
    fetchConversations.mockResolvedValue([conversation()])

    const { result } = renderHook(() => useConversations())

    await waitFor(() => expect(result.current.isLoading).toBe(false))

    expect(result.current.conversations).toHaveLength(1)
    expect(result.current.error).toBeNull()
  })

  it('expone el error si la carga falla', async () => {
    fetchConversations.mockRejectedValue(new Error('sin red'))

    const { result } = renderHook(() => useConversations())

    await waitFor(() => expect(result.current.error).toBeTruthy())
    expect(result.current.conversations).toEqual([])
  })

  it('pone en cero los no leídos de una conversación', async () => {
    fetchConversations.mockResolvedValue([conversation({ unreadCount: 3 })])

    const { result } = renderHook(() => useConversations())
    await waitFor(() => expect(result.current.conversations).toHaveLength(1))

    act(() => result.current.markAsRead('conv-1'))

    expect(result.current.conversations[0].unreadCount).toBe(0)
  })

  it('marca el último mensaje como visto sin abrir el chat', async () => {
    fetchConversations.mockResolvedValue([
      conversation({
        lastMessage: {
          id: 'm1',
          createdAt: '2026-01-01T10:00:00Z',
          isSeenByPeer: false,
          sender: { id: 'user-1', displayName: 'Yo' },
        },
      }),
    ])

    const { result } = renderHook(() => useConversations())
    await waitFor(() => expect(result.current.conversations).toHaveLength(1))

    act(() =>
      hubHandlers.current.onSeen({
        conversationId: 'conv-1',
        readerId: 'user-2',
        readAt: '2026-01-01T10:05:00Z',
      }),
    )

    expect(result.current.conversations[0].lastMessage.isSeenByPeer).toBe(true)
  })

  it('no marca como visto si el otro leyó antes de enviarse el mensaje', async () => {
    fetchConversations.mockResolvedValue([
      conversation({
        lastMessage: {
          id: 'm1',
          createdAt: '2026-01-01T10:10:00Z',
          isSeenByPeer: false,
          sender: { id: 'user-1', displayName: 'Yo' },
        },
      }),
    ])

    const { result } = renderHook(() => useConversations())
    await waitFor(() => expect(result.current.conversations).toHaveLength(1))

    act(() =>
      hubHandlers.current.onSeen({
        conversationId: 'conv-1',
        readerId: 'user-2',
        readAt: '2026-01-01T10:05:00Z',
      }),
    )

    expect(result.current.conversations[0].lastMessage.isSeenByPeer).toBe(false)
  })
})