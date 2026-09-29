import { useEffect, useRef, useState } from 'react'
import { onHubEvent, startHubConnection } from '../services/messagingHub'

/** Evento del hub -> nombre del callback que recibe los datos. */
const EVENTOS = {
  Connected: 'onConnected',
  MessageReceived: 'onMessage',
  JoinedConversation: 'onJoined',
  MessageSeen: 'onSeen',
  MessageUpdated: 'onUpdated',
  MessageDeleted: 'onDeleted',
  NavCountsChanged: 'onNavCounts',
}

/**
 * Escucha los eventos del hub mientras el componente esté montado.
 *
 * No se engancha a la conexión directamente: usa el registro de
 * `messagingHub`, que se encarga de volver a enganchar todo cuando la
 * conexión se rehace. Si cada componente se enganchara por su cuenta, un
 * evento registrado antes de un reconexión se quedaría escuchando a la
 * conexión muerta y dejaría de llegar sin avisar.
 *
 * No hay evento de error a propósito: el hub deja subir la excepción, y eso
 * es lo que hace que invoke() rechace y el front sepa que la operación falló
 * en vez de creer que salió bien.
 *
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
    // Un solo evento de la conexión: si se usa, el botón de "visto" y la
    // lista de conversaciones se enteran. Para eso existe startHubConnection.
    startHubConnection()
      .then((connection) => setIsConnected(connection?.state === 'Connected'))
      .catch(() => {})

    const bajas = Object.entries(EVENTOS).map(([evento, callback]) =>
      onHubEvent(evento, (data) => handlersRef.current?.[callback]?.(data)),
    )

    return () => bajas.forEach((baja) => baja())
  }, [])

  return { isConnected }
}
