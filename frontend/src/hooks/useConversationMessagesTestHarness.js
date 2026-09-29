/**
 * Andamiaje compartido de los tests del hilo.
 *
 * Vive aparte porque los dos archivos de test lo necesitan: aislar el
 * WebSocket sin perder la posibilidad de dispararlo a mano. Duplicarlo
 * haría que un cambio en los mocks se tuviera que aplicar en dos lugares.
 */
import { expect, vi } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useConversationMessages } from './useConversationMessages'
import { fetchMessages } from '../services/conversationService'

// Los mocks se registran con una fábrica que no depende de las importaciones
// de arriba: Vitest las sube, y usarlas capturaría referencias sin
// inicializar. Solo se reexportan los datos y helpers, nunca los mocks.
vi.mock('../services/conversationService', () => ({
  fetchMessages: vi.fn(),
  editMessage: vi.fn(),
  deleteMessage: vi.fn(),
  sendMessage: vi.fn(),
  markConversationAsRead: vi.fn().mockResolvedValue(undefined),
}))

/**
 * Por defecto el hub está desconectado, así los tests ejercitan el camino
 * REST. El caso con conexión se activa poniendo `connected` en true.
 *
 * `sendOk` corre por separado porque el envío es el único camino que
 * decide entre hub y REST según la respuesta, y los tests necesitan poder
 * forzarlo en los dos sentidos.
 */
export const hubState = { connected: false, sendOk: true }

vi.mock('./useConversationHub', () => ({
  useConversationHub: () => ({
    joinConversation: vi.fn(),
    leaveConversation: vi.fn(),
    sendViaHub: vi.fn().mockResolvedValue(hubState.sendOk),
    markReadViaHub: vi.fn().mockResolvedValue(true),
    editMessageViaHub: vi.fn().mockResolvedValue(hubState.connected),
    deleteMessageViaHub: vi.fn().mockResolvedValue(hubState.connected),
  }),
}))

/** Handlers del hub: se guardan para dispararlos desde el test. */
export const hubHandlers = { current: {} }

vi.mock('./useMessageHub', () => ({
  useMessageHub: (handlers) => {
    hubHandlers.current = handlers
  },
}))

export function message(overrides = {}) {
  return {
    id: 'm1',
    conversationId: 'conv-1',
    content: 'hola original',
    createdAt: '2026-01-01T10:00:00Z',
    isSeenByPeer: false,
    isEdited: false,
    isDeleted: false,
    sender: { id: 'user-1', displayName: 'Yo' },
    ...overrides,
  }
}

/** Monta el hilo con un mensaje cargado y espera a que esté listo. */
export async function renderLoaded() {
  const view = renderHook(() => useConversationMessages('conv-1', 'user-1'))
  await waitFor(() => expect(view.result.current.messages).toHaveLength(1))
  return view
}

/** Vuelve el harness al estado inicial: REST y mocks limpios. */
export function resetHarness() {
  vi.clearAllMocks()
  hubState.connected = false
  hubState.sendOk = true
  fetchMessages.mockResolvedValue({ items: [message()], nextCursor: null, hasMore: false })
}