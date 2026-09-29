import { describe, it, expect, vi, beforeEach } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useOptimisticToggle } from './useOptimisticToggle'

/** Promesa que se resuelve a mano, para poder mirar el estado intermedio. */
function deferred() {
  let resolve
  const promise = new Promise((res) => {
    resolve = res
  })
  return { promise, resolve }
}

describe('useOptimisticToggle', () => {
  let preview
  let perform

  const setup = (overrides = {}) =>
    renderHook(() =>
      useOptimisticToggle({
        initial: { on: false, n: 0 },
        preview,
        perform,
        ...overrides,
      }),
    )

  beforeEach(() => {
    preview = vi.fn((s) => ({ on: !s.on, n: s.on ? s.n - 1 : s.n + 1 }))
    perform = vi.fn()
  })

  it('arranca en el estado inicial, sin pendientes', () => {
    const { result } = setup()

    expect(result.current.state).toEqual({ on: false, n: 0 })
    expect(result.current.pending).toBe(false)
  })

  it('muestra el cambio optimista antes de que el servidor conteste', async () => {
    const gate = deferred()
    perform.mockReturnValue(gate.promise)

    const { result } = setup()
    act(() => {
      result.current.toggle()
    })

    expect(result.current.state).toEqual({ on: true, n: 1 })

    await act(async () => {
      gate.resolve({ on: true, n: 1 })
    })
  })

  it('manda a perform el estado previo, no el ya cambiado', async () => {
    perform.mockResolvedValue({ on: true, n: 1 })

    const { result } = setup()
    await act(async () => {
      await result.current.toggle()
    })

    expect(perform).toHaveBeenCalledWith({ on: false, n: 0 })
  })

  it('se queda con lo que dice el servidor, no con lo que suponía', async () => {
    // El servidor corrige un contador distinto del optimista.
    perform.mockResolvedValue({ on: true, n: 99 })

    const { result } = setup()
    await act(async () => {
      await result.current.toggle()
    })

    expect(result.current.state).toEqual({ on: true, n: 99 })
  })

  it('vuelve atrás si el servidor rechaza', async () => {
    perform.mockRejectedValue(new Error('no'))

    const { result } = setup()
    await act(async () => {
      await result.current.toggle()
    })

    expect(result.current.state).toEqual({ on: false, n: 0 })
  })

  it('marca pending mientras dura la operación y lo limpia al terminar', async () => {
    const gate = deferred()
    perform.mockReturnValue(gate.promise)

    const { result } = setup()
    act(() => {
      result.current.toggle()
    })
    expect(result.current.pending).toBe(true)

    await act(async () => {
      gate.resolve({ on: true, n: 1 })
    })
    expect(result.current.pending).toBe(false)
  })

  it('limpia pending también cuando la operación falla', async () => {
    perform.mockRejectedValue(new Error('no'))

    const { result } = setup()
    await act(async () => {
      await result.current.toggle()
    })

    expect(result.current.pending).toBe(false)
  })

  it('ignora el toggle si está deshabilitado', async () => {
    const { result } = setup({ enabled: false })
    await act(async () => {
      await result.current.toggle()
    })

    expect(perform).not.toHaveBeenCalled()
    expect(result.current.state).toEqual({ on: false, n: 0 })
  })

  it('descarta el segundo toggle mientras el primero sigue en vuelo', async () => {
    // El doble clic dispara dos toggle antes de que React repinte. Sin
    // descartarlo, el ícono queda al revés de lo que tiene el servidor.
    const gate = deferred()
    perform.mockReturnValue(gate.promise)

    const { result } = setup()
    act(() => {
      result.current.toggle()
    })
    act(() => {
      result.current.toggle()
    })

    expect(perform).toHaveBeenCalledTimes(1)

    await act(async () => {
      gate.resolve({ on: true, n: 1 })
    })
    expect(result.current.state).toEqual({ on: true, n: 1 })
  })
})
