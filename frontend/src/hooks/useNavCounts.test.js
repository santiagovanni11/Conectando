import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor, act } from '@testing-library/react'
import { toCounts, useNavCounts } from './useNavCounts'
import { fetchNavCounts } from '../services/conversationService'
import { useMessageHub } from './useMessageHub'

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

  it('mantiene la forma aunque el servidor devuelva null', async () => {
    // Regresión del crash en producción. El poll de 30 segundos pedía los
    // contadores; si la respuesta venía con cuerpo no-JSON, apiRequest
    // devolvía null, el hook guardaba ese null y AppNavigation —que vive
    // arriba de todas las páginas— leía counts.unreadMessages y reventaba.
    // El boundary mostraba "Algo salió mal" sin más.
    fetchNavCounts.mockResolvedValue(null)

    const { result } = renderHook(() => useNavCounts())

    await waitFor(() => expect(result.current.counts).toBeDefined())
    expect(result.current.counts).toEqual({
      unreadMessages: 0,
      pendingFriendRequests: 0,
      unreadNotifications: 0,
    })
  })

  it('mantiene la forma si el evento del hub manda algo raro', async () => {
    // El otro camino que alimenta los mismos contadores.
    fetchNavCounts.mockReturnValue(new Promise(() => {}))
    const { result } = renderHook(() => useNavCounts())

    const { onNavCounts } = useMessageHub.mock.calls.at(-1)[0]
    await waitFor(() => expect(result.current.counts).toBeDefined())

    // El evento dispara un setState: sin envolverlo en act, React agrupa
    // el trabajo y la aserción corre contra el render anterior. El primer
    // caso pasaba sin notarlo porque el valor esperado ya era cero.
    const avisar = (dato) => act(() => onNavCounts(dato))

    avisar(null)
    await waitFor(() => expect(result.current.counts.unreadMessages).toBe(0))

    avisar({ unreadMessages: 'muchos' })
    await waitFor(() => expect(result.current.counts.unreadMessages).toBe(0))

    avisar({ unreadMessages: 2 })
    await waitFor(() => expect(result.current.counts.unreadMessages).toBe(2))
  })
})

