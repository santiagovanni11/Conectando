import { useEffect, useState } from 'react'
import PostComposerForm from './PostComposerForm'
import IconButton from '../ui/IconButton'
import { useAuth } from '../../hooks/useAuth'

export default function PostComposerModal({ onPublished }) {
  const { user } = useAuth()
  const [isOpen, setIsOpen] = useState(false)

  // El modal es dueño de su estado: cerrar siempre es local.
  function handleClose() {
    setIsOpen(false)
  }

  useEffect(() => {
    if (!isOpen) return undefined

    function handleKeyDown(event) {
      if (event.key === 'Escape') setIsOpen(false)
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [isOpen])

  function handlePublished(post) {
    setIsOpen(false)
    onPublished?.(post)
  }

  return (
    <>
      <button
        type="button"
        className="composer-trigger"
        onClick={() => setIsOpen(true)}
        aria-label="Crear una nueva publicación"
      >
        <span className="composer-trigger__prompt">
          ¿Qué estás pensando, {user?.displayName ?? user?.userName}?
        </span>
      </button>

      {isOpen && <PostComposerModalBody onPublished={handlePublished} onClose={handleClose} />}
    </>
  )
}

function PostComposerModalBody({ onPublished, onClose }) {
  const { user } = useAuth()

  useEffect(() => {
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => {
      document.body.style.overflow = previousOverflow
    }
  }, [])

  return (
    <div className="composer-modal" role="dialog" aria-modal="true" aria-label="Crear publicación">
      <div className="composer-modal__backdrop" onClick={onClose} />

      <div className="composer-modal__panel">
        <header className="composer-modal__header">
          <h2 className="composer-modal__title">Crear publicación</h2>
          <IconButton name="close" label="Cerrar" onClick={onClose} />
        </header>

        <PostComposerForm
          avatarName={user?.displayName}
          avatarSrc={user?.profileImageUrl}
          onPublished={onPublished}
          onCancel={onClose}
          autoFocus
        />
      </div>
    </div>
  )
}