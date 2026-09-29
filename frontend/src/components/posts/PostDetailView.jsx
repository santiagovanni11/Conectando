import { useState } from 'react'
import { useAuth } from '../../hooks/useAuth'
import { usePostLike } from '../../hooks/usePostLike'
import PostDetailMedia from './PostDetailMedia'
import PostDetailSide from './PostDetailSide'
import PostEditForm from './PostEditForm'

export default function PostDetailView({ post: initialPost, onClose }) {
  const { user } = useAuth()
  const [post, setPost] = useState(initialPost)
  const [editing, setEditing] = useState(false)
  const like = usePostLike(post)
  const own = post.author.id === user?.id
  const hasMedia = Boolean(post.media && post.media.length > 0)

  if (editing) {
    return (
      <div className="post-detail">
        <PostEditForm
          post={post}
          onCancel={() => setEditing(false)}
          onSaved={(updated) => {
            setPost(updated)
            setEditing(false)
          }}
        />
      </div>
    )
  }

  return (
    <div className={`post-detail${hasMedia ? '' : ' post-detail--text'}`}>
      <div className="post-detail__grid">
        {hasMedia && <PostDetailMedia post={post} />}
        <PostDetailSide
          post={post}
          own={own}
          like={like}
          onEdit={() => setEditing(true)}
          onDeleted={onClose}
          onClose={onClose}
        />
      </div>
    </div>
  )
}