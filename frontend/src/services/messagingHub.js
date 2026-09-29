import { HUB_URL } from '../constants/api'
import { TOKEN_STORAGE_KEY } from '../constants/auth'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

/**
 * Conexión al hub de mensajes. Se comparte entre toda la app para no abrir
 * un WebSocket por cada componente.
 *
 * Además es la dueña del registro de eventos. Antes cada componente se
 * enganchaba por su cuenta a la conexión que encontraba en el momento de
 * montarse, y eso se rompía de dos maneras: si todavía no había token se
 * quedaba sin enganchar para el resto de la sesión, y si la conexión se
 * rehacía —al iniciar sesión, o cuando el servidor la corta— los handlers
 * seguían atados a la conexión vieja. Los dos casos se ven igual desde afuera:
 * el número del chat nunca se actualiza, y como el poll de respaldo lo
 * corrige cada 30 segundos, parece que solo anda lento.
 */
let connection = null
let startingConnection = null

/** evento -> Set de funciones. Sobrevive a que la conexión se rehaga. */
const listeners = new Map()
const reconnectedHandlers = new Set()

function getToken() {
  return localStorage.getItem(TOKEN_STORAGE_KEY)
}

/**
 * Escucha un evento del hub, ahora y en cada reconexión futura.
 * @returns {Function} Para dejar de escuchar.
 */
export function onHubEvent(event, handler) {
  if (!listeners.has(event)) listeners.set(event, new Set())
  listeners.get(event).add(handler)

  // Si ya hay conexión, se engancha ahora. Si no, se engancha sola cuando
  // alguien la abra.
  connection?.on(event, handler)

  return () => {
    listeners.get(event)?.delete(handler)
    connection?.off(event, handler)
  }
}

/**
 * Avisa cuando la conexión se rehizo sola, para que cada chat vuelva a entrar
 * a su grupo.
 *
 * La pertenencia a un grupo vive dentro de la conexión, no en el usuario. Si
 * SignalR reconecta, del otro lado hay una conexión nueva, sin ningún grupo:
 * el chat sigue abierto en pantalla pero ya no le llega nada, y el único modo
 * de recuperarlo es recargar. En desarrollo casi no se nota porque el servidor
 * no se cae; en la nube, que apaga el servicio a los 15 minutos, pasa siempre.
 */
export function onReconnected(handler) {
  reconnectedHandlers.add(handler)
  return () => reconnectedHandlers.delete(handler)
}

/** Engancha todos los eventos registrados a una conexión. */
function attachAll(target) {
  for (const [event, handlers] of listeners) {
    for (const handler of handlers) {
      target.on(event, handler)
    }
  }
}

export function getHubConnection() {
  if (connection) return connection
  if (!getToken()) return null

  connection = buildConnection()
  attachAll(connection)

  connection.onreconnecting(() => {
    // SignalR reconecta solo; esto solo informa el estado.
  })

  connection.onreconnected(() => {
    // Del otro lado hay una conexión nueva: hay que volver a enganchar los
    // eventos y a devolver el chat a su grupo.
    attachAll(connection)

    for (const handler of reconnectedHandlers) {
      handler()
    }
  })

  return connection
}

export async function startHubConnection() {
  const existing = getHubConnection()
  if (!existing) return null
  if (existing.state === 'Connected') return existing

  // Varias pantallas pueden pedir la conexión a la vez: se comparte la
  // misma promesa para no abrir dos WebSockets.
  if (!startingConnection) {
    startingConnection = existing.start().finally(() => {
      startingConnection = null
    })
  }

  return startingConnection
}

export async function stopHubConnection() {
  if (!connection) return

  const current = connection
  connection = null
  startingConnection = null

  try {
    await current.stop()
  } catch {
    // Si ya estaba caído no hay nada que limpiar.
  }
}

/**
 * Cierra la conexión porque cambió el usuario o el token.
 *
 * Es indispensable: el WebSocket queda autenticado con el token con el que
 * se abrió. Si no se cierra al cambiar de sesión, la conexión sigue siendo
 * de la cuenta anterior y los mensajes que se envían desde la nueva quedan
 * guardados con el identificador del usuario anterior.
 *
 * Los eventos registrados no se borran: los componentes que siguen montados
 * los necesitan apenas se abra la conexión nueva.
 */
export async function resetHubConnection() {
  await stopHubConnection()
}

function buildConnection() {
  // El token va en el query string porque el navegador no puede mandar
  // headers en un WebSocket. El backend lo acepta solo en /hubs.
  return new HubConnectionBuilder()
    .withUrl(`${HUB_URL}/hubs/messages`, { accessTokenFactory: getToken })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(LogLevel.Warning)
    .build()
}
