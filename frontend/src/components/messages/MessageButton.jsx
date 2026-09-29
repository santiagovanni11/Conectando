import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Button from '../ui/Button'
import Icon from '../ui/Icon/Icon'
import { startDirectConversation } from '../../services/conversationService'
import { ROUTES } from '../../constants/routes'

/**
 * Inicia (o reutiliza) la conversación con un usuario y abre el hilo.
 * Se muestra en el perfil de otra persona.
 */
export default function MessageButton({ userId, size = 'sm', variant = 'primary' }) {
  const navigate = useNavigate()
  const [isBusy, setIsBusy] = useState(false)
  const [error, setError] = useState(null)

  const handleClick = async () => {
    setIsBusy(true)
    setError(null)
    try {
      // Si la conversación ya existe la devuelve; no se duplica.
      const conversation = await startDirectConversation(userId)
      navigate(ROUTES.conversation(conversation.id))
    } catch (cause) {
      setError(cause.message ?? 'No se pudo abrir la conversación.')
    } finally {
      setIsBusy(false)
    }
  }

  return (
    <span className="message-button">
      <Button
        variant={variant}
        size={size}
        loading={isBusy}
        icon={<Icon name="chat" size="sm" />}
        onClick={handleClick}
      >
        Enviar mensaje
      </Button>
      {error && (
        <span className="message-button__error" role="alert">
          {error}
        </span>
      )}
    </span>
  )
}