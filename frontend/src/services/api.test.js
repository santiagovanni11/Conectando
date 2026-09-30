import { describe, it, expect, vi, beforeEach } from 'vitest'
import { apiRequest, ApiError } from './api'

/**
 * El cliente HTTP.
 *
 * Acá se decide qué se considera una respuesta válida, y esa decisión la
 * toman todos los servicios de la app. Por eso los casos raros —cuerpo
 * vacío, cuerpo que no es JSON— se prueban acá y no en cada consumidor.
 */
const fetchMock = vi.fn()

/** Respuesta con cuerpo de texto, como la que devuelve fetch. */
function respuesta(status, cuerpo) {
  return {
    ok: status >= 200 && status < 300,
    status,
    text: async () => cuerpo,
  }
}

beforeEach(() => {
  vi.clearAllMocks()
  globalThis.fetch = fetchMock
})

describe('apiRequest', () => {
  it('devuelve el JSON cuando la respuesta lo trae', async () => {
    fetchMock.mockResolvedValueOnce(respuesta(200, '{"unreadMessages":3}'))

    await expect(apiRequest('/api/x')).resolves.toEqual({ unreadMessages: 3 })
  })

  it('acepta un 202 con el cuerpo vacío', async () => {
    // Regresión: el endpoint de "te mandamos el código" responde 202 sin
    // cuerpo. La regla anterior solo exceptuaba el 204, así que esto
    // tiraba "Respuesta ilegible del servidor" y el flujo de recuperación no
    // arrancaba nunca. La regla ahora es por contenido: si no hay cuerpo,
    // es una respuesta válida, diga el código que diga.
    fetchMock.mockResolvedValueOnce(respuesta(202, ''))

    await expect(
      apiRequest('/api/auth/password-reset/request', { method: 'POST' }),
    ).resolves.toBeNull()
  })

  it('acepta un 204 con el cuerpo vacío', async () => {
    fetchMock.mockResolvedValueOnce(respuesta(204, ''))

    await expect(
      apiRequest('/api/conversations/c1/read', { method: 'POST' }),
    ).resolves.toBeNull()
  })

  it('acepta un cuerpo vacío con espacios', async () => {
    fetchMock.mockResolvedValueOnce(respuesta(202, '   \n '))

    await expect(apiRequest('/api/x')).resolves.toBeNull()
  })

  it('falla si el cuerpo no es JSON, aunque el status sea 200', () => {
    // La página de "despierta" del proxy: hay contenido, pero no es una
    // respuesta. Antes se devolvía null como si fuera un resultado y el
    // consumidor reventaba al leer una propiedad, muy lejos de la causa.
    fetchMock.mockResolvedValueOnce(respuesta(200, '<html>Service waking up</html>'))

    return expect(apiRequest('/api/x')).rejects.toThrow(/ilegible/i)
  })

  it('propaga el mensaje del servidor cuando la llamada falla', async () => {
    fetchMock.mockResolvedValueOnce(respuesta(400, '{"message":"El código no es válido."}'))

    await expect(apiRequest('/api/x')).rejects.toThrow('El código no es válido.')
  })

  it('un error sin mensaje conserva el status', async () => {
    fetchMock.mockResolvedValueOnce(respuesta(429, ''))

    await expect(apiRequest('/api/x')).rejects.toMatchObject({ status: 429 })
  })

  it('manda el cuerpo serializado y el token', async () => {
    localStorage.setItem('conectando.token', 'abc')
    fetchMock.mockResolvedValueOnce(respuesta(200, '{}'))

    await apiRequest('/api/x', { method: 'POST', body: { email: 'a@b.c' } })

    const [, opciones] = fetchMock.mock.calls[0]
    expect(opciones.body).toBe(JSON.stringify({ email: 'a@b.c' }))
    expect(opciones.headers.Authorization).toBe('Bearer abc')

    localStorage.removeItem('conectando.token')
  })

  it('el error que tira es un ApiError', async () => {
    fetchMock.mockResolvedValueOnce(respuesta(400, '{"message":"mal"}'))

    await expect(apiRequest('/api/x')).rejects.toBeInstanceOf(ApiError)
  })
})
