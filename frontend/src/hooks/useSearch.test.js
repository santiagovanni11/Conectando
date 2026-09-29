import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useSearch } from './useSearch'
import { userService } from '../services/userService'
import { socialService } from '../services/socialService'

vi.mock('../services/userService', () => ({
  userService: { searchUsers: vi.fn() },
}))

vi.mock('../services/socialService', () => ({
  socialService: { sendRequest: vi.fn() },
}))

/** Escribe una consulta y deja pasar el debounce. */
async function typeQuery(result, term) {
  act(() => result.current.setQuery(term))
  await act(async () => {
    await vi.advanceTimersByTimeAsync(400)
  })
}

beforeEach(() => {
  vi.clearAllMocks()
  vi.useFakeTimers()
})

afterEach(() => {
  vi.useRealTimers()
})

describe('useSearch', () => {
  it('busca mientras se escribe, sin apretar Enter', async () => {
    userService.searchUsers.mockResolvedValue([{ id: '1', displayName: 'Ana' }])
    const { result } = renderHook(() => useSearch())

    await typeQuery(result, 'ana')

    expect(userService.searchUsers).toHaveBeenCalledWith('ana')
    expect(result.current.results).toHaveLength(1)
  })

  it('no busca con menos de 2 letras', async () => {
    const { result } = renderHook(() => useSearch())

    await typeQuery(result, 'a')

    expect(userService.searchUsers).not.toHaveBeenCalled()
  })

  // Regresión: si la API devuelve { items }, results quedaba como objeto
  // y FeedSearch reventaba con results.length.
  it('normaliza una respuesta envuelta en items', async () => {
    userService.searchUsers.mockResolvedValue({ items: [{ id: '1' }, { id: '2' }] })
    const { result } = renderHook(() => useSearch())

    await typeQuery(result, 'an')

    expect(Array.isArray(result.current.results)).toBe(true)
    expect(result.current.results).toHaveLength(2)
  })

  // Regresión: una respuesta null debe quedar como array vacío, no null.
  it('nunca deja results en null', async () => {
    userService.searchUsers.mockResolvedValue(null)
    const { result } = renderHook(() => useSearch())

    await typeQuery(result, 'an')

    expect(result.current.results).toEqual([])
  })

  it('ignora respuestas viejas si se sigue escribiendo', async () => {
    userService.searchUsers.mockImplementation((term) =>
      term === 'an'
        ? new Promise((resolve) => setTimeout(() => resolve([{ id: 'viejo' }]), 500))
        : Promise.resolve([{ id: '2' }, { id: '3' }]),
    )
    const { result } = renderHook(() => useSearch())

    await typeQuery(result, 'an')
    await typeQuery(result, 'ana')
    await act(async () => {
      await vi.advanceTimersByTimeAsync(900)
    })

    expect(result.current.results).toHaveLength(2)
  })

  it('expone el error sin romper', async () => {
    userService.searchUsers.mockRejectedValue(new Error('Fallo de red'))
    const { result } = renderHook(() => useSearch())

    await typeQuery(result, 'an')

    expect(result.current.error).toBe('Fallo de red')
    expect(result.current.results).toEqual([])
  })

  it('limpia al cerrar la búsqueda', async () => {
    userService.searchUsers.mockResolvedValue([{ id: '1' }])
    const { result } = renderHook(() => useSearch())

    await typeQuery(result, 'ana')
    act(() => result.current.clear())

    expect(result.current.query).toBe('')
    expect(result.current.results).toEqual([])
  })

  it('marca la solicitud como enviada al agregar', async () => {
    socialService.sendRequest.mockResolvedValue(undefined)
    userService.searchUsers.mockResolvedValue([
      { id: '1', displayName: 'Ana', userName: 'ana', friendship: 'none' },
    ])
    const { result } = renderHook(() => useSearch())

    await typeQuery(result, 'ana')
    await act(async () => {
      await result.current.sendRequest('1')
    })

    expect(socialService.sendRequest).toHaveBeenCalledWith('1')
    expect(result.current.results[0].friendship).toBe('sent')
  })

  it('revierte el estado si la solicitud falla', async () => {
    socialService.sendRequest.mockRejectedValue(new Error('Ya es amigo'))
    userService.searchUsers.mockResolvedValue([
      { id: '1', displayName: 'Ana', userName: 'ana', friendship: 'none' },
    ])
    const { result } = renderHook(() => useSearch())

    await typeQuery(result, 'ana')
    await act(async () => {
      await result.current.sendRequest('1')
    })

    expect(socialService.sendRequest).toHaveBeenCalledWith('1')
    expect(result.current.error).toBe('Ya es amigo')
    expect(result.current.results[0].friendship).toBe('none')
  })
})