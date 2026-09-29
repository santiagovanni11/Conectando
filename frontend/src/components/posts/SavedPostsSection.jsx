import Button from '../ui/Button'
import EmptyState from '../ui/EmptyState'
import ErrorState from '../ui/ErrorState'
import Icon from '../ui/Icon/Icon'
import Loader from '../ui/Loader'
import PostGridItem from '../feed/PostGridItem'
import { useSavedPosts } from '../../hooks/useSavedPosts'

/**
 * Apartado "Publicaciones guardadas" del perfil propio.
 *
 * Muestra lo que el usuario guardó, no lo que publicó. Solo lo ve él: el
 * endpoint filtra por sesión, así que no hay nada que ocultar en la interfaz.
 */
export default function SavedPostsSection() {
  const { items, hasMore, phase, error, loadMore, refresh } = useSavedPosts()

  if (phase === 'loading') return <Loader label="Cargando guardados…" />

  if (phase === 'error') return <ErrorState description={error} onRetry={refresh} />

  if (items.length === 0) {
    return (
      <EmptyState
        icon={<Icon name="bookmark" size="lg" />}
        title="Todavía no guardaste nada"
        description="Tapá el marcador de una publicación para encontrarla acá."
      />
    )
  }

  return (
    <section className="saved-posts" aria-label="Publicaciones guardadas">
      <div className="feed-grid">
        {items.map((post) => (
          <PostGridItem key={post.id} post={post} own={false} />
        ))}
      </div>

      {hasMore && (
        <div className="user-posts__more">
          <Button variant="secondary" size="sm" loading={phase === 'loading-more'} onClick={loadMore}>
            Ver más
          </Button>
        </div>
      )}
    </section>
  )
}