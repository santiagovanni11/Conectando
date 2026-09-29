import { HUB_URL } from '../constants/api'
import { TOKEN_STORAGE_KEY } from '../constants/auth'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

/**
 * Conexión al hub de mensajes. Se comparte entre toda la app para no abrir
 * un WebSocket por cada componente.
 */
let connection = null
let startingConnection = null
const reconnectedHandlers = new Set()

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

function getToken() {
  return localStorage.getItem(TOKEN_STORAGE_KEY)
}

export function getHubConnection() {
  if (connection) return connection
  if (!getToken()) return null

  connection = buildConnection()

  connection.onreconnecting(() => {
    // SignalR reconecta solo; esto solo informa el estado.
  })

  connection.onreconnected(() => {
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