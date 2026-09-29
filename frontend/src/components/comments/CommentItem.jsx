import { useState } from 'react'
import Avatar from '../ui/Avatar'
import CommentEditForm from './CommentEditForm'
import Loader from '../ui/Loader'
import { formatRelativeTime } from '../../utils/dateFormatter'
import { useAuth } from '../../hooks/useAuth'

export default function CommentItem({
  comment,
  isReply = false,
  submitting = false,
  replies = [],
  repliesLoading = false,
  repliesOpen = false,
  repliesError = false,
  onToggleReplies,
  onReply,
  onEdit,
  onDelete,
}) {
  const { user } = useAuth()
  const [editing, setEditing] = useState(false)

  const isOwn = Boolean(user && user.id === comment.authorId)
  const hasReplies = (comment.repliesCount ?? 0) > 0 || replies.length > 0

  function handleSave(content) {
    onEdit(comment.id, content)
    setEditing(false)
  }

  if (comment.isDeleted) {
    return (
      <div className="comment comment--deleted">
        <p className="comment__deleted-text">Este comentario fue eliminado.</p>
      </div>
    )
  }

  return (
    <div className={`comment${isReply ? ' comment--reply' : ''}`}>
      <Avatar
        name={comment.author.displayName}
        src={comment.author.profileImageUrl}
        size={isReply ? 'xs' : 'sm'}
        className="comment__avatar"
        alt=""
      />

      <div className="comment__body">
        <header className="comment__header">
          <span className="comment__author">{comment.author.displayName}</span>
          <span className="comment__date">
            {formatRelativeTime(comment.createdAt)}
            {comment.isEdited ? ' · editado' : ''}
          </span>
        </header>

        {editing ? (
          <CommentEditForm
            initialContent={comment.content}
            saving={submitting}
            onSave={handleSave}
            onCancel={() => setEditing(false)}
          />
        ) : (
          <p className="comment__content">{comment.content}</p>
        )}

        <div className="comment__actions">
          {!isReply && (
            <>
              <button type="button" className="comment__action" onClick={() => onReply(comment)}>
                Responder
              </button>
              {hasReplies && (
                <button
                  type="button"
                  className="comment__action"
                  onClick={() => onToggleReplies(comment)}
                  disabled={repliesLoading}
                >
                  {repliesLoading
                    ? 'Cargando respuestas…'
                    : repliesOpen
                      ? `Ocultar respuestas (${comment.repliesCount ?? replies.length})`
                      : `Ver respuestas (${comment.repliesCount ?? replies.length})`}
                </button>
              )}
            </>
          )}
          {isReply && (
            <span className="comment__reply-indicator">
              ↳ Respondiendo a <span className="comment__reply-name">@{comment.author.userName}</span>
            </span>
          )}
          {isOwn && !editing && (
            <>
              <button type="button" className="comment__action" onClick={() => setEditing(true)}>
                Editar
              </button>
              <button
                type="button"
                className="comment__action comment__action--danger"
                onClick={() => onDelete(comment)}
              >
                Eliminar
              </button>
            </>
          )}
        </div>

        {!isReply && repliesOpen && (
          <div className="comment__replies">
            {repliesLoading && replies.length === 0 ? (
              <span className="comment__replies-loading">
                <Loader size="sm" label="Cargando respuestas…" />
              </span>
            ) : (
              replies.map((reply) => (
                <div key={reply.id} className="comment__reply">
                  <CommentItem
                    comment={reply}
                    isReply
                    submitting={submitting}
                    onEdit={onEdit}
                    onDelete={onDelete}
                    onToggleReplies={onToggleReplies}
                  />
                </div>
              ))
            )}
            {repliesError && <p className="comment__replies-error">No se pudieron cargar las respuestas.</p>}
            {!repliesLoading && !repliesError && replies.length === 0 && (
              <p className="comment__replies-empty">Aún no hay respuestas.</p>
            )}
          </div>
        )}
      </div>
    </div>
  )
}