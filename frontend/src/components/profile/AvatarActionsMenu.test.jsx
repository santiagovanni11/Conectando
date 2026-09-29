import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import AvatarActionsMenu from './AvatarActionsMenu'

const onClose = vi.fn()
const onNewPhoto = vi.fn()
const onAdjust = vi.fn()

beforeEach(() => vi.clearAllMocks())

const abrir = (props = {}) =>
  render(
    <AvatarActionsMenu
      open
      onClose={onClose}
      onNewPhoto={onNewPhoto}
      onAdjust={onAdjust}
      canAdjust
      {...props}
    />,
  )

describe('AvatarActionsMenu', () => {
  it('ofrece nueva foto y ajustar', async () => {
    const user = userEvent.setup()
    abrir()

    expect(screen.getByRole('menu', { name: /foto de perfil/i })).toBeTruthy()
    expect(screen.getByRole('menuitem', { name: /nueva foto/i })).toBeTruthy()
    expect(screen.getByRole('menuitem', { name: /ajustar foto/i })).toBeTruthy()

    await user.click(screen.getByRole('menuitem', { name: /ajustar foto/i }))

    expect(onAdjust).toHaveBeenCalled()
  })

  it('cierra antes de ejecutar la acción', async () => {
    // Si no cerrara primero, el menú seguiría abierto debajo del selector de
    // fotos y taparía lo que se acaba de abrir.
    const user = userEvent.setup()
    abrir()

    await user.click(screen.getByRole('menuitem', { name: /nueva foto/i }))

    expect(onClose).toHaveBeenCalled()
    expect(onNewPhoto).toHaveBeenCalled()
  })

  it('oculta ajustar cuando no hay foto', () => {
    abrir({ canAdjust: false })

    expect(screen.getByRole('menuitem', { name: /nueva foto/i })).toBeTruthy()
    expect(screen.queryByRole('menuitem', { name: /ajustar foto/i })).toBeNull()
  })

  it('se cierra con Escape', async () => {
    const user = userEvent.setup()
    abrir()

    await user.keyboard('{Escape}')

    expect(onClose).toHaveBeenCalled()
  })

  it('no muestra nada cuando está cerrado', () => {
    render(
      <AvatarActionsMenu
        open={false}
        onClose={onClose}
        onNewPhoto={onNewPhoto}
        onAdjust={onAdjust}
        canAdjust
      />,
    )

    expect(screen.queryByRole('menu')).toBeNull()
  })
})
