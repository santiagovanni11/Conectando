import Icon from '../ui/Icon/Icon'

/**
 * Me gusta de un comentario.
 *
 * <para>
 * Va con el mismo corazón y el mismo nombre accesible que el de las
 * publicaciones, pero chico y sin número al lado cuando nadie lo puso. El
 * contador solo aparece a partir del primero: en un hilo con veinte
 * comentarios, veinte números al lado de cada uno se comen la pantalla y
 * compiten con el texto, que es lo que importa leer.
 *
 * <para>
 * Muestra su propio estado de carga y no uno global: si no, tocar en un
 * comentario bloquearía todos los demás botones de la pantalla mientras
 * llega la respuesta.
 */
export default function CommentLikeButton({ comment, busy, onToggle }) {
  const liked = Boolean(comment.likedByMe)
  const count = comment.likesCount ?? 0

  return (
    <span className={`comment__like${liked ? ' comment__like--active' : ''}`}>
      <button
        type="button"
        className="comment__action comment__like-button"
        onClick={() => onToggle?.(comment)}
        disabled={busy}
        aria-pressed={liked}
        aria-label={liked ? 'Quitar me gusta del comentario' : 'Me gusta del comentario'}
      >
        <Icon name="heart" size="sm" className="comment__like-icon" />
      </button>

      {count > 0 && (
        <span className="comment__like-count" aria-hidden="true">
          {count}
        </span>
      )}
    </span>
  )
}