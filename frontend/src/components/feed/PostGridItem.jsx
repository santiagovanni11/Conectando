import { Link } from 'react-router-dom'
import Icon from '../ui/Icon/Icon'
import { ROUTES } from '../../constants/routes'

function getCover(post) {
  return post.media?.[0]
}

export default function PostGridItem({ post }) {
  const cover = getCover(post)
  const mediaCount = post.media?.length ?? 0
  const isPhoto = Boolean(cover)

  return (
    <Link
      to={ROUTES.post(post.id)}
      className={`feed-grid__item${isPhoto ? '' : ' feed-grid__item--text'}`}
      aria-label="Ver publicación"
    >
      {isPhoto ? (
        <img
          className="feed-grid__thumb"
          src={cover.url}
          alt=""
          loading="lazy"
          draggable="false"
        />
      ) : (
        <span className="feed-grid__preview">
          <span className="feed-grid__preview-text">{post.content}</span>
        </span>
      )}

      {mediaCount > 1 && (
        <span className="feed-grid__badge feed-grid__badge--carousel" aria-hidden="true">
          <Icon name="image" size="sm" />
        </span>
      )}

      <span className="feed-grid__overlay" aria-hidden="true">
        <span className="feed-grid__stat">
          <Icon name="heart" size="sm" />
          {post.likesCount ?? 0}
        </span>
        <span className="feed-grid__stat">
          <Icon name="comment" size="sm" />
          {post.commentsCount ?? 0}
        </span>
      </span>
    </Link>
  )
}