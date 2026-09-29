import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act, waitFor } from '@testing-library/react'
import { useSavedPosts } from './useSavedPosts'
import { postService } from '../services/postService'

vi.mock('../services/postService', () => ({
  postService: { listSaved: vi.fn() },
}))

const page = (overrides = {}) => ({
  items: [{ id: 'p1', content: 'guardada' }],
  nextCursor: null,
  hasMore: false,
  ...overrides,
})

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useSavedPosts', () => {
  it('carga las publicaciones guardadas', async () => {
    postService.listSaved.mockResolvedValue(page())

    const { result } = renderHook(() => useSavedPosts())
    await waitFor(() => expect(result.current.phase).toBe('loaded'))

    expect(result.current.items).toHaveLength(1)
    expect(postService.listSaved).toHaveBeenCalledWith({ limit: 10 })
  })

  it('carga la página siguiente con el cursor', async () => {
    postService.listSaved
      .mockResolvedValueOnce(page({ hasMore: true, nextCursor: 'c1' }))
      .mockResolvedValueOnce(page({ items: [{ id: 'p2' }] }))

    const { result } = renderHook(() => useSavedPosts())
    await waitFor(() => expect(result.current.hasMore).toBe(true))

    await act(async () => {
      await result.current.loadMore()
    })

    expect(postService.listSaved).toHaveBeenLastCalledWith({ limit: 10, cursor: 'c1' })
    expect(result.current.items).toHaveLength(2)
  })

  it('expone el error si la carga falla', async () => {
    postService.listSaved.mockRejectedValue(new Error('No se pudieron cargar.'))

    const { result } = renderHook(() => useSavedPosts())
    await waitFor(() => expect(result.current.phase).toBe('error'))

    expect(result.current.error).toBe('No se pudieron cargar.')
  })
})