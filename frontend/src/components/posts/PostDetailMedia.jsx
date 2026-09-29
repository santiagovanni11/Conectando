import PostMediaGallery from './PostMediaGallery'

export default function PostDetailMedia({ post }) {
  return (
    <div className="post-detail__media">
      <PostMediaGallery media={post.media} label="Fotos de la publicación" />
    </div>
  )
}