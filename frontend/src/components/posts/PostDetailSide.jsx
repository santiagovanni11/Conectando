import { useState } from 'react'
import PostCardHeader from './PostCardHeader'
import PostCardContent from './PostCardContent'
import PostCardMeta from './PostCardMeta'
import PostActions from './PostActions'
import CommentsSection from '../comments/CommentsSection'
import LikeUsersModal from '../likes/LikeUsersModal'
import { useSharePost } from '../../hooks/useSharePost'

export default function PostDetailSide({ post, own, like, onEdit, onDeleted, onClose }) {
  const [showComments, setShowComments] = useState(true)
  const [showLikes, setShowLikes] = useState(false)
  const { share, message: shareMessage } = useSharePost()
  const [commentsCount, setCommentsCount] = useState(post.commentsCount ?? 0)

  function handleCommentsChange(delta) {
    setCommentsCount((current) => Math.max(0, current + delta))
  }

  return (
    <div className="post-detail__side">
      <PostCardHeader post={post} own={own} onEdit={onEdit} onDelete={onDeleted} />

      <div className="post-detail__side-body">
        <PostCardContent post={post} className="post-detail__content" />
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
      </div>

      {showLikes && <LikeUsersModal postId={post.id} onClose={() => setShowLikes(false)} />}

      <button type="button" className="post-detail__back" onClick={onClose}>
        Volver
      </button>
    </div>
  )
}