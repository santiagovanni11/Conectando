import { useState } from 'react'
import Avatar from '../ui/Avatar'
import CommentEditForm from './CommentEditForm'
import CommentLikeButton from './CommentLikeButton'
import CommentReplies from './CommentReplies'
import UserHandle from '../users/UserHandle'
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
  liking = null,
  onToggleReplies,
  onReply,
  onEdit,
  onDelete,
  onToggleLike,
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
      {/* El avatar también lleva al perfil, como en cualquier red: es el
          blanco más grande y el más fácil de tocar. */}
      <UserHandle user={comment.author} className="comment__avatar-link">
        <Avatar
          name={comment.author.displayName}
          src={comment.author.profileImageUrl}
          size={isReply ? 'xs' : 'sm'}
          className="comment__avatar"
          alt=""
        />
      </UserHandle>

      <div className="comment__body">
        <header className="comment__header">
          <UserHandle user={comment.author} className="comment__author">
            {comment.author.displayName}
          </UserHandle>
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
          <CommentLikeButton
            comment={comment}
            busy={liking === comment.id}
            onToggle={onToggleLike}
          />
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
              ↳ Respondiendo a <UserHandle user={comment.author} className="comment__reply-name" />
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
          <CommentReplies
            replies={replies}
            loading={repliesLoading}
            error={repliesError}
            submitting={submitting}
            liking={liking}
            onToggle={onToggleReplies}
            onToggleLike={onToggleLike}
            onEdit={onEdit}
            onDelete={onDelete}
          />
        )}
      </div>
    </div>
  )
}