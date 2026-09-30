import { describe, it, expect, vi } from 'vitest'
import { renderHook, act } from '@testing-library/react'
import { useSwipeToReply } from './useSwipeToReply'

/** Monta el hook con un onReply espiable. */
function setup(pointerType = 'touch') {
  const onReply = vi.fn()
  const view = renderHook(() => useSwipeToReply(onReply))
  return { onReply, view, pointerType }
}

/**
 * Dispara el gesto entero. Los handlers se releen en cada paso, igual que
 * hace el navegador: tras el movimiento el hook vuelve a renderizar y
 * `onPointerUp` es una función nueva que ya ve el desplazamiento.
 */
function drag(harness, from, to, axis = 'x') {
  const { view, pointerType } = harness
  const fire = (name, point) => act(() => view.result.current.swipeProps[name](point))

  fire('onPointerDown', { clientX: from.x, clientY: from.y, pointerType })
  fire('onPointerMove', { clientX: to.x, clientY: to.y, pointerType })
  fire(
    'onPointerUp',
    axis === 'x'
      ? { clientX: to.x, clientY: to.y, pointerType }
      : { clientX: from.x, clientY: to.y, pointerType },
  )
}

describe('useSwipeToReply', () => {
  it('responde al llegar al umbral', () => {
    const harness = setup()

    drag(harness, { x: 10, y: 100 }, { x: 90, y: 100 })

    expect(harness.onReply).toHaveBeenCalledTimes(1)
  })

  it('no responde si el gesto se queda corto', () => {
    const harness = setup()

    drag(harness, { x: 10, y: 100 }, { x: 35, y: 100 })

    expect(harness.onReply).not.toHaveBeenCalled()
  })

  it('ignora el arrastre a la izquierda', () => {
    // Hacia la izquierda no hay gesto, como en WhatsApp.
    const harness = setup()

    drag(harness, { x: 200, y: 100 }, { x: 10, y: 100 })

    expect(harness.onReply).not.toHaveBeenCalled()
  })

  it('no responde al scrollear en vertical', () => {
    // El caso que arruina la experiencia si se ignora: al subir el hilo con
    // el dedo se respondería el mensaje que pasa por debajo.
    const harness = setup()

    drag(harness, { x: 100, y: 300 }, { x: 102, y: 180 })

    expect(harness.onReply).not.toHaveBeenCalled()
    expect(harness.view.result.current.shift).toBe(0)
  })

  it('no hace nada con el mouse', () => {
    // En mouse arrastrar es para seleccionar texto; el reply va por el menú.
    const harness = setup('mouse')

    drag(harness, { x: 10, y: 100 }, { x: 200, y: 100 })

    expect(harness.onReply).not.toHaveBeenCalled()
  })

  it('vuelve a cero después de soltar', () => {
    const harness = setup()
    const fire = (name, point) =>
      act(() => harness.view.result.current.swipeProps[name](point))
    const finger = { clientX: 90, clientY: 100, pointerType: 'touch' }

    fire('onPointerDown', { clientX: 10, clientY: 100, pointerType: 'touch' })
    fire('onPointerMove', finger)
    expect(harness.view.result.current.shift).toBeGreaterThan(0)

    fire('onPointerUp', finger)

    expect(harness.view.result.current.shift).toBe(0)
  })

  it('no responde a un mensaje borrado porque la fila va deshabilitada', () => {
    const onReply = vi.fn()
    const view = renderHook(() => useSwipeToReply(onReply, { enabled: false }))
    const { swipeProps } = view.result.current

    act(() => swipeProps.onPointerDown({ clientX: 10, clientY: 100, pointerType: 'touch' }))
    act(() => swipeProps.onPointerMove({ clientX: 200, clientY: 100, pointerType: 'touch' }))
    act(() => swipeProps.onPointerUp({ clientX: 200, clientY: 100, pointerType: 'touch' }))

    expect(onReply).not.toHaveBeenCalled()
  })
})
