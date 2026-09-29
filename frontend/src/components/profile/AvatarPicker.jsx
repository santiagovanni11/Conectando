import Avatar from '../ui/Avatar'
import Icon from '../ui/Icon/Icon'
import PhotoSourcePicker from '../upload/PhotoSourcePicker'

/**
 * Avatar que abre el selector de cámara/fototeca al make click.
 * Se usa para cambiar la foto de perfil.
 */
export default function AvatarPicker({ name, src, size = 'xl', onSelect, disabled, uploading }) {
  return (
    <div className="avatar-picker-wrap">
      <PhotoSourcePicker
        disabled={disabled || uploading}
        onSelect={(files) => onSelect?.(files[0])}
        trigger={
          <button
            type="button"
            className="avatar-picker"
            disabled={disabled || uploading}
            aria-label="Cambiar foto de perfil"
          >
            <Avatar name={name} src={src} alt={name} size={size} />
            <span className="avatar-picker__overlay" aria-hidden="true">
              <Icon name="camera" size="lg" />
            </span>
            {!uploading && <span className="avatar-picker__badge" aria-hidden="true"><Icon name="camera" size="sm" /></span>}
          </button>
        }
      />
      {uploading && <p className="avatar-picker__hint" role="status">Subiendo foto…</p>}
    </div>
  )
}