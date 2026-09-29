import { describe, it, expect, vi, beforeEach } from 'vitest'

// jsdom no resuelve rutas relativas, así que se evita construir un
// WebSocket real: lo que importa es la identidad del objeto conexión.
vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: class {
    withUrl() { return this }
    withAutomaticReconnect() { return this }
    configureLogging() { return this }
    build() {
      return {
        state: 'Disconnected',
        onreconnecting: vi.fn(),
        onreconnected: vi.fn(),
        on: vi.fn(),
        off: vi.fn(),
        invoke: vi.fn().mockResolvedValue(undefined),
        start: vi.fn().mockResolvedValue(undefined),
        stop: vi.fn().mockResolvedValue(undefined),
      }
    }
  },
  LogLevel: { Warning: 'Warning' },
}))

const { TOKEN_STORAGE_KEY } = await import('../constants/auth')

/**
 * Regresión del bug de remitente: el WebSocket queda autenticado con el
 * token con el que se abrió. Si al cambiar de sesión no se cierra, los
 * mensajes se guardan con el identificador del usuario anterior.
 */
describe('resetHubConnection', () => {
  beforeEach(async () => {
    localStorage.clear()
    localStorage.setItem(TOKEN_STORAGE_KEY, 'token-de-pepito')
    vi.resetModules()
  })

  it('deja de reutilizar la conexión anterior', async () => {
    const hub = await import('./messagingHub')

    const primera = hub.getHubConnection()
    expect(primera).not.toBeNull()

    await hub.resetHubConnection()

    // Cambió la sesión: la siguiente conexión tiene que ser nueva.
    localStorage.setItem(TOKEN_STORAGE_KEY, 'token-de-jose')
    const segunda = hub.getHubConnection()

    expect(segunda).not.toBe(primera)
  })

  it('cierra la conexión que estaba abierta', async () => {
    const hub = await import('./messagingHub')

    const primera = hub.getHubConnection()
    await hub.resetHubConnection()

    expect(primera.stop).toHaveBeenCalled()
  })

  it('no rompe si no había ninguna conexión abierta', async () => {
    const hub = await import('./messagingHub')

    await expect(hub.resetHubConnection()).resolves.toBeUndefined()
  })

  it('no crea conexión si no hay token', async () => {
    localStorage.clear()
    const hub = await import('./messagingHub')

    expect(hub.getHubConnection()).toBeNull()
  })
})