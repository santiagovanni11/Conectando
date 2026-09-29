import { useEffect } from 'react'
import { usePostLikes } from '../../hooks/usePostLikes'
import LikeUserItem from './LikeUserItem'

export default function LikeUsersModal({ postId, onClose }) {
  const { users, loading, error, hasMore, loadInitial, loadMore } = usePostLikes(postId)

  useEffect(() => {
    loadInitial()
  }, [loadInitial])

  useEffect(() => {
    function handleEscape(event) {
      if (event.key === 'Escape') onClose()
    }
    document.addEventListener('keydown', handleEscape)
    return () => document.removeEventListener('keydown', handleEscape)
  }, [onClose])

  return (
    <div className="likes-modal" role="dialog" aria-modal="true" aria-labelledby="likes-modal-title">
      <div className="likes-modal__backdrop" onClick={onClose} />
      <div className="likes-modal__panel">
        <div className="likes-modal__header">
          <h2 id="likes-modal-title" className="likes-modal__title">Me gusta</h2>
          <button
            type="button"
            className="likes-modal__close"
            onClick={onClose}
            aria-label="Cerrar"
          >
            ✕
          </button>
        </div>
        <div className="likes-modal__body">
          {error && <p className="likes-modal__error">{error}</p>}
          {loading && users.length === 0 && <p className="likes-modal__loading">Cargando...</p>}
          {!loading && users.length === 0 && !error && (
            <p className="likes-modal__empty">Aún no hay me gusta</p>
          )}
          <ul className="likes-modal__list">
            {users.map((user) => (
              <LikeUserItem key={user.id} user={user} />
            ))}
          </ul>
          {hasMore && (
            <button
              type="button"
              className="likes-modal__more"
              onClick={loadMore}
              disabled={loading}
            >
              {loading ? 'Cargando...' : 'Cargar más'}
            </button>
          )}
        </div>
      </div>
    </div>
  )
}