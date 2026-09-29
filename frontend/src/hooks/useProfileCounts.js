import { useEffect, useState } from 'react'
import { userService } from '../services/userService'

/**
 * Conteos de un perfil, ya descontados los bloqueados.
 *
 * Se piden aparte del perfil porque el mismo perfil muestra números
 * distintos según quién lo mire. Si la petición falla se devuelve `null`:
 * la pantalla cae a los del perfil en vez de quedar en cero.
 */
export function useProfileCounts(userId, enabled) {
  const [counts, setCounts] = useState(null)
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    if (!userId || !enabled) return undefined

    let active = true

    // El perfil propio también pide conteos: no tiene bloqueos que restar,
    // pero los números tienen que venir de algún lado. La ruta acepta el
    // propio id igual que cualquier otro, así que alcanza con usar la misma.
    userService
      .getPublicCounts(userId)
      .then((result) => {
        if (active) setCounts(result)
      })
      .catch(() => {
        if (active) setCounts(null)
      })

    return () => {
      active = false
    }
  }, [userId, enabled, attempt])

  // El `null` se decide en el render y no en un efecto: si el hook queda
  // deshabilitado, el valor viejo se limpia sin un setState en el efecto.
  const active = Boolean(userId && enabled)

  return {
    counts: active ? counts : null,
    refresh: () => setAttempt((c) => c + 1),
  }
}