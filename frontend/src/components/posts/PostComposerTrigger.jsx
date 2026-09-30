import Avatar from '../ui/Avatar'
import Icon from '../ui/Icon/Icon'

/**
 * Disparador del compositor: la tarjeta que invita a escribir.
 *
 * Va aparte del modal porque los dos tienen ciclos de vida distintos: este
 * se ve siempre, el modal solo mientras se está escribiendo. Además deja
 * el disparador —lo que se ve en el feed— separado de lo que se abre.
 *
 * El ícono de foto es decorativo y no un botón: la única acción posible
 * acá es abrir el compositor. Si fuera un botón, un lector de pantalla
 * contaría dos controles donde hay uno, y el dedo lo tocaría esperando
 * algo que no pasa.
 */
export default function PostComposerTrigger({ displayName, avatarSrc, onClick }) {
  return (
    <button
      type="button"
      className="composer-trigger"
      onClick={onClick}
      aria-label="Crear una nueva publicación"
    >
      <Avatar name={displayName} src={avatarSrc} alt="" size="md" />

      <span className="composer-trigger__prompt">
        ¿Qué estás pensando, {displayName}?
      </span>

      <span className="composer-trigger__photo" aria-hidden="true">
        <Icon name="camera" size="md" />
      </span>
    </button>
  )
}
