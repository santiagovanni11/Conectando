import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import PasswordInput, { ASPECTO_CONTRASENA } from './PasswordInput'
import { ICONS } from './Icon/icons'

/**
 * El botón del ojo.
 *
 * Va en su propio archivo, y no escondido dentro del test de otro
 * formulario, porque lo que se rompe acá no es un formulario: es una
 * relación entre el ícono, el texto y el estado. Cuando viven separados,
 * se contradicen sin que ningún test lo note.
 */
function montar() {
  return render(<PasswordInput label="Contraseña" />)
}

const boton = () => screen.getByRole('button')
const campo = () => screen.getByLabelText('Contraseña')

/**
 * El `Icon` no pone el nombre del ícono en ninguna clase, así que se
 * compara el trazo que dibuja contra el del ícono esperado. Es lo único
 * que dice inequívocamente "este botón tiene el ojo tachado", y no
 * "este botón tiene un ícono".
 */
function dibuja(icono) {
  const esperado = ICONS[icono]
  const lista = Array.isArray(esperado) ? esperado : [esperado]
  const trazos = [...boton().querySelectorAll('path')].map((p) => p.getAttribute('d'))
  return trazos.length > 0 && trazos.every((t) => lista.includes(t))
}

const tachado = () => dibuja('eye-off')
const sinTacha = () => dibuja('eye')

beforeEach(() => {
  vi.clearAllMocks()
})

describe('PasswordInput: ícono del ojo', () => {
  it('arranca tachado, que es cómo está la contraseña', () => {
    montar()

    expect(campo()).toHaveAttribute('type', 'password')
    expect(tachado()).toBe(true)
  })

  it('se destapa cuando la contraseña se ve', async () => {
    // Regresión: el ícono estaba al revés. Se veía el ojo tachado
    // justamente cuando la contraseña estaba a la vista, y el ojo limpio
    // cuando seguía oculta.
    const user = userEvent.setup()
    montar()

    await user.click(boton())

    expect(campo()).toHaveAttribute('type', 'text')
    expect(sinTacha()).toBe(true)
  })

  it('vuelve a taparse al volver a ocultarla', async () => {
    const user = userEvent.setup()
    montar()

    await user.click(boton())
    await user.click(boton())

    expect(campo()).toHaveAttribute('type', 'password')
    expect(tachado()).toBe(true)
  })
})

describe('PasswordInput: la tabla de aspecto', () => {
  // El ícono y el texto salen de la misma fila justamente para que no
  // puedan contradecirse. Esta tabla es el contrato entre ambos.
  it('el ícono describe el estado y el texto la acción', () => {
    expect(ASPECTO_CONTRASENA.oculta).toEqual({
      icono: 'eye-off',
      accion: 'Mostrar contraseña',
    })
    expect(ASPECTO_CONTRASENA.visible).toEqual({
      icono: 'eye',
      accion: 'Ocultar contraseña',
    })
  })

  it('el texto siempre dice lo que va a pasar al tocar', async () => {
    // El ícono puede leerse como estado; el texto no puede, porque el
    // usuario no sabe si el ojo describe lo que ve o lo que va a pasar.
    const user = userEvent.setup()
    montar()

    expect(boton()).toHaveAccessibleName('Mostrar contraseña')
    await user.click(boton())
    expect(boton()).toHaveAccessibleName('Ocultar contraseña')
  })

  it('el botón avienta el estado a los lectores de pantalla', async () => {
    const user = userEvent.setup()
    montar()

    expect(boton()).toHaveAttribute('aria-pressed', 'false')
    await user.click(boton())
    expect(boton()).toHaveAttribute('aria-pressed', 'true')
  })
})
