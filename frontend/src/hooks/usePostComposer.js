import { useState } from 'react'
import { postService } from '../services/postService'
import { MAX_POST_MEDIA } from '../constants/post'

const MAX_FILE_BYTES = 8 * 1024 * 1024
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif']

export function usePostComposer(initialPost = null) {
  const initialMedia = initialPost?.media ?? []
  const [content, setContent] = useState(initialPost?.content ?? '')
  const [privacy, setPrivacy] = useState(initialPost?.privacy ?? 'Public')
  const [drafts, setDrafts] = useState(() =>
    initialMedia.map((media, index) => ({ id: media.id, url: media.url, order: index, existing: true })),
  )
  const [uploading, setUploading] = useState(false)
  const [publishing, setPublishing] = useState(false)
  const [error, setError] = useState('')

  const isEdit = Boolean(initialPost)
  const canPublish = Boolean(content.trim() || drafts.length)

  function selectFiles(fileList) {
    const files = Array.from(fileList ?? [])
    if (files.length === 0) return

    if (files.length + drafts.length > MAX_POST_MEDIA) {
      setError(`No podés publicar más de ${MAX_POST_MEDIA} fotos.`)
      return
    }

    const invalid = files.find((file) => file.size > MAX_FILE_BYTES || !ALLOWED_TYPES.includes(file.type))
    if (invalid) {
      setError('Solo imágenes JPG, PNG, WEBP o GIF de hasta 8 MB.')
      return
    }

    setError('')
    upload(files)
  }

  async function upload(files) {
    setUploading(true)
    try {
      const uploaded = await postService.uploadMedia(files)
      setDrafts((prev) => [
        ...prev,
        ...uploaded.map((media, index) => ({ id: media.id, url: media.url, order: prev.length + index, existing: false })),
      ])
    } catch (err) {
      setError(err.message)
    } finally {
      setUploading(false)
    }
  }

  async function removeDraft(id) {
    const draft = drafts.find((item) => item.id === id)
    if (draft && !draft.existing) {
      await postService.deleteMedia(id).catch(() => {})
    }
    setDrafts((prev) => prev.filter((item) => item.id !== id).map((item, index) => ({ ...item, order: index })))
  }

  function moveDraft(id, delta) {
    setDrafts((prev) => {
      const index = prev.findIndex((draft) => draft.id === id)
      const target = index + delta
      if (index < 0 || target < 0 || target >= prev.length) return prev

      const next = [...prev]
      const [item] = next.splice(index, 1)
      next.splice(target, 0, item)
      return next.map((draft, position) => ({ ...draft, order: position }))
    })
  }

  async function publish() {
    if (!canPublish || publishing) return null
    setPublishing(true)
    setError('')

    try {
      const ordered = [...drafts].sort((a, b) => a.order - b.order)
      const mediaIds = ordered.map((draft) => draft.id)
      const payload = {
        content: content.trim() || null,
        privacy,
        mediaIds,
      }

      const result = isEdit
        ? await postService.updatePost(initialPost.id, payload)
        : await postService.createPost(payload)

      if (!isEdit) reset()
      return result
    } catch (err) {
      setError(err.message)
      return null
    } finally {
      setPublishing(false)
    }
  }

  function reset() {
    setContent('')
    setPrivacy('Public')
    setDrafts([])
    setError('')
  }

  return {
    content,
    setContent,
    privacy,
    setPrivacy,
    drafts,
    uploading,
    publishing,
    error,
    canPublish,
    selectFiles,
    removeDraft,
    moveDraft,
    publish,
    reset,
  }
}