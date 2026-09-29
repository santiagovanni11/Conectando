/**
 * Andamiaje compartido de los tests de los formularios de cuenta.
 *
 * Vive aparte porque los necesitan dos archivos: el del cambio de clave y
 * el del botón del ojo. Duplicar el llenado de formularios significaba que
 * cambiar una etiqueta obligaba a tocarlos a los dos.
 */
import { screen } from '@testing-library/react'

/** El label lleva un " *" de required, así que se busca por patrón. */
export const campo = (nombre) => screen.getByLabelText(new RegExp(`^${nombre}`))

export const llenar = async (user, labels) => {
  for (const [label, value] of Object.entries(labels)) {
    await user.clear(campo(label))
    if (value) await user.type(campo(label), value)
  }
}

/** Llena el formulario con datos válidos y aprieta "cambiar contraseña". */
export const enviarCambioValido = async (user) => {
  await llenar(user, {
    'Contraseña actual': 'Vieja1',
    'Contraseña nueva': 'NuevaClave9',
    'Repetí la contraseña nueva': 'NuevaClave9',
  })
  await user.click(screen.getByRole('button', { name: /cambiar contraseña/i }))
}
