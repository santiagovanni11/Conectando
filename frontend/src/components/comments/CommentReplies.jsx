import CommentItem from './CommentItem'
import Loader from '../ui/Loader'

/**
 * Las respuestas de un comentario, desplegadas.
 *
 * Va aparte porque es lo único de `CommentItem` que tiene estado propio —se
 * abre, se cierra y se recarga— y mezclarlo hacía que el comentario creciera
 * con cada cosa que se le agregara. Además se usa recursivamente: es el mismo
 * componente en la raíz y en cada respuesta.
 */
export default function CommentReplies({
  replies,
  loading,
  error,
  submitting,
  liking,
  onToggle,
  onToggleLike,
  onEdit,
  onDelete,
}) {
  return (
    <div className="comment__replies">
      {loading && replies.length === 0 ? (
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
              liking={liking}
              onToggleLike={onToggleLike}
              onEdit={onEdit}
              onDelete={onDelete}
              onToggleReplies={onToggle}
            />
          </div>
        ))
      )}

      {error && <p className="comment__replies-error">No se pudieron cargar las respuestas.</p>}

      {!loading && !error && replies.length === 0 && (
        <p className="comment__replies-empty">Aún no hay respuestas.</p>
      )}
    </div>
  )
}