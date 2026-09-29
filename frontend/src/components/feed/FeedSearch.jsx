import Icon from '../ui/Icon/Icon'
import Button from '../ui/Button'
import UserCard from '../social/UserCard'
import { useSearch } from '../../hooks/useSearch'

export default function FeedSearch() {
  const {
    results, loading, error, query, setQuery, hasQuery, isSearchOpen, setIsSearchOpen, clear, sendRequest,
  } = useSearch()

  function close() {
    setIsSearchOpen(false)
    clear()
  }

  return (
    <div className="home__header-right">
      <Button
        variant="ghost"
        size="sm"
        onClick={() => setIsSearchOpen(true)}
        aria-label="Buscar personas"
        className="home__search-button"
      >
        <Icon name="search" size="sm" />
        <span>Buscar</span>
      </Button>

      {isSearchOpen && (
        <div className="home__search-overlay">
          <div className="home__search-box">
            <input
              type="text"
              placeholder="Buscá por nombre o usuario..."
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              disabled={loading}
              aria-label="Buscar personas"
              autoFocus
              autoComplete="off"
              className="home__search-input"
            />
            {loading && <span className="home__search-loading">Buscando…</span>}
            {error && <p className="home__search-error">{error}</p>}
            <button
              type="button"
              onClick={close}
              className="home__search-close"
              aria-label="Cerrar búsqueda"
            >
              <Icon name="close" size="sm" />
            </button>
          </div>

          {!loading && !error && query.trim() !== '' && !hasQuery && (
            <p className="home__search-empty">Escribí al menos 2 letras.</p>
          )}

          {!loading && !error && hasQuery && results.length === 0 && (
            <p className="home__search-empty">No encontramos a nadie con ese nombre.</p>
          )}

          {results.length > 0 && (
            <ul className="user-card-list home__search-results">
              {results.map((result) => (
                <UserCard
                  key={result.id}
                  user={result}
                  onSendRequest={sendRequest}
                  onOpen={close}
                />
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  )
}