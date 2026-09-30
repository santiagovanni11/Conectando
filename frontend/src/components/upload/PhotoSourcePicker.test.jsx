import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import PhotoSourcePicker from './PhotoSourcePicker'

/**
 * Los inputs de archivo de este selector.
 *
 * La prueba que importa es la primera. En iOS, un input de archivo con
 * `hidden` —o con `display: none`— hace que el menú de "Cámara / Fototeca /
 * Buscar" se abra en blanco: las opciones existen pero no se ven y no hay
 * forma de elegir. Es un comportamiento del sistema, así que ningún test de
 * interacción lo detecta: hay que afirmar sobre el atributo.
 */
const inputs = () => [
  screen.getByTestId('camera-input'),
  screen.getByTestId('gallery-input'),
]

describe('PhotoSourcePicker', () => {
  it('no oculta los inputs con el atributo hidden', () => {
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)

    for (const input of inputs()) {
      expect(input).not.toHaveAttribute('hidden')
    }
  })

  it('los inputs se tapan con la clase, no con hidden', () => {
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)

    for (const input of inputs()) {
      expect(input).toHaveClass('photo-source__input')
    }
  })

  it('saca los inputs del recorrido del teclado', () => {
    // Recortados pero no ocultos: sin esto el teclado deja el foco en algo
    // que no se ve, y quien navega sin mouse se queda sin saber dónde está.
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)

    for (const input of inputs()) {
      expect(input).toHaveAttribute('tabindex', '-1')
    }
  })

  it('abre el selector con las dos opciones', async () => {
    const user = userEvent.setup()
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)

    await user.click(screen.getByText('Agregar foto'))

    expect(screen.getByRole('button', { name: /tomar foto/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /fototeca/i })).toBeInTheDocument()
  })

  it('entrega los archivos elegidos', async () => {
    const onSelect = vi.fn()
    const user = userEvent.setup()
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} onSelect={onSelect} />)

    await user.click(screen.getByText('Agregar foto'))
    await user.upload(
      screen.getByTestId('gallery-input'),
      new File(['x'], 'foto.jpg', { type: 'image/jpeg' }),
    )

    expect(onSelect).toHaveBeenCalledTimes(1)
    expect(onSelect.mock.calls[0][0][0].name).toBe('foto.jpg')
  })

  it('el input de cámara pide captura y el de fototeca no', () => {
    // Si los dos pidieran captura, en iOS los dos abrirían la cámara y no
    // habría forma de llegar a la fototeca.
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)

    expect(screen.getByTestId('camera-input')).toHaveAttribute('capture')
    expect(screen.getByTestId('gallery-input')).not.toHaveAttribute('capture')
  })
})