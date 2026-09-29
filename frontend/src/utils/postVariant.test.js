import { describe, it, expect } from 'vitest'
import { POST_VARIANT, postVariant } from './postVariant'

describe('postVariant', () => {
  it('con fotos manda la foto', () => {
    expect(postVariant({ media: [{ id: 'm1' }] })).toBe(POST_VARIANT.MEDIA)
  })

  it('con varias fotos también', () => {
    expect(postVariant({ media: [{ id: 'm1' }, { id: 'm2' }] })).toBe(POST_VARIANT.MEDIA)
  })

  it('sin fotos manda el texto', () => {
    expect(postVariant({ content: 'hola' })).toBe(POST_VARIANT.TEXTO)
    expect(postVariant({ content: 'hola', media: [] })).toBe(POST_VARIANT.TEXTO)
  })

  it('un post sin media todavía así sigue siendo de texto', () => {
    // El backend siempre manda `media`, pero un post recién creado en el
    // cliente puede no tenerlo. Que no reviente ni devuelva indefinido.
    expect(postVariant({ content: 'hola', media: null })).toBe(POST_VARIANT.TEXTO)
    expect(postVariant(null)).toBe(POST_VARIANT.TEXTO)
  })
})
