import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import Avatar from '../ui/Avatar'
import IconButton from '../ui/IconButton'
import UserHandle from '../users/UserHandle'
import { formatRelativeTime } from '../../utils/dateFormatter'
import { ROUTES } from '../../constants/routes'

export default function PostCardHeader({ post, own = false, onEdit, onDelete }) {
  const [menuOpen, setMenuOpen] = useState(false)
  const menuRef = useRef(null)

  function closeMenu() {
    setMenuOpen(false)
  }

  useEffect(() => {
    if (!menuOpen) return

    function handlePointerDown(event) {
      if (!menuRef.current?.contains(event.target)) {
        closeMenu()
      }
    }

    function handleKeyDown(event) {
      if (event.key === 'Escape') {
        closeMenu()
      }
    }

    document.addEventListener('pointerdown', handlePointerDown)
    document.addEventListener('keydown', handleKeyDown)

    return () => {
      document.removeEventListener('pointerdown', handlePointerDown)
      document.removeEventListener('keydown', handleKeyDown)
    }
  }, [menuOpen])

  return (
    <div className="post-card__header">
      <Link
        to={ROUTES.user(post.author.id)}
        className="post-card__author"
        aria-label={`Ver el perfil de ${post.author.displayName}`}
      >
        <Avatar
          name={post.author.displayName}
          src={post.author.profileImageUrl}
          alt={post.author.displayName}
          size="sm"
        />
        <span className="post-card__identity">
          {/* El nombre lo decide UserHandle: si la cuenta fue dada de baja,
              muestra la etiqueta y no el identificador que quedó guardado. */}
          <UserHandle user={post.author} className="post-card__name" />
          <span className="post-card__sub">{formatRelativeTime(post.createdAt)}</span>
        </span>
      </Link>

      {own && (
        <div className="post-card__menu" ref={menuRef}>
          <IconButton name="menu" size="sm" label="Opciones de la publicación" onClick={() => setMenuOpen((open) => !open)} />
          {menuOpen && (
            <div className="post-card__menu-popover" role="menu">
              <button type="button" role="menuitem" className="post-card__menu-item" onClick={() => { closeMenu(); onEdit?.() }}>Editar</button>
              <button type="button" role="menuitem" className="post-card__menu-item post-card__menu-item--danger" onClick={() => { closeMenu(); onDelete?.() }}>Eliminar</button>
            </div>
          )}
        </div>
      )}
    </div>
  )
}