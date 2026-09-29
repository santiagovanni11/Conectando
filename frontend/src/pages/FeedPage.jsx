import { useAuth } from '../hooks/useAuth'
import { useFeed } from '../hooks/useFeed'
import EmptyState from '../components/ui/EmptyState'
import ErrorState from '../components/ui/ErrorState'
import Icon from '../components/ui/Icon/Icon'
import PostComposerModal from '../components/posts/PostComposerModal'
import FeedList from '../components/feed/FeedList'
import FeedSentinel from '../components/feed/FeedSentinel'
import FeedSkeleton from '../components/feed/FeedSkeleton'
import FeedSearch from '../components/feed/FeedSearch'

export default function FeedPage() {
  const { user } = useAuth()
  const { items, hasMore, phase, error, loadMore, refresh } = useFeed()

  const showSkeleton = phase === 'loading'
  const showError = phase === 'error'
  const isEmpty = phase === 'loaded' && items.length === 0
  const showList = items.length > 0
  const endReached = showList && !hasMore && phase === 'loaded'
  const loadMoreFailed = showList && phase === 'loaded' && Boolean(error)

  return (
    <div className="home feed">
      <header className="home__header">
        <div className="home__header-left">
          <h1>Hola, {user?.displayName ?? user?.userName}</h1>
          <p className="home__subtitle">Esto es lo que está pasando.</p>
        </div>

        <FeedSearch />
      </header>

      <PostComposerModal onPublished={refresh} />

      {showSkeleton && <FeedSkeleton />}

      {showError && (
        <ErrorState
          title="No pudimos cargar tu feed"
          description={error}
          onRetry={refresh}
        />
      )}

      {isEmpty && (
        <EmptyState
          icon={<Icon name="home" size="lg" />}
          title="Tu feed está vacío"
          description="Publicá algo o conectate con otras personas para ver novedades aquí."
        />
      )}

      {showList && <FeedList items={items} />}

      {phase === 'loading-more' && (
        <p className="feed__status" role="status">
          Cargando más publicaciones…
        </p>
      )}

      {showList && hasMore && (
        <FeedSentinel
          onLoadMore={loadMore}
          disabled={phase === 'loading-more'}
          loading={phase === 'loading-more'}
        />
      )}

      {loadMoreFailed && (
        <ErrorState
          title="No pudimos cargar más publicaciones"
          description={error}
          onRetry={loadMore}
          className="feed__inline-error"
        />
      )}

      {endReached && <p className="feed__status">Estás al día ✨</p>}
    </div>
  )
}