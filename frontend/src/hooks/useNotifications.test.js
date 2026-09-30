import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useNotifications } from './useNotifications'
import { notificationService } from '../services/notificationService'

vi.mock('../services/notificationService', () => ({
  notificationService: {
    getAll: vi.fn(),
    markAsRead: vi.fn(),
    markAllAsRead: vi.fn(),
  },
}))

const aviso = (i) => ({
  id: 'n' + i,
  content: 'aviso ' + i,
  createdAt: '2026-01-01T10:00:00Z',
  isRead: false,
})

const pagina = (items) => ({ items, nextCursor: null, hasMore: false, unreadCount: 3 })

beforeEach(() => {
  vi.clearAllMocks()
  notificationService.getAll.mockResolvedValue(pagina([aviso(1), aviso(2)]))
  notificationService.markAllAsRead.mockResolvedValue(undefined)
})

describe('useNotifications', () => {
  it('monta sin reventar', async () => {
    // Regresión del crash en producción. El efecto que marca todo como
    // leído estaba declarado ANTES de la const que nombraba en su array de
    // dependencias. El array se evalúa en la línea del useEffect, así que
    // la const seguía en zona muerta: ReferenceError al primer render, la
    // pantalla entera caída y un mensaje minificado que no decía nada
    // ("Cannot access 'y' before initialization").
    const { result } = renderHook(() => useNotifications())

    await waitFor(() => expect(result.current.loading).toBe(false))
    expect(result.current.items).toHaveLength(2)
  })

  it('marca todo como leído al abrir la pantalla', async () => {
    // El motivo de existir del efecto: si la pantalla está abierta, los
    // avisos ya se leyeron, y el número de arriba tiene que bajar solo.
    const { result } = renderHook(() => useNotifications())

    await waitFor(() => expect(notificationService.markAllAsRead).toHaveBeenCalled())
    await waitFor(() => expect(result.current.unreadCount).toBe(0))
  })

  it('no marca nada si no hay nada sin leer', async () => {
    notificationService.getAll.mockResolvedValue({
      ...pagina([aviso(1)]),
      unreadCount: 0,
    })

    const { result } = renderHook(() => useNotifications())
    await waitFor(() => expect(result.current.loading).toBe(false))

    expect(notificationService.markAllAsRead).not.toHaveBeenCalled()
  })

  it('reintenta cuando falla el listado', async () => {
    notificationService.getAll.mockRejectedValueOnce(new Error('sin red'))

    const { result } = renderHook(() => useNotifications())
    await waitFor(() => expect(result.current.error).toBe('sin red'))
  })
})
