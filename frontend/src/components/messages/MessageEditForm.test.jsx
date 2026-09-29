import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import MessageThread from './MessageThread'
import { CURRENT_USER_ID, message } from './messageTestFixtures'

const currentUserId = CURRENT_USER_ID

beforeEach(() => {
  vi.clearAllMocks()
})

/**
 * Monta un hilo con un mensaje propio y abre el formulario de edición.
 * `content` es el texto con el que se arma el mensaje.
 */
async function openEditor({ content = 'hola', ...props } = {}, user = userEvent.setup()) {
  render(
    <MessageThread
      messages={[message('m1', currentUserId, content)]}
      currentUserId={currentUserId}
      isLoading={false}
      onEdit={vi.fn()}
      {...props}
    />,
  )

  await user.click(screen.getByLabelText('Opciones del mensaje'))
  await user.click(screen.getByRole('menuitem', { name: 'Editar' }))

  return user
}

const editorInput = () => screen.getByLabelText('Editar mensaje')
const saveButton = () => screen.getByRole('button', { name: 'Guardar' })

describe('edición de un mensaje propio', () => {
  it('guarda la edición con el texto nuevo', async () => {
    // Regresión: al pulsar "Guardar" el mensaje no se actualizaba.
    const onEdit = vi.fn()
    const user = await openEditor({ onEdit })

    await user.clear(editorInput())
    await user.type(editorInput(), 'hola corregido')
    await user.click(saveButton())

    expect(onEdit).toHaveBeenCalledWith('m1', 'hola corregido')
  })

  it('el textarea arranca con el contenido actual del mensaje', async () => {
    await openEditor({ content: 'texto viejo' })

    expect(editorInput()).toHaveValue('texto viejo')
  })

  it('mantiene abierto el formulario si el servidor rechaza la edición', async () => {
    // Regresión: el formulario se cerraba siempre y el texto se perdía,
    // así que un rechazo del servidor parecía "no se guarda".
    const user = await openEditor({ onEdit: vi.fn().mockResolvedValue(false) })

    await user.clear(editorInput())
    await user.type(editorInput(), 'texto nuevo')
    await user.click(saveButton())

    // Sigue editable y con lo que se escribió.
    expect(editorInput()).toHaveValue('texto nuevo')
  })

  it('muestra el error junto a la edición, no escondido', async () => {
    const user = await openEditor({
      onEdit: vi.fn().mockResolvedValue(false),
      actionError: 'Solo se puede editar durante 15 minutos.',
    })

    await user.click(saveButton())

    expect(screen.getByRole('alert')).toHaveTextContent('15 minutos')
  })

  it('cierra el formulario y vuelve a la burbuja cuando sí se guardó', async () => {
    const user = await openEditor({ onEdit: vi.fn().mockResolvedValue(true) })

    await user.click(saveButton())

    expect(screen.queryByLabelText('Editar mensaje')).toBeNull()
    expect(screen.getByText('hola')).toBeInTheDocument()
  })
})