import { useAuth } from '../../hooks/useAuth'
import PostCard from '../posts/PostCard'

export default function FeedList({ items, onEdited, onDeleted }) {
  const { user } = useAuth()

  return (
    <div className="feed__list">
      {items.map((post) => (
        <PostCard
          key={post.id}
          post={post}
          own={post.author.id === user?.id}
          onEdited={onEdited}
          onDeleted={onDeleted}
        />
      ))}
    </div>
  )
}
