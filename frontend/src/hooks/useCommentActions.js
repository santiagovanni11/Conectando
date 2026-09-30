import { useState, useCallback } from 'react'
import { commentService } from '../services/commentService'

export function useCommentActions(postId) {
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState(null)

  const createComment = useCallback(async (content, parentCommentId) => {
    setSubmitting(true)
    setError(null)
    try {
      const data = { content, parentCommentId }
      const result = await commentService.create(postId, data)
      return result
    } catch {
      setError('No se pudo publicar el comentario')
      return null
    } finally {
      setSubmitting(false)
    }
  }, [postId])

  const updateComment = useCallback(async (commentId, content) => {
    setSubmitting(true)
    setError(null)
    try {
      const result = await commentService.update(commentId, { content })
      return result
    } catch {
      setError('No se pudo editar el comentario')
      return null
    } finally {
      setSubmitting(false)
    }
  }, [])

  const deleteComment = useCallback(async (commentId) => {
    setSubmitting(true)
    setError(null)
    try {
      await commentService.delete(commentId)
      return true
    } catch {
      setError('No se pudo eliminar el comentario')
      return false
    } finally {
      setSubmitting(false)
    }
  }, [])

  return { submitting, error, createComment, updateComment, deleteComment }
}

/**
 * Me gusta de un comentario.
 *
 * <para>
 * Va en su propio hook y no agregado al de comentarios porque es otra cosa:
 * esas son acciones sobre el texto —crear, editar, borrar— y se firman con el
 * campo del compositor. Este no tiene campo, y mezclarlo haría que el estado
 * de "guardando" saltara al tocar un corazón.
 *
 * <para>
 * El id del comentario que se está tocando viaja en el estado para que solo
 * ese botón muestre la espera: bloquear toda la lista mientras llega la
 * respuesta se siente lento cuando hay veinte comentarios.
 * </para>
 */
export function useCommentLikes() {
  const [liking, setLiking] = useState(null)
  const [error, setError] = useState(null)

  const toggle = useCallback(async (comment) => {
    if (liking) return null
    setLiking(comment.id)
    setError(null)

    // El servidor devuelve el número final, así que se espera su respuesta en
    // vez de suponerlo. Si falla, el estado no se toca y el botón vuelve a su
    // posición anterior.
    try {
      const result = comment.likedByMe
        ? await commentService.unlike(comment.id)
        : await commentService.like(comment.id)

      return { likesCount: result.likesCount, likedByMe: result.likedByMe }
    } catch {
      setError('No se pudo guardar el me gusta')
      return null
    } finally {
      setLiking(null)
    }
  }, [liking])

  return { liking, error, toggle }
}