import Modal from '../ui/Modal'
import Button from '../ui/Button'

/**
 * Confirmación antes de bloquear.
 *
 * Bloquear es una acción fuerte: termina la amistad, corta el seguimiento y
 * esconde los posts. Por eso se pregunta siempre, y el botón de cancelar
 * queda primero para que un toque rápido no borre nada por accidente.
 */
export default function BlockConfirmModal({ userName, onConfirm, onClose, busy = false }) {
  return (
    <Modal title="Confirmar bloqueo" onClose={onClose}>
      <div className="block-confirm">
        <p className="block-confirm__text">
          ¿Seguro que querés bloquear a <strong>{userName}</strong>?
        </p>

        <ul className="block-confirm__effects">
          <li>Va a dejar de ser tu amigo, si lo era.</li>
          <li>Dejará de seguirte y vos dejás de seguirlo.</li>
          <li>No vas a ver sus publicaciones ni él las tuyas.</li>
        </ul>

        <p className="block-confirm__note">Podés desbloquearlo cuando quieras.</p>

        <div className="dialog-actions">
          <Button variant="ghost" onClick={onClose} disabled={busy}>
            No, cancelar
          </Button>
          <Button variant="destructive" onClick={onConfirm} loading={busy}>
            Sí, bloquear
          </Button>
        </div>
      </div>
    </Modal>
  )
}