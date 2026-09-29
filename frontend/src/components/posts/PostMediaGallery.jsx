import { useRef, useState } from 'react'
import Icon from '../ui/Icon/Icon'
import { useMediaQuery } from '../../hooks/useMediaQuery'
import { BREAKPOINTS_MEDIA } from '../../constants/breakpoints'

export default function PostMediaGallery({ media, label = 'Fotos de la publicación', onDoubleClick }) {
  const isDesktop = useMediaQuery(BREAKPOINTS_MEDIA.md)
  const trackRef = useRef(null)
  const [index, setIndex] = useState(0)

  if (!media || media.length === 0) return null

  const isCarousel = media.length > 1

  function goTo(next) {
    const track = trackRef.current
    if (!track) return
    const target = Math.max(0, Math.min(media.length - 1, next))
    track.scrollTo({ left: target * track.clientWidth, behavior: 'smooth' })
    setIndex(target)
  }

  function handleScroll() {
    const track = trackRef.current
    if (!track) return
    setIndex(Math.round(track.scrollLeft / track.clientWidth))
  }

  return (
    <div className={`post-gallery ${isCarousel ? 'is-carousel' : 'is-single'}`} role="img" aria-label={label}>
      <div className="post-gallery__track" ref={trackRef} onScroll={handleScroll}>
        {media.map((item) => (
          <img
            key={item.id}
            className="post-gallery__item"
            src={item.url}
            alt=""
            loading="lazy"
            draggable="false"
            onDoubleClick={onDoubleClick}
          />
        ))}
      </div>

      {isCarousel && isDesktop && (
        <>
          <button
            type="button"
            className="post-gallery__arrow post-gallery__arrow--prev"
            onClick={() => goTo(index - 1)}
            disabled={index === 0}
            aria-label="Foto anterior"
          >
            <Icon name="chevron-left" size="md" />
          </button>
          <button
            type="button"
            className="post-gallery__arrow post-gallery__arrow--next"
            onClick={() => goTo(index + 1)}
            disabled={index === media.length - 1}
            aria-label="Foto siguiente"
          >
            <Icon name="chevron-right" size="md" />
          </button>
        </>
      )}

      {isCarousel && (
        <div className="post-gallery__dots" aria-label="Posición del carrusel">
          {media.map((item, i) => (
            <button
              key={item.id}
              type="button"
              className={`post-gallery__dot${i === index ? ' is-active' : ''}`}
              onClick={() => goTo(i)}
              aria-label={`Ir a la foto ${i + 1}`}
            />
          ))}
        </div>
      )}
    </div>
  )
}