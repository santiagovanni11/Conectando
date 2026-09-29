import Avatar from '../ui/Avatar'
import Button from '../ui/Button'
import Icon from '../ui/Icon/Icon'
import ComposerMediaGrid from './ComposerMediaGrid'
import ComposerPrivacyPicker from './ComposerPrivacyPicker'
import PhotoSourcePicker from '../upload/PhotoSourcePicker'
import { useAutoResize } from '../../hooks/useAutoResize'
import { usePostComposer } from '../../hooks/usePostComposer'
import { MAX_POST_MEDIA } from '../../constants/post'

export default function PostComposerForm({
  initialPost = null,
  avatarName,
  avatarSrc,
  onPublished,
  onCancel,
  autoFocus = false,
}) {
  const {
    content, setContent, privacy, setPrivacy, drafts, uploading, publishing,
    error, canPublish, selectFiles, removeDraft, moveDraft, publish,
  } = usePostComposer(initialPost)
  const textareaRef = useAutoResize(content, 320)

  const isEdit = Boolean(initialPost)
  const isFull = drafts.length >= MAX_POST_MEDIA

  async function submit() {
    const result = await publish()
    if (result) onPublished?.(result)
  }

  return (
    <form
      className="composer-form"
      onSubmit={(event) => {
        event.preventDefault()
        submit()
      }}
    >
      <div className="composer-form__author">
        <Avatar name={avatarName} src={avatarSrc} alt={avatarName} size="md" />
        <div className="composer-form__identity">
          <span className="composer-form__name">{avatarName}</span>
          <ComposerPrivacyPicker value={privacy} onChange={setPrivacy} />
        </div>
      </div>

      <textarea
        ref={textareaRef}
        className="composer-form__input"
        value={content}
        onChange={(event) => setContent(event.target.value)}
        placeholder={isEdit ? 'Editá tu publicación…' : '¿Qué estás pensando?'}
        maxLength={2000}
        rows={1}
        autoFocus={autoFocus}
        aria-label="Texto de la publicación"
      />

      <ComposerMediaGrid drafts={drafts} uploading={uploading} onRemove={removeDraft} onMove={moveDraft} />

      {error && <p className="composer-form__error" role="alert">{error}</p>}

      <footer className="composer-form__footer">
        <PhotoSourcePicker
          multiple
          disabled={isFull}
          onSelect={selectFiles}
          trigger={
            <Button
              type="button"
              variant="secondary"
              size="sm"
              icon={<Icon name="camera" size="sm" />}
              loading={uploading}
              disabled={isFull}
              className="composer-form__add"
            >
              {isFull ? `Máximo ${MAX_POST_MEDIA} fotos` : 'Agregar fotos'}
            </Button>
          }
        />

        <div className="composer-form__actions">
          {isEdit && onCancel && (
            <Button type="button" variant="ghost" size="sm" disabled={publishing} onClick={onCancel}>
              Cancelar
            </Button>
          )}
          <Button type="submit" variant="primary" size="sm" loading={publishing} disabled={!canPublish}>
            {isEdit ? 'Guardar' : 'Publicar'}
          </Button>
        </div>
      </footer>
    </form>
  )
}