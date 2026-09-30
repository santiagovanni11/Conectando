import { API_BASE_URL } from '../constants/api'
import { TOKEN_STORAGE_KEY } from '../constants/auth'

export class ApiError extends Error {
  constructor(message, status) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export async function apiRequest(path, { method = 'GET', body } = {}) {
  const headers = { Accept: 'application/json' }
  const isFormData = body instanceof FormData
  if (body !== undefined && !isFormData) headers['Content-Type'] = 'application/json'

  const token = localStorage.getItem(TOKEN_STORAGE_KEY)
  if (token) headers.Authorization = `Bearer ${token}`

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: body !== undefined ? (isFormData ? body : JSON.stringify(body)) : undefined,
  })

  /*
   * Se lee el cuerpo como texto y recién después se interpreta.
   *
   * La regla es por contenido, no por código de estado: un 202 Accepted
   * —que es lo que devuelve "te mandamos el código"— llega con el cuerpo
   * vacío igual que un 204, y los dos son respuestas válidas de una
   * operación que no devuelve nada. Distinguir por el status obligaba a
   * enumerar uno por uno los que llegan vacíos, y el que se olvidara rompía.
   *
   * Lo que sí es una respuesta rota es un cuerpo con contenido que no es
   * JSON: suele ser el proxy devolviendo una página de error o de
   * "despierta" cuando el servicio vuelve de una suspensión. Eso se
   * reporta acá para que cada consumidor lo trate con el try/catch que ya
   * tiene, en vez de recibir un null y reventar al leer una propiedad.
   */
  const texto = await response.text()
  const tieneCuerpo = texto.trim().length > 0
  let data = null

  if (tieneCuerpo) {
    try {
      data = JSON.parse(texto)
    } catch {
      throw new ApiError(
        `Respuesta ilegible del servidor (HTTP ${response.status})`,
        response.status,
      )
    }
  }

  if (!response.ok) {
    const message = data?.message ?? `Error ${response.status}`
    throw new ApiError(message, response.status)
  }

  return data
}