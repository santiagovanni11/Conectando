import { describe, it, expect, beforeEach } from 'vitest'
import { act, waitFor, renderHook } from '@testing-library/react'
import {
  hubState,
  message,
  resetHarness,
} from './useConversationMessagesTestHarness'
import { useConversationMessages } from './useConversationMessages'
import { editMessage, deleteMessage } from '../services/conversationService'

beforeEach(resetHarness)

const mount = () => renderHook(() => useConversationMessages('conv-1', 'user-1'))

describe('useConversationMessages: editar y eliminar por REST', () => {
  it('persiste la edición y la refleja en el hilo', async () => {
    editMessage.mockResolvedValue(message({ content: 'hola corregido', isEdited: true }))

    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    await act(async () => {
      await result.current.edit('m1', 'hola corregido')
    })

    expect(editMessage).toHaveBeenCalledWith('conv-1', 'm1', 'hola corregido')
    expect(result.current.messages[0].content).toBe('hola corregido')
    expect(result.current.messages[0].isEdited).toBe(true)
  })

  it('conserva el remitente al editar', async () => {
    editMessage.mockResolvedValue(message({ content: 'nuevo', isEdited: true }))

    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    await act(async () => {
      await result.current.edit('m1', 'nuevo')
    })

    // Si el reemplazo pierde el remitente, la burbuja deja de saber si es propia.
    expect(result.current.messages[0].sender.id).toBe('user-1')
  })

  it('expone el error si el servidor rechaza la edición', async () => {
    editMessage.mockRejectedValue(new Error('Solo se puede editar durante 15 minutos.'))

    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    await act(async () => {
      await result.current.edit('m1', 'nuevo')
    })

    expect(result.current.actionError).toContain('15 minutos')
    // El texto original debe seguir en pantalla: no se pierde lo escrito.
    expect(result.current.messages[0].content).toBe('hola original')
  })

  it('no llama al servidor si el texto queda vacío', async () => {
    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    await act(async () => {
      await result.current.edit('m1', '   ')
    })

    expect(editMessage).not.toHaveBeenCalled()
  })

  it('la edición sobrevive al poll de "visto"', async () => {
    // El poll reconsulta el hilo cada 4s. Si al volver a pintar el mensaje
    // usara el texto del servidor sin respetar la edición local, el cambio
    // desaparecería solo a los pocos segundos de guardarlo.
    editMessage.mockResolvedValue(message({ content: 'hola corregido', isEdited: true }))

    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    await act(async () => {
      await result.current.edit('m1', 'hola corregido')
    })
    expect(result.current.messages[0].content).toBe('hola corregido')

    // El servidor todavía devuelve el texto viejo en su página.
    await act(async () => {
      await result.current.reload()
    })

    expect(result.current.messages[0].content).toBe('hola corregido')
  })

  it('persiste el borrado y refleja el placeholder', async () => {
    deleteMessage.mockResolvedValue(message({ content: '', isDeleted: true }))

    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    await act(async () => {
      await result.current.remove('m1')
    })

    expect(result.current.messages[0].isDeleted).toBe(true)
  })

  it('usa el hub cuando hay conexión y no vuelve a guardar por REST', async () => {
    // El hub ya guarda y difunde: si además se llamara al REST, el mensaje
    // se editaría dos veces y el remitente recibiría dos avisos.
    hubState.connected = true
    editMessage.mockResolvedValue(message({ content: 'vía hub', isEdited: true }))

    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    await act(async () => {
      await result.current.edit('m1', 'vía hub')
    })

    expect(editMessage).not.toHaveBeenCalled()
  })

  it('cae al REST si el hub no está conectado', async () => {
    hubState.connected = false
    editMessage.mockResolvedValue(message({ content: 'vía rest', isEdited: true }))

    const { result } = mount()
    await waitFor(() => expect(result.current.messages).toHaveLength(1))

    await act(async () => {
      await result.current.edit('m1', 'vía rest')
    })

    // Sin hub el guardado tiene que funcionar igual, y actualizar la vista.
    expect(editMessage).toHaveBeenCalledWith('conv-1', 'm1', 'vía rest')
    expect(result.current.messages[0].content).toBe('vía rest')
  })
})