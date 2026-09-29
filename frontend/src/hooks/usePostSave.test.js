import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { usePostSave } from './usePostSave'
import { postService } from '../services/postService'

vi.mock('../services/postService', () => ({
  postService: { toggleSave: vi.fn() },
}))

const post = (overrides = {}) => ({ id: 'p1', savedByMe: false, ...overrides })

beforeEach(() => {
  vi.clearAllMocks()
})

describe('usePostSave', () => {
  it('arranca con lo que vino del post', () => {
    const { result } = renderHook(() => usePostSave(post({ savedByMe: true })))

    expect(result.current).toMatchObject({ saved: true })
  })

  it('guarda y espera lo que confirme el servidor', async () => {
    postService.toggleSave.mockResolvedValue({ savedByMe: true })

    const { result } = renderHook(() => usePostSave(post()))
    await act(async () => {
      await result.current.toggle()
    })

    expect(postService.toggleSave).toHaveBeenCalledWith('p1')
    expect(result.current).toMatchObject({ saved: true })
  })

  it('desguarda si ya estaba guardado', async () => {
    postService.toggleSave.mockResolvedValue({ savedByMe: false })

    const { result } = renderHook(() => usePostSave(post({ savedByMe: true })))
    await act(async () => {
      await result.current.toggle()
    })

    expect(result.current).toMatchObject({ saved: false })
  })

  it('vuelve atrás si el servidor rechaza', async () => {
    postService.toggleSave.mockRejectedValue(new Error('no'))

    const { result } = renderHook(() => usePostSave(post({ savedByMe: true })))
    await act(async () => {
      await result.current.toggle()
    })

    expect(result.current).toMatchObject({ saved: true })
  })

  it('no hace nada si todavía no hay post', async () => {
    const { result } = renderHook(() => usePostSave(null))
    await act(async () => {
      await result.current.toggle()
    })

    expect(postService.toggleSave).not.toHaveBeenCalled()
    expect(result.current).toMatchObject({ saved: false })
  })
})
