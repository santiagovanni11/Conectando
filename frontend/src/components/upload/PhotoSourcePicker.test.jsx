import { describe, it, expect, vi } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import PhotoSourcePicker from './PhotoSourcePicker'

/**
 * Los inputs de archivo de este selector.
 *
 * En iOS, la hoja de "Cámara / Fototeca / Buscar" aparece en blanco si el
 * input de archivo no está bien en pantalla. Pasaron dos formas de romperla:
 * con `hidden`, y con un input de un píxel que se disparaba con `.click()`
 * desde JavaScript mientras el modal seguía montado.
 *
 * Estas pruebas no pueden reproducir eso —es comportamiento del sistema—, así
 * que se afirma sobre lo que evita las dos: que el input sea real, que esté
 * montado sobre su opción y que no lo dispare JavaScript.
 */
const inputs = () => [
  screen.getByTestId('camera-input'),
  screen.getByTestId('gallery-input'),
]

const unaFoto = () =>
  fireEvent.change(screen.getByTestId('gallery-input'), {
    target: { files: [new File(['x'], 'foto.jpg', { type: 'image/jpeg' })] },
  })

/**
 * Abre el modal con `userEvent` y no con un `.click()` a pelo: React no vuelca
 * el cambio de estado hasta que la actualización está contenida en `act()`, y
 * sin eso el modal todavía no está en el DOM cuando se lo busca.
 */
const abrir = async () => {
  const user = userEvent.setup()
  await user.click(screen.getByText('Agregar foto'))
}

describe('PhotoSourcePicker', () => {
  it('los inputs se dibujan encima de su opción', async () => {
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)
    await abrir()

    for (const input of inputs()) {
      // Ni `hidden` ni de un píxel: en iOS, un input que no se ve bien hace que
      // la hoja salga en blanco.
      expect(input).not.toHaveAttribute('hidden')
      expect(input).toHaveClass('photo-source__input')
    }
  })

  it('el input es hijo de la opción y no un botón que lo dispara', async () => {
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)
    await abrir()

    for (const input of inputs()) {
      // Un input suelto al que se le llama .click() desde JavaScript es lo que
      // dejaba la hoja en blanco: iOS la presenta como respuesta a ese toque y
      // encuentra el modal todavía montado encima.
      expect(input.closest('.photo-source__option')).not.toBeNull()
    }
  })

  it('cada input tiene nombre propio', async () => {
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)
    await abrir()

    for (const input of inputs()) {
      expect(input).toHaveAccessibleName()
    }
  })

  it('abre el selector con las dos opciones', async () => {
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)
    await abrir()

    expect(screen.getByText('Tomar foto')).toBeInTheDocument()
    expect(screen.getByText('Elegir de la fototeca')).toBeInTheDocument()
  })

  it('el input de cámara pide captura y el de fototeca no', async () => {
    // Si los dos pidieran captura, en iOS los dos abrirían la cámara y no
    // habría forma de llegar a la fototeca.
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)
    await abrir()

    expect(screen.getByTestId('camera-input')).toHaveAttribute('capture')
    expect(screen.getByTestId('gallery-input')).not.toHaveAttribute('capture')
  })

  it('entrega los archivos elegidos', async () => {
    const onSelect = vi.fn()
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} onSelect={onSelect} />)
    await abrir()

    unaFoto()

    expect(onSelect).toHaveBeenCalledTimes(1)
    expect(onSelect.mock.calls[0][0][0].name).toBe('foto.jpg')
  })

  it('el modal se cierra recién cuando llega el archivo', async () => {
    // Si se cerrara al tocar la opción, el input se desmontaría mientras iOS
    // todavía está armando la hoja.
    render(<PhotoSourcePicker trigger={<span>Agregar foto</span>} />)
    await abrir()

    unaFoto()

    expect(screen.queryByText('Elegir de la fototeca')).not.toBeInTheDocument()
  })
})