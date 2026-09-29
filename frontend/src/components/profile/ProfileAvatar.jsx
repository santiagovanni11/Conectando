import { useRef, useState } from 'react'
import Avatar from '../ui/Avatar'
import Icon from '../ui/Icon/Icon'
import PhotoSourcePicker from '../upload/PhotoSourcePicker'
import AvatarActionsMenu from './AvatarActionsMenu'
import PhotoAdjuster from './PhotoAdjuster'
import { useLongPress } from '../../hooks/useLongPress'
import { avatarUrl } from '../../utils/cloudinary'

/**
 * El avatar del perfil propio, con las acciones para cambiarlo.
 *
 * Abre el menú de dos maneras, a propósito. La pulsación larga es lo que piden
 * las otras apps y ya la sabés hacer, pero es invisible: nadie adivina que hay
 * que sostener el dedo. Por eso además aparece un botón de cámara al pasar el
 * mouse o al tocar, que abre lo mismo con un toque. Una vía sola deja afuera a
 * la mitad de la gente.
 *
 * Cuando no es el perfil propio, no hay menú: es una imagen y nada más.
 */
export default function ProfileAvatar({
  name,
  src,
  framing,
  isOwn = false,
  size = 'xl',
  uploading = false,
  onSelectPhoto,
  onSaveFraming,
  savingFraming = false,
}) {
  const [menuAbierto, setMenuAbierto] = useState(false)
  const [ajustando, setAjustando] = useState(false)
  const boton = useRef(null)

  const { isPressing, handlers } = useLongPress(() => setMenuAbierto(true))

  const encuadre = {
    zoom: framing?.zoom ?? 1,
    offsetX: framing?.offsetX ?? 0,
    offsetY: framing?.offsetY ?? 0,
  }

  if (!isOwn) {
    return <Avatar name={name} src={avatarUrl(src, encuadre)} alt={name} size={size} />
  }

  // "Ajustar" solo tiene sentido si hay algo que ajustar.
  const puedeAjustar = Boolean(src)

  // El botón ya abre el selector de cámara/fototeca, así que "Nueva foto" no
  // necesita su propio camino: alcanza con apretarlo.
  const nuevaFoto = () => boton.current?.click()

  return (
    <div className={`profile-avatar ${isPressing ? 'profile-avatar--pressing' : ''}`} {...handlers}>
      <PhotoSourcePicker
        multiple={false}
        onSelect={(files) => onSelectPhoto?.(files[0])}
        disabled={uploading || ajustando}
        trigger={
          <button
            ref={boton}
            type="button"
            className="profile-avatar__hit"
            aria-label="Opciones de la foto de perfil"
            aria-haspopup="menu"
            aria-expanded={menuAbierto}
          >
            <Avatar name={name} src={avatarUrl(src, encuadre)} alt={name} size={size} />
            <span className="profile-avatar__overlay" aria-hidden="true">
              <Icon name="camera" size="lg" />
            </span>
            {uploading && <span className="profile-avatar__status">Subiendo…</span>}
          </button>
        }
      />

      <AvatarActionsMenu
        open={menuAbierto}
        onClose={() => setMenuAbierto(false)}
        onNewPhoto={nuevaFoto}
        onAdjust={() => setAjustando(true)}
        canAdjust={puedeAjustar}
      />

      {ajustando && puedeAjustar && (
        <PhotoAdjuster
          src={src}
          framing={encuadre}
          saving={savingFraming}
          onCancel={() => setAjustando(false)}
          onSave={async (nuevo) => {
            const ok = await onSaveFraming?.(nuevo)
            if (ok !== false) setAjustando(false)
          }}
        />
      )}
    </div>
  )
}
