import { useLayoutEffect, useRef } from 'react'

const DEFAULT_MAX_HEIGHT = 120

/**
 * Ajusta la altura de un textarea a su contenido, con techo fijo
 * para no destruir el layout. El mínimo lo maneja CSS (min-height).
 * Al superar el techo aparece scroll interno.
 */
export function useAutoResize(value, maxHeight = DEFAULT_MAX_HEIGHT) {
  const ref = useRef(null)

  useLayoutEffect(() => {
    const node = ref.current
    if (!node) return
    node.style.height = 'auto'
    const target = Math.min(node.scrollHeight, maxHeight)
    node.style.height = `${target}px`
    node.style.overflowY = node.scrollHeight > maxHeight ? 'auto' : 'hidden'
  }, [value, maxHeight])

  return ref
}
