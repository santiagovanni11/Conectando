function SkeletonCard() {
  return (
    <div className="feed-skeleton__card" aria-hidden="true">
      <div className="feed-skeleton__row">
        <div className="feed-skeleton__avatar" />
        <div className="feed-skeleton__lines">
          <div className="feed-skeleton__line feed-skeleton__line--name" />
          <div className="feed-skeleton__line feed-skeleton__line--sub" />
        </div>
      </div>
      <div className="feed-skeleton__line feed-skeleton__line--body" />
      <div className="feed-skeleton__line feed-skeleton__line--body feed-skeleton__line--short" />
    </div>
  )
}

export default function FeedSkeleton({ cards = 3 }) {
  return (
    <div className="feed-skeleton" role="status" aria-label="Cargando publicaciones">
      {Array.from({ length: cards }, (_, index) => (
        <SkeletonCard key={index} />
      ))}
    </div>
  )
}
