import { describe, it, expect, vi, beforeEach } from 'vitest'
import { readFileSync } from 'node:fs'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import ProfileTabs from './ProfileTabs'

const CSS = 'src/styles/profile/profile-tabs.css'

const scrollIntoView = vi.fn()

beforeEach(() => {
  vi.clearAllMocks()
  // jsdom no implementa scrollIntoView: sin esto el efecto revienta.
  Element.prototype.scrollIntoView = scrollIntoView
})

function renderTabs(active = 'posts') {
  const onChange = vi.fn()
  render(<ProfileTabs active={active} onChange={onChange} />)
  return onChange
}

describe('ProfileTabs', () => {
  it('trae la pestaña activa a la vista al montar', () => {
    // Sin esto, entrar directo a "Cuenta" en un celular deja la pantalla
    // mostrando otras pestañas: la activa es la última y queda afuera.
    renderTabs('account')

    expect(scrollIntoView).toHaveBeenCalledTimes(1)
  })

  it('no mueve la página en vertical al traer la pestaña', () => {
    // `block: 'nearest'` es lo que evita que salte el scroll de la página
    // entera cada vez que se cambia de pestaña.
    renderTabs('saved')

    expect(scrollIntoView).toHaveBeenCalledWith({
      block: 'nearest',
      inline: 'center',
    })
  })

  it('vuelve a traer la vista cuando cambia la pestaña activa', () => {
    const { rerender } = render(<ProfileTabs active="posts" onChange={vi.fn()} />)
    scrollIntoView.mockClear()

    rerender(<ProfileTabs active="account" onChange={vi.fn()} />)

    expect(scrollIntoView).toHaveBeenCalled()
  })

  it('avisa el cambio de pestaña', async () => {
    const onChange = renderTabs('posts')
    const user = userEvent.setup()

    await user.click(screen.getByRole('tab', { name: 'Cuenta' }))

    expect(onChange).toHaveBeenCalledWith('account')
  })

  it('marca la activa con aria-selected', () => {
    renderTabs('account')

    expect(screen.getByRole('tab', { name: 'Cuenta' })).toHaveAttribute(
      'aria-selected',
      'true',
    )
  })

  it('la fila scrollea en horizontal y no arrastra la página entera', () => {
    // El problema original: cuatro pestañas de ~600 px contra los ~343 px
    // útiles de un iPhone, sin scroll, dejaban "Cuenta" entera afuera.
    const css = readFileSync(CSS, 'utf8')
    const fila = css.match(/\.profile-tabs\s*\{[^}]*\}/)?.[0] ?? ''

    expect(fila).toMatch(/overflow-x:\s*auto/)
    // Sin esto la barra horizontal del body permite mover toda la pantalla
    // de lado, que es lo que más se siente mal en un celular.
    expect(fila).toMatch(/overscroll-behavior-x:\s*contain/)
  })

  it('las pestañas no se encogen ni parten el texto', () => {
    const css = readFileSync(CSS, 'utf8')
    const tab = css.match(/\.profile-tabs__tab\s*\{[^}]*\}/)?.[0] ?? ''

    // Sin flex: none la pestaña se estira para "entrar" en la pantalla y
    // el texto se parte a media palabra.
    expect(tab).toMatch(/flex:\s*0 0 auto/)
    expect(tab).toMatch(/white-space:\s*nowrap/)
  })

  it('la pestaña tiene zona cómoda para el dedo', () => {
    // Un <button> pelado queda por debajo de los 44px recomendados.
    const css = readFileSync(CSS, 'utf8')
    const tab = css.match(/\.profile-tabs__tab\s*\{[^}]*\}/)?.[0] ?? ''

    expect(tab).toMatch(/min-height:\s*var\(--tap-target\)/)
  })

  it('en pantallas anchas no queda scroll, como antes', () => {
    const css = readFileSync(CSS, 'utf8')
    const ancho = css.match(/@media \(min-width: 48rem\)[\s\S]*?\}/)?.[0] ?? ''

    expect(ancho).toMatch(/overflow-x:\s*visible/)
  })
})
