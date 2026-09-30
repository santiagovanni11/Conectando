import { useCallback, useState } from 'react'
import Modal from '../ui/Modal'
import PostComposerForm from './PostComposerForm'
import PostComposerTrigger from './PostComposerTrigger'
import { useAuth } from '../../hooks/useAuth'

/**
 * Publicar: el disparador del feed y el modal donde se escribe.
 *
 * Arma el `Modal` de la casa en vez de uno propio. Antes llevaba clases
 * `composer-modal__*` que no existían en ningún archivo de estilos —solo
 * había reglas de móvil— así que el panel salía como un div común en el
 * flujo de la página: sin fondo, sin overlay, sin sombra y sin centrado, en
 * cualquier pantalla. Reusar el primitivo le trae de una el overlay, el
 * centrado, el cierre con Escape y con el fondo, el bloqueo del scroll de
 * fondo, las medidas que se adaptan al móvil y el espacio seguro del notch.
 *
 * `--tall` porque el compositor es más alto que un modal normal: lleva área
 * de texto, grilla de fotos y pie. En celular ocupa toda la pantalla, que es
 * lo que hacen las demás apps, en vez de una hoja abajo con el contenido
 * apretado contra el teclado.
 */
export default function PostComposerModal({ onPublished }) {
  const { user } = useAuth()
  const [isOpen, setIsOpen] = useState(false)

  const close = useCallback(() => setIsOpen(false), [])

  function handlePublished(post) {
    setIsOpen(false)
    onPublished?.(post)
  }

  const displayName = user?.displayName ?? user?.userName ?? ''

  return (
    <>
      <PostComposerTrigger
        displayName={displayName}
        avatarSrc={user?.profileImageUrl}
        onClick={() => setIsOpen(true)}
      />

      {isOpen && (
        <Modal title="Crear publicación" onClose={close} className="modal__panel--tall">
          <PostComposerForm
            avatarName={user?.displayName}
            avatarSrc={user?.profileImageUrl}
            onPublished={handlePublished}
            onCancel={close}
            autoFocus
          />
        </Modal>
      )}
    </>
  )
}
