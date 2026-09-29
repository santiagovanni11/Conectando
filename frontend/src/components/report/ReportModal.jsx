import { useState } from 'react'
import Modal from '../ui/Modal'
import Button from '../ui/Button'
import Textarea from '../ui/Textarea'
import ErrorState from '../ui/ErrorState'
import { REPORT_DETAILS_MAX, REPORT_REASONS } from '../../services/reportService'
import { useReportUser } from '../../hooks/useReportUser'

/**
 * Modal de denuncia.
 *
 * El motivo es obligatorio y se elige de una lista cerrada; el comentario
 * es opcional. Se cuentan los caracteres porque el backend los recorta.
 */
export default function ReportModal({ user, onClose, onReported }) {
  const [reason, setReason] = useState('')
  const [details, setDetails] = useState('')
  const { send, sending, error, sent } = useReportUser()

  async function handleSubmit(event) {
    event.preventDefault()
    if (!reason || sending) return

    if (await send(user.id, reason, details)) onReported?.()
  }

  if (sent) {
    return (
      <Modal title="Denuncia enviada" onClose={onClose}>
        <div className="report-modal__done">
          <p>
            Gracias por avisarnos. Vamos a revisar lo que reportaste y lo
            vamos a tener en cuenta.
          </p>
          <Button variant="primary" onClick={onClose}>
            Cerrar
          </Button>
        </div>
      </Modal>
    )
  }

  return (
    <Modal title={`Denunciar a ${user.displayName}`} onClose={onClose}>
      <form className="report-modal" onSubmit={handleSubmit}>
        <p className="report-modal__lead">
          ¿Por qué querés denunciar esta cuenta? La información se envía de
          forma confidencial.
        </p>

        <fieldset className="report-modal__reasons">
          <legend className="report-modal__legend">Motivo</legend>
          {REPORT_REASONS.map((option) => (
            <label key={option.id} className="report-modal__reason">
              <input
                type="radio"
                name="report-reason"
                value={option.id}
                checked={reason === option.id}
                onChange={() => setReason(option.id)}
              />
              <span>{option.label}</span>
            </label>
          ))}
        </fieldset>

        <label className="report-modal__details-label" htmlFor="report-details">
          Agregá información (opcional)
        </label>
        <Textarea
          id="report-details"
          value={details}
          onChange={(event) => setDetails(event.target.value)}
          maxLength={REPORT_DETAILS_MAX}
          rows={4}
          placeholder="Contanos qué pasó."
        />

        {error && <ErrorState description={error} />}

        <div className="dialog-actions">
          <Button variant="ghost" onClick={onClose} disabled={sending}>
            Cancelar
          </Button>
          <Button type="submit" variant="destructive" loading={sending} disabled={!reason}>
            Enviar denuncia
          </Button>
        </div>
      </form>
    </Modal>
  )
}