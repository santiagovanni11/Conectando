import PostComposerModal from './PostComposerModal'
import UserPostGrid from './UserPostGrid'
import { useUserPosts } from '../../hooks/useUserPosts'

export default function UserPostsSection({ userId, isOwn = false }) {
  const { items, hasMore, phase, error, loadMore, prepend, refresh } = useUserPosts(userId)

  return (
    <section className="user-posts" aria-label="Publicaciones">
      {isOwn && <PostComposerModal onPublished={prepend} />}

      <UserPostGrid
        items={items}
        hasMore={hasMore}
        phase={phase}
        error={error}
        isOwn={isOwn}
        onLoadMore={loadMore}
        onRetry={refresh}
      />
    </section>
  )
}