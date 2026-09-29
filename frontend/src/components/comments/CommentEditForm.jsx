import { useState } from 'react'
import { MAX_COMMENT_LENGTH } from '../../constants/comments'

export default function CommentEditForm({ initialContent, saving = false, onSave, onCancel }) {
  const [content, setContent] = useState(initialContent)
  const isValid = content.trim().length > 0

  function handleSubmit(event) {
    event.preventDefault()
    if (!isValid || saving) return
    onSave(content.trim())
  }

  return (
    <form className="comment-edit" onSubmit={handleSubmit}>
      <textarea
        className="comment-edit__input"
        value={content}
        onChange={(event) => setContent(event.target.value)}
        maxLength={MAX_COMMENT_LENGTH}
        rows={2}
        autoFocus
        aria-label="Editar comentario"
      />
      <div className="comment-edit__actions">
        <button type="button" className="comment-edit__btn" onClick={onCancel} disabled={saving}>
          Cancelar
        </button>
        <button
          type="submit"
          className="comment-edit__btn comment-edit__btn--primary"
          disabled={!isValid || saving}
        >
          Guardar
        </button>
      </div>
    </form>
  )
}
