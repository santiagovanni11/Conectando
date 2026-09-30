import { describe, it, expect, vi } from 'vitest'
import { readFileSync } from 'node:fs'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import BlockConfirmModal from './BlockConfirmModal'

const MODAL_CSS = 'src/styles/ui/modal.css'

function renderModal(props = {}) {
  const onConfirm = vi.fn()
  const onClose = vi.fn()
  render(
    <BlockConfirmModal
      userName="Ana"
      onConfirm={onConfirm}
      onClose={onClose}
      {...props}
    />,
  )
  return { onConfirm, onClose }
}

describe('BlockConfirmModal', () => {
  it('pregunta si estás seguro, con sí y no', () => {
    renderModal()

    expect(screen.getByText(/seguro que querés bloquear/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /no, cancelar/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /sí, bloquear/i })).toBeInTheDocument()
  })

  it('no bloquea si elegís no', async () => {
    const viewer = userEvent.setup()
    const { onConfirm, onClose } = renderModal()

    await viewer.click(screen.getByRole('button', { name: /no, cancelar/i }))

    expect(onConfirm).not.toHaveBeenCalled()
    expect(onClose).toHaveBeenCalled()
  })

  it('bloquea si confirmás', async () => {
    const viewer = userEvent.setup()
    const { onConfirm } = renderModal()

    await viewer.click(screen.getByRole('button', { name: /sí, bloquear/i }))

    expect(onConfirm).toHaveBeenCalled()
  })

  it('explica qué pasa al bloquear, para decidir con información', () => {
    renderModal()

    expect(screen.getByText(/dejar de ser tu amigo/i)).toBeInTheDocument()
    expect(screen.getByText(/no vas a ver sus publicaciones/i)).toBeInTheDocument()
    // Aclarar que es reversible evita que se bloquee por error.
    expect(screen.getByText(/podés desbloquearlo/i)).toBeInTheDocument()
  })

  it('no se puede confirmar dos veces mientras guarda', () => {
    renderModal({ busy: true })

    expect(screen.getByRole('button', { name: /sí, bloquear/i })).toBeDisabled()
  })

  it('el contenido va en un cuerpo con padding y scroll', () => {
    // Regresión: el contenido se pegaba al borde del panel y se cortaba
    // sin scroll cuando no entraba, sobre todo en pantallas bajas.
    render(<BlockConfirmModal userName="Ana" onClose={vi.fn()} />)

    // El modal se dibuja en un portal contra el body, no en el contenedor que
    // devuelve render(). Por eso se busca ahí y no en `container`.
    const body = document.body.querySelector('.modal__body')
    expect(body).toBeTruthy()
    // El texto está partido por el <strong>, así que se busca un fragmento.
    expect(body?.textContent).toMatch(/seguro que querés bloquear/i)
  })

  it('el modal reserva padding y scroll en su hoja de estilos', () => {
    // El CSS es la parte que se rompe en silencio: el test pasa igual
    // aunque el modal quede ilegible en un teléfono chico.
    const css = readFileSync(MODAL_CSS, 'utf8')

    expect(css).toMatch(/\.modal__body\s*\{[^}]*padding:/)
    expect(css).toMatch(/\.modal__body\s*\{[^}]*overflow-y:\s*auto/)
  })

  it('la cabecera no se aplasta cuando el contenido es largo', () => {
    const css = readFileSync(MODAL_CSS, 'utf8')

    // Sin flex-shrink la cabecera con el botón de cerrar se comprime.
    expect(css).toMatch(/\.modal__header\s*\{[^}]*flex-shrink:\s*0/)
  })
})