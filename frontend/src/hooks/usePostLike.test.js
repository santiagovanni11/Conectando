import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { usePostLike } from './usePostLike'
import { postService } from '../services/postService'

vi.mock('../services/postService', () => ({
  postService: { like: vi.fn(), unlike: vi.fn() },
}))

const post = (overrides = {}) => ({
  id: 'p1',
  likedByMe: false,
  likesCount: 3,
  ...overrides,
})

/** Promesa que se resuelve a mano, para poder mirar el estado intermedio. */
function deferred() {
  let resolve
  const promise = new Promise((res) => {
    resolve = res
  })
  return { promise, resolve }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('usePostLike', () => {
  it('arranca con lo que vino del post', () => {
    const { result } = renderHook(() =>
      usePostLike(post({ likedByMe: true, likesCount: 5 })),
    )

    expect(result.current).toMatchObject({ liked: true, count: 5 })
  })

  it('da like y suma uno cuando todavía no estaba marcado', async () => {
    postService.like.mockResolvedValue({ likedByMe: true, likesCount: 4 })

    const { result } = renderHook(() => usePostLike(post()))
    await act(async () => {
      await result.current.toggle()
    })

    expect(postService.like).toHaveBeenCalledWith('p1')
    expect(postService.unlike).not.toHaveBeenCalled()
    expect(result.current).toMatchObject({ liked: true, count: 4 })
  })

  it('quita el like y resta uno cuando ya estaba marcado', async () => {
    postService.unlike.mockResolvedValue({ likedByMe: false, likesCount: 2 })

    const { result } = renderHook(() => usePostLike(post({ likedByMe: true })))
    await act(async () => {
      await result.current.toggle()
    })

    expect(postService.unlike).toHaveBeenCalledWith('p1')
    expect(postService.like).not.toHaveBeenCalled()
    expect(result.current).toMatchObject({ liked: false, count: 2 })
  })

  it('vuelve atrás si el servidor rechaza', async () => {
    postService.like.mockRejectedValue(new Error('no'))

    const { result } = renderHook(() => usePostLike(post()))
    await act(async () => {
      await result.current.toggle()
    })

    expect(result.current).toMatchObject({ liked: false, count: 3 })
  })

  it('sube el contador al instante, antes de que conteste el servidor', async () => {
    // Si el contador no se mueve solo, el like se siente lento aunque el
    // ícono ya se haya encendido.
    const gate = deferred()
    postService.like.mockReturnValue(gate.promise)

    const { result } = renderHook(() => usePostLike(post()))
    act(() => {
      result.current.toggle()
    })

    expect(result.current).toMatchObject({ liked: true, count: 4 })

    await act(async () => {
      gate.resolve({ likedByMe: true, likesCount: 4 })
    })
  })

  it('baja el contador al instante al quitar el like', async () => {
    const gate = deferred()
    postService.unlike.mockReturnValue(gate.promise)

    const { result } = renderHook(() =>
      usePostLike(post({ likedByMe: true, likesCount: 3 })),
    )
    act(() => {
      result.current.toggle()
    })

    expect(result.current).toMatchObject({ liked: false, count: 2 })

    await act(async () => {
      gate.resolve({ likedByMe: false, likesCount: 2 })
    })
  })

  it('no hace nada si todavía no hay post', async () => {
    const { result } = renderHook(() => usePostLike(null))
    await act(async () => {
      await result.current.toggle()
    })

    expect(postService.like).not.toHaveBeenCalled()
    expect(result.current).toMatchObject({ liked: false, count: 0 })
  })
})
