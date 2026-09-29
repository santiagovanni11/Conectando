import { useEffect, useState } from 'react'
import CommentItem from './CommentItem'
import CommentComposer from './CommentComposer'
import { usePostComments } from '../../hooks/usePostComments'
import { useCommentActions } from '../../hooks/useCommentActions'

const DELETED_TEXT = 'Este comentario fue eliminado.'

export default function CommentsSection({ postId, onCountChange }) {
  const [replyingTo, setReplyingTo] = useState(null)
  const postComments = usePostComments(postId)
  const { submitting, error: actionError, createComment, updateComment: updateCommentApi, deleteComment } = useCommentActions(postId)
  const {
    comments, loading, error, hasMore, replyState,
    loadInitial, loadMore, toggleReplies,
    addComment, addReply, updateComment, removeComment,
  } = postComments

  useEffect(() => {
    loadInitial()
  }, [loadInitial])

  async function handleCreate(content, parentCommentId) {
    const result = await createComment(content, parentCommentId)
    if (!result) return false
    if (parentCommentId) {
      addReply(parentCommentId, result)
      setReplyingTo(null)
    } else {
      addComment(result)
      // Solo las comentarios raíz cuentan para el post.
      onCountChange?.(1)
    }
    return true
  }

  async function handleUpdate(commentId, content) {
    const result = await updateCommentApi(commentId, content)
    if (result) {
      updateComment(result)
    }
  }

  async function handleDelete(comment) {
    const success = await deleteComment(comment.id)
    if (!success) return
    if (comment.parentCommentId) {
      // El backend enmascara las respuestas eliminadas: mantenerlas en el hilo.
      updateComment({ ...comment, content: DELETED_TEXT, isDeleted: true, isEdited: false })
    } else {
      // El backend oculta las raíces eliminadas del listado.
      removeComment(comment.id)
      onCountChange?.(-1)
    }
  }

  return (
    <section className="comments-section" aria-label="Comentarios">
      <CommentComposer
        onSubmit={(content) => handleCreate(content)}
        submitting={submitting}
        placeholder="Escribí un comentario..."
        submitLabel="Publicar comentario"
      />

      {error && <p className="comments-section__error">{error}</p>}
      {actionError && <p className="comments-section__error">{actionError}</p>}

      {loading && comments.length === 0 && <p className="comments-section__loading">Cargando comentarios...</p>}

      {!loading && comments.length === 0 && !error && (
        <p className="comments-section__empty">Sé el primero en comentar.</p>
      )}

      <div className="comments-section__list">
        {comments.map((comment) => (
          <div key={comment.id} className="comments-section__item">
            <CommentItem
              comment={comment}
              replies={replyState[comment.id]?.items ?? []}
              repliesLoading={replyState[comment.id]?.loading ?? false}
              repliesOpen={replyState[comment.id]?.open ?? false}
              repliesError={replyState[comment.id]?.error ?? false}
              submitting={submitting}
              onToggleReplies={toggleReplies}
              onReply={setReplyingTo}
              onEdit={handleUpdate}
              onDelete={handleDelete}
            />
            {replyingTo && (replyingTo.id === comment.id || replyingTo.parentCommentId === comment.id) && (
              <div className="comments-section__reply">
                <CommentComposer
                  replyTo={replyingTo}
                  onSubmit={(content) => handleCreate(content, comment.id)}
                  onCancel={() => setReplyingTo(null)}
                  submitting={submitting}
                  autoFocus
                  placeholder="Escribí tu respuesta..."
                  submitLabel="Responder"
                />
              </div>
            )}
          </div>
        ))}
      </div>

      {hasMore && (
        <button type="button" className="comments-section__load-more" onClick={loadMore} disabled={loading}>
          {loading ? 'Cargando...' : 'Cargar más comentarios'}
        </button>
      )}
    </section>
  )
}