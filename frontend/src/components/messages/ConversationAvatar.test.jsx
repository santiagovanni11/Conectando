import { describe, it, expect } from 'vitest'
import { render } from '@testing-library/react'
import ConversationAvatar from './ConversationAvatar'

/**
 * La imagen del avatar va con alt vacío, o sea decorativa: el nombre ya
 * está escrito al lado. Por eso se busca en el DOM y no por rol "img",
 * que para un lector de pantalla directamente no existe.
 */
const fotosDe = (container) => container.querySelectorAll('img')

/** Conversación directa, que es la única que hay hoy. */
function direct(peer) {
  return { id: 'c1', peers: [peer] }
}

const ana = { id: 'u1', displayName: 'Ana Pérez', profileImageUrl: 'https://cdn/ana.jpg' }
const bruno = { id: 'u2', displayName: 'Bruno Díaz' }

describe('ConversationAvatar', () => {
  it('muestra la foto cuando el usuario tiene una', () => {
    const { container } = render(<ConversationAvatar conversation={direct(ana)} />)

    const [foto] = fotosDe(container)
    expect(foto).toHaveAttribute('src', 'https://cdn/ana.jpg')
  })

  it('cae a las iniciales cuando no hay foto', () => {
    // Lo que se veía antes de esta función: una letra suelta. Ahora sale
    // el mismo componente Avatar que usa el resto de la app.
    const { container } = render(<ConversationAvatar conversation={direct(bruno)} />)

    expect(fotosDe(container)).toHaveLength(0)
    expect(container.querySelector('.avatar__initials')).toHaveTextContent('BD')
  })

  it('no rompe si la conversación llega sin participantes', () => {
    const { container } = render(<ConversationAvatar conversation={{ id: 'c1', peers: [] }} />)

    expect(fotosDe(container)).toHaveLength(0)
  })

  it('no rompe si la conversación no trae la lista de peers', () => {
    const { container } = render(<ConversationAvatar conversation={{ id: 'c1' }} />)

    expect(fotosDe(container)).toHaveLength(0)
  })

  it('en un grupo muestra a lo sumo dos avatares', () => {
    // El modelo ya habla en plural, así que el día que haya grupos esto ya
    // está contemplado. Recortar a dos es lo que evita un collage de seis.
    const { container } = render(
      <ConversationAvatar
        conversation={{
          id: 'c1',
          peers: [ana, bruno, { id: 'u3', displayName: 'Carla Soto' }],
        }}
      />,
    )

    // Ana trae foto y Bruno no: uno cae a iniciales, y Carla se queda
    // afuera. Dos avatares, ni uno más.
    expect(container.querySelectorAll('.avatar')).toHaveLength(2)
    expect(fotosDe(container)).toHaveLength(1)
    expect(container.textContent).not.toContain('CS')
  })

  it('la foto es decorativa porque el nombre ya está al lado', () => {
    // Una imagen con alt propio haría que un lector de pantalla leyera el
    // nombre dos veces: una en el alt y otra en el texto de al lado.
    const { container } = render(<ConversationAvatar conversation={direct(ana)} />)

    expect(fotosDe(container)[0]).toHaveAttribute('alt', '')
  })
})
