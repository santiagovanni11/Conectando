import Button from '../ui/Button'
import EmptyState from '../ui/EmptyState'
import ErrorState from '../ui/ErrorState'
import Icon from '../ui/Icon/Icon'
import Loader from '../ui/Loader'
import PostGridItem from '../feed/PostGridItem'

export default function UserPostGrid({ items, hasMore, phase, error, isOwn, onLoadMore, onRetry }) {
  if (phase === 'loading') return <Loader label="Cargando publicaciones…" />

  if (phase === 'error') return <ErrorState description={error} onRetry={onRetry} />

  if (items.length === 0) {
    return (
      <EmptyState
        icon={<Icon name="image" size="lg" />}
        title={isOwn ? 'Todavía no publicaste nada' : 'Sin publicaciones'}
        description={isOwn ? 'Contale al mundo qué estás tramando.' : 'Esta persona todavía no publicó nada.'}
      />
    )
  }

  return (
    <>
      <div className="feed-grid">
        {items.map((post) => (
          <PostGridItem key={post.id} post={post} own={isOwn} />
        ))}
      </div>

      {hasMore && (
        <div className="user-posts__more">
          <Button variant="secondary" size="sm" loading={phase === 'loading-more'} onClick={onLoadMore}>
            Ver más
          </Button>
        </div>
      )}
    </>
  )
}