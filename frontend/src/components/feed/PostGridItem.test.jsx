import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import PostGridItem from './PostGridItem'

function renderItem(post) {
  return render(
    <MemoryRouter>
      <PostGridItem post={post} />
    </MemoryRouter>,
  )
}

const PHOTO_POST = {
  id: 'p1',
  media: [{ id: 'm1', url: 'http://x/1.jpg' }],
  content: 'con foto',
  likesCount: 3,
  commentsCount: 2,
}

const TEXT_POST = {
  id: 'p2',
  media: [],
  content: 'hola',
  likesCount: 0,
  commentsCount: 0,
}

describe('PostGridItem', () => {
  it('muestra la foto cuando el post tiene media', () => {
    const { container } = renderItem(PHOTO_POST)

    expect(container.querySelector('.feed-grid__thumb')).toBeInTheDocument()
    expect(container.querySelector('.feed-grid__item--text')).not.toBeInTheDocument()
  })

  // Regresión: los posts de solo texto se veían como un cuadrado con una letra.
  it('muestra el texto real cuando no hay media', () => {
    const { container } = renderItem(TEXT_POST)

    expect(container.querySelector('.feed-grid__preview-text')).toHaveTextContent('hola')
    expect(container.querySelector('.feed-grid__item--text')).toBeInTheDocument()
    expect(container.querySelector('img')).not.toBeInTheDocument()
  })

  it('muestra el texto completo, sin recortar', () => {
    const largo = 'palabra '.repeat(200).trim()
    const { container } = renderItem({ ...TEXT_POST, content: largo })

    // La tarjeta crece con el texto: no se recorta ni se agrega "...".
    const preview = container.querySelector('.feed-grid__preview-text')
    expect(preview.textContent).toBe(largo)
    expect(preview.textContent).not.toContain('…')
  })

  it('marca el carrusel cuando hay varias fotos', () => {
    const { container } = renderItem({
      ...PHOTO_POST,
      media: [{ id: 'm1', url: 'a.jpg' }, { id: 'm2', url: 'b.jpg' }],
    })

    expect(container.querySelector('.feed-grid__badge--carousel')).toBeInTheDocument()
  })

  it('enlaza al detalle del post', () => {
    renderItem(TEXT_POST)
    expect(screen.getByRole('link')).toHaveAttribute('href', '/posts/p2')
  })
})