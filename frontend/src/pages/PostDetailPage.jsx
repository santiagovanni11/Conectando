import { useNavigate, useParams } from 'react-router-dom'
import { usePost } from '../hooks/usePost'
import { usePostMeta } from '../hooks/usePostMeta'
import PostDetailView from '../components/posts/PostDetailView'
import Loader from '../components/ui/Loader'
import ErrorState from '../components/ui/ErrorState'
import EmptyState from '../components/ui/EmptyState'
import Button from '../components/ui/Button'
import Icon from '../components/ui/Icon/Icon'
import { ROUTES } from '../constants/routes'

export default function PostDetailPage() {
  const { postId } = useParams()
  const navigate = useNavigate()
  const { post, error, notFound, status, reload } = usePost(postId)

  usePostMeta(post)

  function goHome() {
    navigate(ROUTES.home)
  }

  if (notFound) {
    return (
      <div className="post-detail-page">
        <EmptyState
          icon={<Icon name="image" size="lg" />}
          title="Publicación no encontrada"
          description="Este post no existe o no tenés permiso para verlo."
        >
          <Button variant="secondary" size="sm" onClick={goHome}>Volver al inicio</Button>
        </EmptyState>
      </div>
    )
  }

  if (status === 'error') {
    return (
      <div className="post-detail-page">
        <ErrorState
          title="No pudimos cargar la publicación"
          description={error}
          onRetry={reload}
        />
      </div>
    )
  }

  if (status === 'loading' || !post) {
    return (
      <div className="post-detail-page">
        <Loader label="Cargando publicación…" />
      </div>
    )
  }

  return <PostDetailView post={post} onClose={() => navigate(-1)} />
}