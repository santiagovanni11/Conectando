import { useState } from 'react'
import Button from '../ui/Button'
import BlockConfirmModal from './BlockConfirmModal'
import ReportModal from '../report/ReportModal'
import { socialService } from '../../services/socialService'

/**
 * Acciones de seguridad sobre otro usuario: bloquear y denunciar.
 *
 * Viven aparte de las acciones sociales porque se usan en un momento
 * distinto —cuando algo salió mal— y porque el bloqueo necesita una
 * confirmación que SocialActions no debe conocer.
 */
export default function SafetyActions({ user, blockedByMe, busy, onChanged }) {
  const [confirmingBlock, setConfirmingBlock] = useState(false)
  const [reporting, setReporting] = useState(false)

  function handleBlock() {
    setConfirmingBlock(false)
    onChanged(() => socialService.block(user.id))
  }

  if (blockedByMe) return null

  return (
    <div className="safety-actions">
      <Button variant="ghost" size="sm" loading={busy} onClick={() => setConfirmingBlock(true)}>
        Bloquear
      </Button>

      <Button variant="ghost" size="sm" onClick={() => setReporting(true)}>
        Denunciar
      </Button>

      {confirmingBlock && (
        <BlockConfirmModal
          userName={user.displayName}
          onConfirm={handleBlock}
          onClose={() => setConfirmingBlock(false)}
          busy={busy}
        />
      )}

      {reporting && (
        <ReportModal user={user} onClose={() => setReporting(false)} />
      )}
    </div>
  )
}