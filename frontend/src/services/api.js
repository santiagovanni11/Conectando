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

  // Un 204 no tiene cuerpo y es una respuesta legítima: esos endpoints
  // existen justamente para confirmar sin devolver nada.
  const vacio = response.status === 204

  let data = null
  try {
    data = await response.json()
  } catch {
    if (!vacio) {
      // Un 2xx con cuerpo que no es JSON no es una respuesta válida: suele
      // ser el proxy devolviendo una página de error o de "despierta",
      // que pasa cuando el servicio vuelve de una suspensión.
      //
      // Antes se devolvía `null` como si fuera un resultado, y cada
      // consumidor que hacía `data.algo` reventaba en un pantalla
      // distinta, muy lejos de la causa. Romper acá deja que el
      // try/catch que ya tiene cada consumidor lo trate como lo que es.
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