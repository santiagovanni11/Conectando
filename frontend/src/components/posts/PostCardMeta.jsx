function commentsLabel(count) {
  if (count === 0) return 'Sin comentarios'
  if (count === 1) return '1 comentario'
  return `${count} comentarios`
}

export default function PostCardMeta({ likedCount, commentsCount, onOpenLikes, onShowComments }) {
  return (
    <div className="post-card__meta">
      {likedCount > 0 && (
        <button type="button" className="post-card__likes" onClick={onOpenLikes}>
          {likedCount === 1 ? '1 me gusta' : `${likedCount} me gusta`}
        </button>
      )}

      <button
        type="button"
        className="post-card__comments-link"
        onClick={onShowComments}
        disabled={commentsCount === 0}
      >
        {commentsLabel(commentsCount)}
      </button>
    </div>
  )
}