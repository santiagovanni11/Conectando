/**
 * Recepción de los cambios que hace el otro usuario.
 *
 * Va en su propio archivo porque es otra responsabilidad: los tests del
 * archivo anterior comprueban que lo que hago yo se guarde; estos comprueban
 * que lo que hace el otro me llega sin recargar.
 */
import { describe, it, expect, beforeEach } from 'vitest'
import { act, waitFor, renderHook } from '@testing-library/react'
import {
  hubHandlers,
  message,
  resetHarness,
} from './useConversationMessagesTestHarness'
import { useConversationMessages } from './useConversationMessages'

const OTRO = { id: 'user-2', displayName: 'Ana' }

beforeEach(resetHarness)

const mount = () => renderHook(() => useConversationMessages('conv-1', 'user-1'))

describe('useConversationMessages: cambios del otro en vivo', () => {
  it('refleja la edición del otro sin recargar', async () => {
    // El caso que motivó el cambio: el otro edita y hay que verlo al instante.
    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))
    expect(result.current.messages[0].content).toBe('hola original')

    act(() => {
      hubHandlers.current.onUpdated(
        message({ content: 'hola editado por el otro', isEdited: true, sender: OTRO }),
      )
    })

    expect(result.current.messages[0].content).toBe('hola editado por el otro')
    expect(result.current.messages[0].isEdited).toBe(true)
  })

  it('refleja el borrado del otro sin recargar', async () => {
    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    act(() => {
      hubHandlers.current.onDeleted(
        message({ content: '', isDeleted: true, sender: OTRO }),
      )
    })

    expect(result.current.messages[0].isDeleted).toBe(true)
  })

  it('ignora un cambio belonging a otro chat', async () => {
    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    act(() => {
      hubHandlers.current.onUpdated(
        message({ conversationId: 'otra-conv', content: 'de otro hilo', sender: OTRO }),
      )
    })

    expect(result.current.messages[0].content).toBe('hola original')
  })

  it('agrega el mensaje propio que vuelve por el hub, sin duplicarlo', async () => {
    // El emisor tambien necesita verlo: nadie lo suma a la lista por otro
    // lado, asi que descartarlo lo dejaba invisible hasta recargar el chat.
    // Lo que evita el duplicado no es descartarlo, sino que applyIncoming
    // ignore los ids repetidos.
    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    const propio = message({
      id: 'm2',
      content: 'hola',
      sender: { id: 'user-1', displayName: 'Yo' },
    })

    act(() => {
      hubHandlers.current.onMessage(propio)
    })

    expect(result.current.messages).toHaveLength(2)

    act(() => {
      hubHandlers.current.onMessage(propio)
    })

    expect(result.current.messages).toHaveLength(2)
  })


  it('sí agrega el mensaje que envía el otro', async () => {
    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    act(() => {
      hubHandlers.current.onMessage(
        message({ id: 'm2', content: 'hola Ana', sender: OTRO }),
      )
    })

    expect(result.current.messages).toHaveLength(2)
    expect(result.current.messages[1].content).toBe('hola Ana')
  })
})
