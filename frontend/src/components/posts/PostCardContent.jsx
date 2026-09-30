import { useState } from 'react'
import UserHandle from '../users/UserHandle'

const CAPTION_LIMIT = 125

export default function PostCardContent({ post, className = '' }) {
  const [expanded, setExpanded] = useState(false)
  const content = post.content?.trim() ?? ''
  const isLong = content.length > CAPTION_LIMIT
  const shown = expanded || !isLong ? content : `${content.slice(0, CAPTION_LIMIT - 2).trimEnd()}…`

  if (!content) return null

  return (
    <p className={`post-card__content ${className}`.trim()}>
      <UserHandle user={post.author} className="post-card__caption-username" />
      {' '}
      <span className="post-card__caption-text">{shown}</span>
      {isLong && !expanded && (
        <button type="button" className="post-card__more" onClick={() => setExpanded(true)}>
          {' '}más
        </button>
      )}
    </p>
  )
}