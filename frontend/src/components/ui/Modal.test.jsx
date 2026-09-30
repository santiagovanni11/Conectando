import { describe, it, expect, vi } from 'vitest'
import { render } from '@testing-library/react'
import Modal from './Modal'

/**
 * Dónde se dibuja el modal.
 *
 * El compositor de publicaciones es un modal y, al abrir el selector de fotos,
 * abre otro modal adentro. Renderizado donde le toca, el de adentro queda
 * dentro del `position: fixed` del de afuera y en iOS el panel interior se
 * dibuja mal: era lo que dejaba el selector de fotos en blanco.
 */
describe('Modal', () => {
  it('se dibuja contra el body y no donde lo usan', () => {
    const { container } = render(
      <div data-testid="anfitrion">
        <Modal title="Prueba" onClose={() => {}}>
          Contenido
        </Modal>
      </div>,
    )

    expect(container.querySelector('.modal')).toBeNull()
    expect(document.body.querySelector('.modal')).not.toBeNull()
  })

  it('el modal de adentro queda como hermano y no anidado', () => {
    // El que se abre después tiene que quedar arriba en el DOM. Anidados, el
    // orden visual depende del z-index y en iOS se rompe.
    render(
      <Modal title="Exterior" onClose={() => {}}>
        <Modal title="Interior" onClose={() => {}}>
          Adentro
        </Modal>
      </Modal>,
    )

    const modales = document.body.querySelectorAll('.modal')
    expect(modales).toHaveLength(2)
    expect(modales[0].contains(modales[1])).toBe(false)
  })

  it('cierra con Escape', () => {
    const onClose = vi.fn()
    render(
      <Modal title="Prueba" onClose={onClose}>
        Contenido
      </Modal>,
    )

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))

    expect(onClose).toHaveBeenCalled()
  })

  it('devuelve el scroll del body al cerrarse', () => {
    const { unmount } = render(
      <Modal title="Prueba" onClose={() => {}}>
        Contenido
      </Modal>,
    )

    expect(document.body.style.overflow).toBe('hidden')
    unmount()
    expect(document.body.style.overflow).not.toBe('hidden')
  })
})
