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