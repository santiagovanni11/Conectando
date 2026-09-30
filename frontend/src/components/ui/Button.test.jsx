import { render, screen } from '@testing-library/react'
import Button from './Button'
import { readFileSync } from 'node:fs'

/**
 * El botón de la casa.
 *
 * Se prueban las clases que decide y no el ancho en píxeles: jsdom no calcula
 * layout, así que una aserción de "100%" acá no probaría nada.
 */
describe('Button', () => {
  it('sin block se queda con el ancho de su contenido', () => {
    render(<Button>Entrar</Button>)

    expect(screen.getByRole('button').className).not.toContain('btn--block')
  })

  it('con block ocupa todo el ancho', () => {
    // Regresión: el prop existía en 18 lugares de la app y no hacía nada,
    // porque el componente lo pasaba al <button> en vez de aplicarlo. El
    // botón principal de una pantalla de login quedaba del tamaño de su
    // etiqueta, pegado al texto.
    render(<Button block>Entrar</Button>)

    expect(screen.getByRole('button')).toHaveClass('btn--block')
  })

  it('block no llega como atributo al botón', () => {
    render(<Button block>Entrar</Button>)

    expect(screen.getByRole('button')).not.toHaveAttribute('block')
  })

  it('la clase de ancho completo existe en los estilos', () => {
    // El componente sin su regla CSS es el mismo bug por otro lado.
    const css = readFileSync('src/styles/ui/buttons.css', 'utf8')

    expect(css).toMatch(/\.btn--block\s*\{[^}]*width:\s*100%/)
  })

  it('mantiene la variante y el tamaño', () => {
    render(<Button variant="ghost" size="lg" block>Cancelar</Button>)

    expect(screen.getByRole('button').className).toContain('btn--ghost')
    expect(screen.getByRole('button').className).toContain('btn--lg')
  })
})
