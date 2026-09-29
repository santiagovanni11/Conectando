import { useState } from 'react'
import PostActions from './PostActions'
import PostCardContent from './PostCardContent'
import PostCardHeader from './PostCardHeader'
import PostCardMeta from './PostCardMeta'
import PostEditForm from './PostEditForm'
import PostMediaGallery from './PostMediaGallery'
import CommentsSection from '../comments/CommentsSection'
import LikeUsersModal from '../likes/LikeUsersModal'
import { postService } from '../../services/postService'
import { useSharePost } from '../../hooks/useSharePost'
import { usePostLike } from '../../hooks/usePostLike'
import { usePostSave } from '../../hooks/usePostSave'
import { postVariant } from '../../utils/postVariant'

export default function PostCard({ post, own = false, onEdited, onDeleted }) {
  const like = usePostLike(post)
  const save = usePostSave(post)
  const { share, message: shareMessage } = useSharePost()
  const [editing, setEditing] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const [removed, setRemoved] = useState(false)
  const [showComments, setShowComments] = useState(false)
  const [showLikes, setShowLikes] = useState(false)
  // El conteo arranca en el post y se actualiza al comentar o borrar.
  const [commentsCount, setCommentsCount] = useState(post.commentsCount ?? 0)
  // Con fotos la publicación se muestra de otra manera: la imagen manda y el
  // resto se acomoda alrededor en vez de competir con ella.
  const variant = postVariant(post)

  function handleCommentsChange(delta) {
    setCommentsCount((current) => Math.max(0, current + delta))
  }

  if (removed) return null

  async function handleDelete() {
    if (deleting) return
    setDeleting(true)
    try {
      await postService.deletePost(post.id)
      setRemoved(true)
      onDeleted?.(post.id)
    } catch {
      setDeleting(false)
    }
  }

  return (
    <article className={`post-card post-card--${variant}`}>
      <PostCardHeader post={post} own={own} onEdit={() => setEditing(true)} onDelete={handleDelete} />

      <PostMediaGallery media={post.media} onDoubleClick={like.toggle} />

      {editing ? (
        <PostEditForm post={post} onCancel={() => setEditing(false)} onSaved={onEdited} />
      ) : (
        <>
          <PostCardContent post={post} />
          <PostActions
            liked={like.liked}
            count={like.count}
            pending={like.pending}
            onToggleLike={like.toggle}
            commentsCount={commentsCount}
            showComments={showComments}
            onToggleComments={() => setShowComments((open) => !open)}
            onOpenLikes={() => setShowLikes(true)}
            onShare={() => share(post.id)}
            shareMessage={shareMessage}
            saved={save.saved}
            savePending={save.pending}
            onToggleSave={save.toggle}
          />
          <PostCardMeta
            likedCount={like.count}
            commentsCount={commentsCount}
            onOpenLikes={() => setShowLikes(true)}
            onShowComments={() => setShowComments(true)}
          />
          {showComments && (
            <CommentsSection postId={post.id} onCountChange={handleCommentsChange} />
          )}
        </>
      )}

      {showLikes && <LikeUsersModal postId={post.id} onClose={() => setShowLikes(false)} />}
    </article>
  )
}