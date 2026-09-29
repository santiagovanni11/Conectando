import { useState } from 'react'
import Avatar from '../ui/Avatar'
import Icon from '../ui/Icon/Icon'
import Loader from '../ui/Loader'
import { useAuth } from '../../hooks/useAuth'
import { useAutoResize } from '../../hooks/useAutoResize'
import { MAX_COMMENT_LENGTH } from '../../constants/comments'

export default function CommentComposer({
  onSubmit,
  submitting = false,
  placeholder = 'Escribí un comentario...',
  submitLabel = 'Publicar comentario',
  replyTo = null,
  onCancel,
  autoFocus = false,
}) {
  const { user } = useAuth()
  const [content, setContent] = useState('')
  const textareaRef = useAutoResize(content, 180)

  const trimmed = content.trim()
  const isValid = trimmed.length > 0 && trimmed.length <= MAX_COMMENT_LENGTH
  const canSubmit = isValid && !submitting
  const atLimit = content.length >= MAX_COMMENT_LENGTH
  const showCounter = content.length > 0 && !atLimit

  async function handleSubmit(event) {
    event.preventDefault()
    if (!canSubmit) return
    const success = await onSubmit(trimmed)
    if (success) setContent('')
  }

  function handleCancel() {
    onCancel?.()
  }

  return (
    <form
      className={`comment-composer${replyTo ? ' comment-composer--reply' : ''}`}
      onSubmit={handleSubmit}
    >
      {replyTo && (
        <div className="comment-composer__replying">
          <Icon name="chevron-right" size="sm" className="comment-composer__replying-icon" />
          <span className="comment-composer__replying-text">
            Respondiendo a{' '}
            <span className="comment-composer__reply-name">@{replyTo.author.userName}</span>
          </span>
          <button type="button" className="comment-composer__cancel" onClick={handleCancel} disabled={submitting}>
            Cancelar
          </button>
        </div>
      )}

      <div className="comment-composer__box">
        <Avatar
          name={user?.displayName ?? ''}
          src={user?.profileImageUrl}
          size="xs"
          className="comment-composer__avatar"
        />
        <textarea
          ref={textareaRef}
          className="comment-composer__input"
          value={content}
          onChange={(event) => setContent(event.target.value)}
          placeholder={placeholder}
          rows={1}
          maxLength={MAX_COMMENT_LENGTH}
          disabled={submitting}
          autoFocus={autoFocus}
          aria-label={replyTo ? 'Escribir tu respuesta' : 'Escribir un comentario'}
        />
        <button
          type="submit"
          className={`comment-composer__send${submitting ? ' comment-composer__send--loading' : ''}`}
          disabled={!canSubmit}
          aria-busy={submitting || undefined}
          aria-label={submitLabel}
          title={submitLabel}
        >
          {submitting ? <Loader size="sm" /> : <Icon name="send" size="md" />}
        </button>
      </div>

      {showCounter && (
        <p
          className="comment-composer__counter"
          aria-live="polite"
        >
          {content.length} / {MAX_COMMENT_LENGTH}
        </p>
      )}
    </form>
  )
}