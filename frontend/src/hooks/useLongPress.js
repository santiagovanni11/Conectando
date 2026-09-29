import { useCallback, useEffect, useRef, useState } from 'react'

/** Píxeles que el dedo o el mouse puede moverse sin que deje de ser "presionar". */
const TOLERANCIA = 10

/**
 * Detecta una pulsación larga: apretar y sostener, sin mover.
 *
 * Va aparte del componente porque es la parte con reglas raras: hay que
 * distinguir "sostuvo" de "estuvo moviendo el dedo" y cancelar el temporizador
 * si se levanta antes de tiempo. Si esto viviera en el avatar, cada avatar con
 * menú tendría que reimplementarlo.
 *
 * Usa Pointer Events, que cubren mouse, dedo y lápiz con el mismo código.
 *
 * @param {Function} callback Se dispara cuando se completa la pulsación.
 * @param {{delayMs?: number}} options
 */
export function useLongPress(callback, { delayMs = 500 } = {}) {
  const [isPressing, setIsPressing] = useState(false)
  const timer = useRef(null)
  const origen = useRef({ x: 0, y: 0 })

  // El callback cambia en cada render; guardarlo en una ref evita que el
  // temporizador quede apuntando a una versión vieja.
  const guardada = useRef(callback)
  guardada.current = callback

  const cancelar = useCallback(() => {
    clearTimeout(timer.current)
    timer.current = null
    setIsPressing(false)
  }, [])

  const onPointerDown = useCallback(
    (evento) => {
      // Solo el botón principal: el clic derecho abre el menú del navegador.
      if (evento.button !== 0) return
      origen.current = { x: evento.clientX, y: evento.clientY }
      setIsPressing(true)
      timer.current = setTimeout(() => guardada.current?.(evento), delayMs)
    },
    [delayMs],
  )

  const onPointerMove = useCallback(
    (evento) => {
      if (!timer.current) return
      const { x, y } = origen.current
      const movido = Math.hypot(evento.clientX - x, evento.clientY - y)
      // Moverse cancela: ya no es una pulsación larga, es un scroll o arrastre.
      if (movido > TOLERANCIA) cancelar()
    },
    [cancelar],
  )

  // Desmontar con el dedo apretado dejaría el temporizador vivo.
  useEffect(() => cancelar, [cancelar])

  return {
    isPressing,
    handlers: {
      onPointerDown,
      onPointerMove,
      onPointerUp: cancelar,
      onPointerLeave: cancelar,
      onPointerCancel: cancelar,
      onContextMenu: (evento) => evento.preventDefault(),
    },
  }
}
