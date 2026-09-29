import { useEffect, useRef } from 'react'
import Button from '../ui/Button'

export default function FeedSentinel({ onLoadMore, disabled = false, loading = false }) {
  const sentinelRef = useRef(null)

  useEffect(() => {
    const node = sentinelRef.current
    if (!node || disabled) return undefined

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) onLoadMore()
      },
      { rootMargin: '240px' },
    )

    observer.observe(node)
    return () => observer.disconnect()
  }, [onLoadMore, disabled])

  if (disabled) return null

  return (
    <div className="feed__sentinel">
      <div ref={sentinelRef} className="feed__sentinel-marker" aria-hidden="true" />
      <Button variant="secondary" size="sm" onClick={onLoadMore} disabled={loading}>
        {loading ? 'Cargando…' : 'Cargar más'}
      </Button>
    </div>
  )
}
