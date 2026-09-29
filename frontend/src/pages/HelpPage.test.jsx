import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import HelpPage from './HelpPage'
import { SUPPORT_EMAIL } from '../constants/support'

function renderPage() {
  return render(
    <MemoryRouter>
      <HelpPage />
    </MemoryRouter>,
  )
}

describe('HelpPage', () => {
  it('muestra el correo de contacto', () => {
    renderPage()

    expect(screen.getByText(SUPPORT_EMAIL)).toBeInTheDocument()
  })

  it('el correo abre el cliente de correo con el enlace completo', () => {
    renderPage()

    const links = screen.getAllByRole('link')
    for (const link of links) {
      expect(link).toHaveAttribute('href', expect.stringContaining(`mailto:${SUPPORT_EMAIL}`))
    }
  })

  it('no ofrece un botón aparte: el correo es el único contacto', () => {
    renderPage()

    // Se pidió sacar el botón: queda solo el correo, como enlace.
    expect(screen.queryByRole('link', { name: /escribir por email/i })).toBeNull()
    expect(screen.getAllByRole('link')).toHaveLength(1)
  })

  it('dice que se intentará resolver el problema', () => {
    renderPage()

    expect(screen.getByText(/resol/i)).toBeInTheDocument()
  })

  it('el enlace del correo se puede copiar: es texto, no una imagen', () => {
    renderPage()

    expect(screen.getByText(SUPPORT_EMAIL).tagName).toBe('A')
  })
})