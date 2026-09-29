import NotificationItem from '../components/notifications/NotificationItem'
import Button from '../components/ui/Button'
import EmptyState from '../components/ui/EmptyState'
import ErrorState from '../components/ui/ErrorState'
import Icon from '../components/ui/Icon/Icon'
import Loader from '../components/ui/Loader'
import { useNotifications } from '../hooks/useNotifications'

export default function NotificationsPage() {
  const {
    items, unreadCount, hasMore, loading, loadingMore, error,
    refresh, loadMore, markAsRead, markAllAsRead,
  } = useNotifications()

  return (
    <div className="notifications-page">
      <header className="notifications-page__header">
        <h1 className="notifications-page__title">Notificaciones</h1>
        {unreadCount > 0 && (
          <Button variant="ghost" size="sm" onClick={markAllAsRead}>
            Marcar todas como leídas
          </Button>
        )}
      </header>

      {loading && <Loader label="Cargando notificaciones…" />}

      {error && (
        <ErrorState title="No pudimos cargar tus notificaciones" description={error} onRetry={refresh} />
      )}

      {!loading && !error && items.length === 0 && (
        <EmptyState
          icon={<Icon name="bell" size="lg" />}
          title="No tenés notificaciones"
          description="Cuando alguien interactúe con vos, te avisamos acá."
        />
      )}

      {items.length > 0 && (
        <ul className="notifications-list">
          {items.map((item) => (
            <NotificationItem key={item.id} notification={item} onRead={markAsRead} />
          ))}
        </ul>
      )}

      {hasMore && (
        <div className="notifications-page__more">
          <Button variant="secondary" size="sm" loading={loadingMore} onClick={loadMore}>
            Cargar más
          </Button>
        </div>
      )}
    </div>
  )
}