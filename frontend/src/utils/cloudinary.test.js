import { describe, it, expect } from 'vitest'
import { avatarUrl, isCustomFraming, MAX_ZOOM } from './cloudinary'

/**
 * La URL que Cloudinary devuelve al subir una foto.
 * El formato es /upload/<public_id>.<ext> o /upload/v1234/<public_id>.<ext>
 */
const URL_CLOUDINARY = 'https://res.cloudinary.com/demo/image/upload/posts/abc123.jpg'
const URL_COMUN = 'https://otro-sitio.com/foto.jpg'

describe('avatarUrl', () => {
  it('sin encuadre devuelve la foto tal cual', () => {
    expect(avatarUrl(URL_CLOUDINARY, null)).toBe(URL_CLOUDINARY)
    expect(avatarUrl(URL_CLOUDINARY, { zoom: 1, offsetX: 0, offsetY: 0 })).toBe(URL_CLOUDINARY)
  })

  it('con zoom agrega el recorte de Cloudinary', () => {
    const url = avatarUrl(URL_CLOUDINARY, { zoom: 2, offsetX: 0, offsetY: 0 })

    expect(url).toContain('/upload/c-crop,')
    expect(url).toContain('w_400,h_400')
    // Acercar achica la región que se recorta: por eso el lado baja.
    expect(url).toContain('w_200,h_200')
  })

  it('con desplazamiento mueve el origen del recorte', () => {
    const url = avatarUrl(URL_CLOUDINARY, { zoom: 1, offsetX: 40, offsetY: -25 })

    expect(url).toContain('x_40')
    expect(url).toContain('y_-25')
  })

  it('redondea a enteros', () => {
    // Un offset con decimales rompería la URL de Cloudinary.
    const url = avatarUrl(URL_CLOUDINARY, { zoom: 1, offsetX: 12.6, offsetY: -3.2 })

    expect(url).toContain('x_13')
    expect(url).toContain('y_-3')
  })

  it('no deja pasar un zoom absurdo', () => {
    // Con un zoom de 999 Cloudinary pediría una región de menos de un píxel.
    const exagerado = avatarUrl(URL_CLOUDINARY, { zoom: 999 })
    const maximo = avatarUrl(URL_CLOUDINARY, { zoom: MAX_ZOOM })

    expect(exagerado).toBe(maximo)
  })

  it('no toca una URL que no es de Cloudinary', () => {
    // Si el usuario puso una URL de otro lado, inventarle una transformación
    // la rompe.
    expect(avatarUrl(URL_COMUN, { zoom: 2, offsetX: 10, offsetY: 10 })).toBe(URL_COMUN)
  })

  it('sin foto devuelve null', () => {
    expect(avatarUrl(null, { zoom: 2 })).toBeNull()
    expect(avatarUrl(undefined, null)).toBeNull()
  })
})

describe('isCustomFraming', () => {
  it('reconoce el encuadre de fábrica', () => {
    expect(isCustomFraming(null)).toBe(false)
    expect(isCustomFraming({ zoom: 1, offsetX: 0, offsetY: 0 })).toBe(false)
  })

  it('reconoce un encuadre tocado', () => {
    expect(isCustomFraming({ zoom: 1, offsetX: 5, offsetY: 0 })).toBe(true)
    expect(isCustomFraming({ zoom: 1.5, offsetX: 0, offsetY: 0 })).toBe(true)
  })
})
