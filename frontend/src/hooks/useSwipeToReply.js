import { useCallback, useRef, useState } from 'react'

/** Cuánto se corre la burbuja como máximo, y desde cuánto ya cuenta. */
const MAX_SHIFT = 72
const CONFIRM_AT = 44

/** Desplazamiento que decide si el gesto va a ser horizontal o vertical. */
const AXIS_LOCK = 8

/**
 * Gesto de deslizar a la derecha para responder, como en WhatsApp.
 *
 * El detalle que hace que funcione es decidir el eje una sola vez, con los
 * primeros 8 píxeles: si el dedo arrancó hacia arriba o hacia abajo, el
 * usuario está scrolleando el hilo y la burbuja no se tiene que mover. Sin
 * ese corte, cualquier intento de subir la pantalla respondería al mensaje
 * que estuviera en el camino.
 *
 * Solo se activa con dedo y lápiz. En mouse la respuesta va por el menú de
 * los tres puntitos, y ahí arrastrar viene bien para seleccionar texto.
 *
 * @param {Function} onReply - Se llama al soltar, si el gesto llegó al umbral.
 * @param {{ enabled?: boolean }} options
 * @returns {{ shift: number, swipeProps: object }}
 */
export function useSwipeToReply(onReply, { enabled = true } = {}) {
  const [shift, setShift] = useState(0)
  const start = useRef(null)

  const onPointerDown = useCallback(
    (event) => {
      if (!enabled || event.pointerType === 'mouse') return
      start.current = { x: event.clientX, y: event.clientY, axis: null }
    },
    [enabled],
  )

  const onPointerMove = useCallback((event) => {
    const from = start.current
    if (!from) return

    const dx = event.clientX - from.x
    const dy = event.clientY - from.y

    if (from.axis === null && Math.max(Math.abs(dx), Math.abs(dy)) > AXIS_LOCK) {
      from.axis = Math.abs(dx) > Math.abs(dy) ? 'x' : 'y'
    }

    // Eje ya decidido en vertical: el hilo se scrollea y la fila no se toca.
    if (from.axis !== 'x') return

    // Solo hacia la derecha: es el sentido de "responder". Hacia la izquierda
    // no hay gesto, como en WhatsApp.
    setShift(Math.max(0, Math.min(dx, MAX_SHIFT)))
  }, [])

  const finish = useCallback(() => {
    const wasHorizontal = start.current?.axis === 'x'
    const didConfirm = shift >= CONFIRM_AT

    start.current = null
    setShift(0)

    if (wasHorizontal && didConfirm) onReply()
  }, [shift, onReply])

  return {
    shift,
    swipeProps: {
      onPointerDown,
      onPointerMove,
      onPointerUp: finish,
      onPointerCancel: finish,
    },
  }
}
