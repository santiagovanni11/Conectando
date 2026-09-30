import { useState } from 'react'
import MessageThreadRow from './MessageThreadRow'

/**
 * Lista del hilo.
 *
 * La edición es en línea, como en WhatsApp: el mensaje se convierte en un
 * campo de texto y se confirma al guardar o al cancelar. El estado de la
 * edición es de la lista y no de la fila, porque solo puede haber una
 * edición abierta a la vez en todo el hilo.
 */
export default function MessageThread({
  messages,
  currentUserId,
  isLoading,
  isLoadingMore,
  hasMore,
  onLoadMore,
  onEdit,
  onDelete,
  onReply,
  actionError,
  error,
}) {
  const [editingId, setEditingId] = useState(null)
  const [draft, setDraft] = useState('')

  const startEditing = (message) => {
    setEditingId(message.id)
    setDraft(message.content)
  }

  const saveEdit = async (messageId) => {
    // Si el servidor rechaza —pasada la ventana de 15 minutos, o sin
    // permiso— el formulario sigue abierto: así el texto escrito no se
    // pierde y el error queda a la vista, junto a la edición.
    const saved = await onEdit?.(messageId, draft)
    if (saved === false) return

    setEditingId(null)
    setDraft('')
  }

  if (isLoading) {
    return <p className="message-thread__status">Cargando mensajes…</p>
  }

  return (
    <>
      {/*
        Un fallo de carga NO es lo mismo que un chat sin mensajes. Mostrar
        "Decí hola" cuando la consulta falló hace creer que la conversación
        está vacía, y fue exactamente lo que pasó: la migración faltante
        daba 500 y el chat aparecía sin historial, con los datos intactos en
        la base.

        El aviso va solo cuando no hay nada que mostrar. Si el hilo ya está
        en pantalla, un fallo de sincronización no puede taparlo ni agregar
        ruido: el usuario tiene lo que fue a buscar.
      */}
      {error && messages.length === 0 && (
        <p className="message-thread__error" role="alert">
          No se pudieron cargar los mensajes: {error.message ?? 'error desconocido'}.
        </p>
      )}

      {/* "Chat vacío" y "no se pudo cargar" son excluyentes: con error, el
          aviso de carga manda y el otro no aparece, o se leen como dos
          verdades que no pueden ser ciertas a la vez. */}
      {!error && messages.length === 0 && (
        <p className="message-thread__status">Todavía no hay mensajes. Decí hola.</p>
      )}

      {messages.length > 0 && (
        <>
          {hasMore && (
            <button
              type="button"
              className="message-thread__more"
              onClick={onLoadMore}
              disabled={isLoadingMore}
            >
              {isLoadingMore ? 'Cargando…' : 'Cargar mensajes anteriores'}
            </button>
          )}

          {/* Cada mensaje es su propio <li>, y lo aporta MessageThreadRow:
              por eso el <ul> queda en columna sin anidar. */}
          <ul className="message-thread">
            {messages.map((message) => (
              <MessageThreadRow
                key={message.id}
                message={message}
                isOwn={message.sender?.id === currentUserId}
                isEditing={editingId === message.id}
                error={editingId === message.id ? actionError : ''}
                draft={draft}
                onDraftChange={setDraft}
                onStartEdit={() => startEditing(message)}
                onCancelEdit={() => setEditingId(null)}
                onSaveEdit={() => saveEdit(message.id)}
                onDelete={onDelete}
                onReply={onReply}
              />
            ))}
          </ul>
        </>
      )}
    </>
  )
}
