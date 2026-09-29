import { useEffect, useRef, useState } from 'react'
import { startHubConnection } from '../services/messagingHub'

/**
 * Escucha los eventos del hub mientras el componente esté montado.
 * @param {object} handlers - callbacks para cada evento del hub.
 * @returns {{ isConnected: boolean }}
 */
export function useMessageHub(handlers) {
  const [isConnected, setIsConnected] = useState(false)
  const handlersRef = useRef(handlers)

  // En un efecto aparte: escribir el ref durante el render está prohibido
  // y solo hace falta una vez por render.
  useEffect(() => {
    handlersRef.current = handlers
  })

  useEffect(() => {
    let cancelled = false
    let active = null
    const registered = []

    const register = async () => {
      const connection = await startHubConnection()
      if (!connection || cancelled) return
      active = connection

      const events = {
        Connected: () => setIsConnected(true),
        MessageReceived: (message) => handlersRef.current?.onMessage?.(message),
        JoinedConversation: (data) => handlersRef.current?.onJoined?.(data),
        MessageSeen: (data) => handlersRef.current?.onSeen?.(data),
        // Edición y borrado llegan con el mensaje ya actualizado por el
        // servidor, así que el front solo lo reemplaza en la lista.
        // No hay evento de error a propósito: el hub deja subir la
        // excepción, y eso es lo que hace que invoke() rechace y el front
        // sepa que la operación falló en vez de creer que salió bien.
        MessageUpdated: (message) => handlersRef.current?.onUpdated?.(message),
        MessageDeleted: (message) => handlersRef.current?.onDeleted?.(message),
      }

      for (const [name, handler] of Object.entries(events)) {
        connection.on(name, handler)
        registered.push([name, handler])
      }

      setIsConnected(connection.state === 'Connected')
    }

    // Si el hub no conecta, la app sigue funcionando con el REST.
    register().catch(() => {})

    return () => {
      cancelled = true
      // Sin esto los handlers se acumulan: al abrir otra conversación
      // el mensaje nuevo llegaría a listeners de pantallas viejas.
      for (const [name, handler] of registered) {
        active?.off?.(name, handler)
      }
    }
  }, [])

  return { isConnected }
}