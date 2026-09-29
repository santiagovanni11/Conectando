import { apiRequest } from './api'

/**
 * Motivos de denuncia, en el mismo orden que se muestran.
 *
 * El backend valida contra su propia lista cerrada, así que agregar uno acá
 * sin tocarlo allá devolvería error: ambos lados tienen que coincidir.
 */
export const REPORT_REASONS = [
  { id: 'spam', label: 'Es spam o publicidad' },
  { id: 'harassment', label: 'Me acosa o me amenaza' },
  { id: 'hate_speech', label: 'Discurso de odio' },
  { id: 'violence', label: 'Violencia' },
  { id: 'nudity', label: 'Contenido sexual no consentido' },
  { id: 'false_identity', label: 'Se hace pasar por otra persona' },
  { id: 'other', label: 'Otro motivo' },
]

/** Máximo del comentario opcional; alineado con ReportLimits del backend. */
export const REPORT_DETAILS_MAX = 1000

export const reportService = {
  create(targetUserId, reason, details) {
    return apiRequest(`/api/reports/${targetUserId}`, {
      method: 'POST',
      body: { reason, details: details?.trim() || null },
    })
  },

  getOwn() {
    return apiRequest('/api/reports')
  },
}