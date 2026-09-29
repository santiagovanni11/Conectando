import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useNavCounts } from './useNavCounts'
import { fetchNavCounts } from '../services/conversationService'

vi.mock('../services/conversationService', () => ({
  fetchNavCounts: vi.fn(),
}))

vi.mock('./useMessageHub', () => ({
  useMessageHub: vi.fn(),
}))

const counts = {
  unreadMessages: 4,
  pendingFriendRequests: 1,
  unreadNotifications: 7,
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useNavCounts', () => {
  it('carga los tres contadores', async () => {
    fetchNavCounts.mockResolvedValue(counts)

    const { result } = renderHook(() => useNavCounts())

    await waitFor(() => expect(result.current.counts.unreadMessages).toBe(4))
    expect(result.current.counts.pendingFriendRequests).toBe(1)
    expect(result.current.counts.unreadNotifications).toBe(7)
  })

  it('empieza en cero para no mostrar avisos falsos', () => {
    fetchNavCounts.mockReturnValue(new Promise(() => {}))

    const { result } = renderHook(() => useNavCounts())

    expect(result.current.counts).toEqual({
      unreadMessages: 0,
      pendingFriendRequests: 0,
      unreadNotifications: 0,
    })
  })

  it('conserva lo anterior si la carga falla', async () => {
    fetchNavCounts.mockResolvedValueOnce(counts)
    const { result } = renderHook(() => useNavCounts())
    await waitFor(() => expect(result.current.counts.unreadNotifications).toBe(7))

    fetchNavCounts.mockRejectedValue(new Error('sin red'))
    await result.current.refresh()

    expect(result.current.counts.unreadNotifications).toBe(7)
  })
})