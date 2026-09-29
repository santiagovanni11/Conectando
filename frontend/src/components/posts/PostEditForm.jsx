import { useAuth } from '../../hooks/useAuth'
import PostComposerForm from './PostComposerForm'

export default function PostEditForm({ post, onCancel, onSaved }) {
  const { user } = useAuth()

  return (
    <PostComposerForm
      initialPost={post}
      avatarName={user?.displayName}
      avatarSrc={user?.profileImageUrl}
      onCancel={onCancel}
      onPublished={onSaved}
      autoFocus
    />
  )
}