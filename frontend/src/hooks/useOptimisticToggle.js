import { useRef, useState } from 'react'

/**
 * Alternancia optimista con reversión.
 *
 * Es el mecanismo que comparten "me gusta" y "guardar": se aplica el
 * cambio al instante para que el ícono responda sin esperar, se avisa al
 * servidor y, si lo rechaza, se vuelve al valor anterior en vez de dejar
 * la pantalla mintiendo.
 *
 * Cada caso declara solo su dominio y hereda el resto.
 *
 * @param {object}   config
 * @param {object}   config.initial  Estado de partida.
 * @param {Function} config.preview  (actual) => siguiente. El cambio optimista; puro y síncrono.
 * @param {Function} config.perform  (previo) => Promise<estado>. La ida al servidor, que devuelve
 *                                    lo que el servidor dice, no lo que se suponía.
 * @param {boolean}  [config.enabled] Si es false, `toggle` no hace nada.
 * @returns {{state: object, pending: boolean, toggle: Function}}
 */
export function useOptimisticToggle({ initial, preview, perform, enabled = true }) {
  const [state, setState] = useState(initial)
  const [pending, setPending] = useState(false)

  // `pending` solo se entera al reiniciar, así que alcanza tarde para
  // frenar un segundo toggle. El ref se entera ya, y por eso evita el
  // doble clic: antes mandaba dos pedidos y el ícono terminaba al revés
  // respecto de lo que el servidor tenía.
  const inFlight = useRef(false)

  async function toggle() {
    if (inFlight.current || !enabled) return
    inFlight.current = true

    const previous = state
    setState(preview(previous))
    setPending(true)

    try {
      setState(await perform(previous))
    } catch {
      setState(previous)
    } finally {
      inFlight.current = false
      setPending(false)
    }
  }

  return { state, pending, toggle }
}