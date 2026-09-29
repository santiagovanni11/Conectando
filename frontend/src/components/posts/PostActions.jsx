import Icon from '../ui/Icon/Icon'

export default function PostActions({
  liked,
  count,
  pending,
  onToggleLike,
  commentsCount,
  showComments,
  onToggleComments,
  onOpenLikes,
  onShare,
  shareMessage,
  saved,
  savePending,
  onToggleSave,
}) {
  return (
    <div className="post-actions" aria-label="Acciones de la publicación">
      <button
        type="button"
        className={`post-action post-action--like${liked ? ' post-action--active' : ''}${pending ? ' post-action--pending' : ''}`}
        onClick={onToggleLike}
        disabled={pending}
        aria-pressed={liked}
        aria-label={liked ? 'Quitar me gusta' : 'Me gusta'}
      >
        <Icon name="heart" size="md" className="post-action__icon" />
      </button>

      {count > 0 && (
        <button
          type="button"
          className="post-action post-action--count"
          onClick={onOpenLikes}
          aria-label={`Ver ${count} personas que dieron me gusta`}
        >
          {count}
        </button>
      )}

      <button
        type="button"
        className={`post-action post-action--comment${showComments ? ' post-action--active' : ''}`}
        onClick={onToggleComments}
        aria-pressed={showComments}
        aria-label={showComments ? 'Ocultar comentarios' : commentsCount > 0 ? `Ver comentarios (${commentsCount})` : 'Comentar'}
      >
        <Icon name="comment" size="md" className="post-action__icon" />
        {commentsCount > 0 && (
          <span className="post-action__number" aria-hidden="true">
            {commentsCount}
          </span>
        )}
      </button>

      {onShare && (
        <button
          type="button"
          className="post-action post-action--share"
          onClick={onShare}
          aria-label="Compartir publicación"
        >
          <Icon name="share" size="md" className="post-action__icon" />
        </button>
      )}

      {/* Guardar es personal: no avisa al autor, como el marcador de
          Instagram. Por eso va suelto, fuera del menú de "Compartir". */}
      {onToggleSave && (
        <button
          type="button"
          className={`post-action post-action--save${saved ? ' post-action--active' : ''}`}
          onClick={onToggleSave}
          disabled={savePending}
          aria-pressed={saved}
          aria-label={saved ? 'Quitar de guardados' : 'Guardar publicación'}
        >
          <Icon
            name="bookmark"
            size="md"
            className={`post-action__icon${saved ? ' is-filled' : ''}`}
          />
        </button>
      )}

      {shareMessage && (
        <span className="post-actions__feedback" role="status">
          {shareMessage}
        </span>
      )}
    </div>
  )
}