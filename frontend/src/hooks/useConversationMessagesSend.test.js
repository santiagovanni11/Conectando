import { describe, it, expect, beforeEach } from 'vitest'
import { act, waitFor } from '@testing-library/react'
// El andamiaje va antes del servicio a propósito: es él quien registra el
// mock, y el orden de los imports decide si se trae la versión real.
import { renderLoaded, resetHarness, hubState } from './useConversationMessagesTestHarness'
import { sendMessage } from '../services/conversationService'

/**
 * El aviso de error al enviar.
 *
 * Vive aparte porque es un contrato distinto al del resto del hilo: el hub
 * ya no manda eventos de error, sube la excepción, y por eso quien tiene
 * que avisar es el propio envío.
 */
describe('useConversationMessages - error al enviar', () => {
  beforeEach(() => {
    resetHarness()
    // El hub se cae, que es lo que hace que el envío vuelva al REST.
    hubState.sendOk = false
  })

  it('avisa con el mensaje que devuelve el REST', async () => {
    // El camino real: el hub no está, el REST lo rechaza y el motivo
    // concreto es lo que el usuario tiene que ver.
    sendMessage.mockRejectedValue(new Error('No sos miembro de esta conversación.'))

    const { result } = await renderLoaded()
    await act(async () => {
      await result.current.send('hola')
    })

    await waitFor(() =>
      expect(result.current.sendError).toBe('No sos miembro de esta conversación.'),
    )
  })

  it('usa un texto propio cuando el fallo no trae motivo', async () => {
    sendMessage.mockRejectedValue(new Error(''))

    const { result } = await renderLoaded()
    await act(async () => {
      await result.current.send('hola')
    })

    await waitFor(() => expect(result.current.sendError).toBe('No se pudo enviar el mensaje.'))
  })

  it('limpia el error anterior cuando un envio nuevo sale bien', async () => {
    sendMessage.mockRejectedValueOnce(new Error('Fallo anterior'))

    const { result } = await renderLoaded()
    await act(async () => {
      await result.current.send('hola')
    })
    await waitFor(() => expect(result.current.sendError).not.toBe(''))

    sendMessage.mockResolvedValueOnce({ id: 'm2' })
    await act(async () => {
      await result.current.send('otro')
    })

    await waitFor(() => expect(result.current.sendError).toBe(''))
  })

  it('no toca nada si el mensaje viene vacio', async () => {
    const { result } = await renderLoaded()
    await act(async () => {
      await result.current.send('   ')
    })

    expect(sendMessage).not.toHaveBeenCalled()
    expect(result.current.sendError).toBe('')
  })
})
